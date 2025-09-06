import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../../objects/api.service';
import { CommonModule } from '@angular/common';

@Component({
	selector: 'app-multi-factor-generate',
	imports: [CommonModule],
	templateUrl: './multi-factor-generate.component.html',
	styleUrl: './multi-factor-generate.component.scss',
})
export class MultiFactorGenerateComponent implements OnInit {
	public qrCodeImage: string | null = null;
	constructor(private apiService: ApiService, private router: Router) {}

	public async ngOnInit(): Promise<void> {
		await this.generateQrCode();
	}

	public async generateQrCode(): Promise<void> {
		await this.apiService.generateMfa().subscribe({
			next: (response: Blob) => {
				const reader = new FileReader();
				reader.onloadend = () => {
					this.qrCodeImage = reader.result as string;
				};

				// Convert the Blob to a Base64 URL
				reader.readAsDataURL(response);
			},
		});
	}
}
