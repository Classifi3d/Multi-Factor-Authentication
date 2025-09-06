import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const challengeGuard: CanActivateFn = (route, state) => {
	const router = inject(Router);
	const challengeToken = localStorage.getItem('Challenge-Token');

	// return challengeToken ? true : router.createUrlTree(['/login']);
	return true;
};
