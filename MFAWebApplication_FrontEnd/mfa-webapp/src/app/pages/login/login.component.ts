import { Component } from '@angular/core';
import {
	Validators,
	ReactiveFormsModule,
	FormBuilder,
	FormGroup,
	FormsModule,
} from '@angular/forms';
import { Router } from '@angular/router';
import { User } from '../../objects/user.model';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { ApiService } from '../../objects/api.service';
@Component({
	selector: 'app-login',
	standalone: true,
	imports: [
		MatFormFieldModule,
		MatInputModule,
		MatButtonModule,
		MatIconModule,
		FormsModule,
		ReactiveFormsModule,
	],
	templateUrl: './login.component.html',
	styleUrl: './login.component.scss',
})
export class LoginComponent {
	public hide = true;
	public loginForm!: FormGroup;
	public loginResponse: string | undefined;
	constructor(
		private formBuilder: FormBuilder,
		private apiService: ApiService,
		private router: Router
	) {
		this.loginForm = this.formBuilder.group({
			email: ['', [Validators.required, Validators.email]],
			password: ['', [Validators.required, Validators.minLength(8)]],
		});
	}

	public ngOnInit(): void {
		localStorage.removeItem('OAuth-Token');
		localStorage.removeItem('Challange-Token');
	}

	public onSubmit(): void {
		if (this.loginForm.valid) {
			const user = new User();
			user.email = this.loginForm.value.email;
			user.password = this.loginForm.value.password;
			this.apiService.loginUser(user).subscribe({
				next: (res) => {
					const oAuthToken = res.token;
					if (!!oAuthToken) {
						console.log(oAuthToken);
						localStorage.setItem('OAuth-Token', oAuthToken);
						this.router.navigate(['/user-menu']);
					}
					const challengeToken = res.challengeId;
					if (!!challengeToken) {
						console.log(challengeToken);
						localStorage.setItem('Challange-Token', challengeToken);
						this.router.navigate(['/multi-factor-auth']);
					}
				},
				error: () => {
					console.log('Login Error!');
				},
			});
		}
	}

	public getEmailErrorMessage(): string {
		if (this.loginForm.get('email')?.hasError('required')) {
			return 'You must enter an email';
		}
		return this.loginForm.get('email')?.hasError('email')
			? 'Not a valid email'
			: '';
	}
	public getPasswordErrorMessage(): string {
		if (this.loginForm.get('password')?.hasError('required')) {
			return 'You must enter a password';
		}
		return this.loginForm.get('password')?.hasError('minLength')
			? ''
			: 'Password too short';
	}
}
