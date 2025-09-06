import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const oauthGuard: CanActivateFn = (route, state) => {
	const router = inject(Router);
	const token = localStorage.getItem('OAuth-Token');
	// return token ? true : router.createUrlTree(['/login']);
	return true;
};
