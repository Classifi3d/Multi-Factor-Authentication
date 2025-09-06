import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from './user.model';
import { MfaVerificationDto } from './mfa-verification.model';

@Injectable({
	providedIn: 'root',
})
export class ApiService {
	private http = inject(HttpClient);
	private url = 'https://localhost:7077';
	private headers = new HttpHeaders({
		'Content-Type': 'application/json',
		'Access-Control-Allow-Origin': '*',
	});

	private logedInHeader = new HttpHeaders({
		'Content-Type': 'application/json',
		'Access-Control-Allow-Origin': '*',
		Authorization: `Bearer ${localStorage.getItem('OAuth-Token')}`,
	});

	private paramsUsers = new HttpParams();

	// ========== LOGIN ==========
	public loginUser(user: User): Observable<any> {
		return this.http.post<any>(`${this.url}/user/login`, user, {
			headers: this.headers,
		});
	}

	// ========== MFA ==========
	public verifyMfaCode(mfaVerification: MfaVerificationDto): Observable<any> {
		console.log(mfaVerification);
		return this.http.post<any>(
			`${this.url}/user/verify-mfa`,
			mfaVerification,
			{
				headers: this.headers,
			}
		);
	}

	public generateMfa(): Observable<Blob> {
		return this.http.post<Blob>(
			`${this.url}/user/enable-mfa`,
			{},
			{
				headers: this.logedInHeader,
				responseType: 'blob' as 'json', // Make sure the response type is Blob
			}
		);
	}

	public disableMfa(): Observable<any> {
		return this.http.post<any>(
			`${this.url}/user/disable-mfa`,
			{},
			{ headers: this.logedInHeader }
		);
	}

	// ========== SIGN UP ==========
	public signUpUser(user: User): Observable<void> {
		console.log(user);
		return this.http.post<void>(`${this.url}/user/sign-up`, user, {
			headers: this.headers,
		});
	}

	// ========== USER MENU ==========
	public getUserData(): Observable<any> {
		return this.http.get<any>(`${this.url}/user/user-data`, {
			headers: this.logedInHeader,
		});
	}
}
