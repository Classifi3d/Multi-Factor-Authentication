import { Component, OnInit } from '@angular/core';
import { User } from '../../objects/user.model';
import { ApiService } from '../../objects/api.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
	selector: 'app-user-menu',
	imports: [CommonModule, MatTooltipModule],
	templateUrl: './user-menu.component.html',
	styleUrl: './user-menu.component.scss',
})
export class UserMenuComponent implements OnInit {
	public userData: User = new User();
	public loading = true;
	public error: string | null = null;
	public isMfaEnabled: boolean = false;
	public oAuthToken: string | null = null;
	public challangeToken: string | null = null;
	public oAuthTokenSmall: string | null | undefined = null;

	constructor(private apiService: ApiService, private router: Router) {}

	public async ngOnInit(): Promise<void> {
		await this.fetchUserData();
	}

	public async fetchUserData(): Promise<void> {
		setTimeout(() => {
			this.apiService.getUserData().subscribe({
				next: (data: any) => {
					this.userData = data as User;
					this.oAuthToken = localStorage.getItem('OAuth-Token');
					this.challangeToken =
						localStorage.getItem('Challange-Token');
					this.oAuthTokenSmall =
						this.oAuthToken?.slice(0, this.oAuthToken.length / 4) +
						'...';
					this.isMfaEnabled = data.isMfaEnabled || false;
					this.loading = false;
				},
				error: (err) => {
					console.error('Error fetching user data: ', err);
					this.error = 'Failed to load user data.';
					this.loading = false;
				},
			});
		}, 200);
	}

	public logout(): void {
		localStorage.removeItem('OAuth-Token');
		localStorage.removeItem('Challange-Token');
		this.router.navigate(['/login']);
	}

	public enableMfa(): void {
		this.router.navigate(['/multi-factor-generate']);
	}

	public disableMfa(): void {
		this.apiService.disableMfa().subscribe({
			next: () => {
				console.log('MFA successfully disabled.');
				this.isMfaEnabled = false;
			},
			error: (error) => {
				console.error(
					'Error disabling multi-factor authentication:',
					error
				);
			},
		});
	}
}
