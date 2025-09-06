import { Component } from '@angular/core';
import { ApiService } from '../../objects/api.service';
import {
	FormGroup,
	FormBuilder,
	Validators,
	FormsModule,
	ReactiveFormsModule,
} from '@angular/forms';
import { Router } from '@angular/router';
import { User } from '../../objects/user.model';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
	selector: 'app-sign-up',
	imports: [
		MatFormFieldModule,
		MatInputModule,
		MatButtonModule,
		MatIconModule,
		FormsModule,
		ReactiveFormsModule,
	],
	templateUrl: './sign-up.component.html',
	styleUrl: './sign-up.component.scss',
})
export class SignUpComponent {
	hide = true;
	signUpForm: FormGroup;
	signUpResponse: string | undefined;

	constructor(
		private apiService: ApiService,
		private formBuilder: FormBuilder,
		private router: Router
	) {
		this.signUpForm = this.formBuilder.group({
			email: ['', [Validators.required, Validators.email]],
			password: ['', [Validators.required, Validators.minLength(8)]],
		});
	}

	public onSubmit(): void {
		console.log('SUBMIT!');
		if (this.signUpForm.valid) {
			console.log('SUBMIT!');

			const user = new User();
			user.username = this.signUpForm.value.username;
			user.email = this.signUpForm.value.email;
			user.password = this.signUpForm.value.password;
			console.log(user);
			this.apiService.signUpUser(user).subscribe({
				next: (res) => {
					console.log('SignUp Status: ' + res);
					this.router.navigate(['/login']);
				},
				error: (error) => {
					console.log('SignUp Error!');
				},
			});
		}
	}

	public getUsernameErrorMessage(): string {
		if (this.signUpForm.get('username')?.hasError('required')) {
			return 'You must enter a username';
		}
		return this.signUpForm.get('username')?.hasError('required')
			? 'Not a valid email'
			: '';
	}
	public getEmailErrorMessage(): string {
		if (this.signUpForm.get('email')?.hasError('required')) {
			return 'You must enter an email';
		}
		return this.signUpForm.get('email')?.hasError('email')
			? 'Not a valid email'
			: '';
	}
	public getPasswordErrorMessage(): string {
		if (this.signUpForm.get('password')?.hasError('required')) {
			return 'You must enter a password';
		}
		return this.signUpForm.get('password')?.hasError('minLength')
			? ''
			: 'Password too short';
	}
}
