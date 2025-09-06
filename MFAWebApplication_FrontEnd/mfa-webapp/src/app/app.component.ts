import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BackgroundLoginComponent } from './elements/background-login/background-login.component';

@Component({
	selector: 'app-root',
	imports: [RouterOutlet, BackgroundLoginComponent],
	templateUrl: './app.component.html',
})
export class AppComponent {
	title = 'MFA';
}
