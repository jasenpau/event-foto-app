import { Component, OnDestroy } from '@angular/core';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { ButtonComponent } from '../../../components/actions/button/button.component';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ButtonType } from '../../../components/actions/button/button.types';
import { AuthService } from '../../../services/auth/auth.service';
import { LoginRequestDto } from '../../../services/auth/auth.types';
import { Subject, takeUntil, tap } from 'rxjs';
import { AppError } from '../../../globals/errors';
import { Router } from '@angular/router';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [InputFieldComponent, ButtonComponent, ReactiveFormsModule, NgIf],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent implements OnDestroy {
  private destroy$: Subject<void> = new Subject<void>();

  buttonType = ButtonType.Wide;
  form: FormGroup;
  loginError: string | null = null;

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {
    this.form = new FormGroup({
      email: new FormControl('', []),
      password: new FormControl('', []),
    });
  }

  login = () => {
    const loginRequest: LoginRequestDto = this.form.value;
    this.authService
      .login(loginRequest)
      .pipe(
        tap((result) => {
          if (result.success) {
            this.router.navigate(['/']);
            this.loginError = null;
          } else if (result.error === AppError.InvalidCredentials) {
            this.form.patchValue({ password: '' });
            this.loginError = 'Neteisingas el. pašto adresas arba slaptažodis.';
          } else {
            this.form.patchValue({ password: '' });
            this.loginError = 'Nenumatyta klaida.';
          }
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  };

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
