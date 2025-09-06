export class User {
	public id: string;
	public username: string;
	public password: string;
	public email: string;
	public isMfaEnabled: boolean;
	constructor() {
		this.id = '';
		this.username = '';
		this.password = '';
		this.email = '';
		this.isMfaEnabled = false;
	}
}
