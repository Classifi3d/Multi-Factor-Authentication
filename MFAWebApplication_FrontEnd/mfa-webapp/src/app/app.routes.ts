import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { SignUpComponent } from './pages/sign-up/sign-up.component';
import { MultiFactorAuthComponent } from './pages/multi-factor-auth/multi-factor-auth.component';
import { MultiFactorGenerateComponent } from './pages/multi-factor-generate/multi-factor-generate.component';
import { UserMenuComponent } from './pages/user-menu/user-menu.component';
import { oauthGuard } from './guards/oauth.guard';
import { challengeGuard } from './guards/challenge.guard';

export const routes: Routes = [
	{ path: '', component: LoginComponent },
	{ path: 'login', component: LoginComponent },
	{ path: 'sign-up', component: SignUpComponent },
	{
		path: 'multi-factor-generate',
		component: MultiFactorGenerateComponent,
		canActivate: [oauthGuard],
	},
	{
		path: 'multi-factor-auth',
		component: MultiFactorAuthComponent,
		canActivate: [challengeGuard],
	},
	{
		path: 'user-menu',
		component: UserMenuComponent,
		canActivate: [oauthGuard],
	},
];
