import { Component, OnDestroy, OnInit } from '@angular/core';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonSize, ButtonType } from '../../../components/button/button.types';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../services/auth/auth.service';
import { UserService } from '../../../services/user/user.service';
import { takeUntil, tap } from 'rxjs';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [InputFieldComponent, ButtonComponent, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent extends DisposableComponent implements OnInit, OnDestroy {
  buttonType = ButtonType.Filled;
  buttonSize = ButtonSize.Wide;

  form: FormGroup;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private router: Router,
  ) {
    super();
    this.form = new FormGroup({
      name: new FormControl('', [
        Validators.required,
        Validators.maxLength(40),
      ]),
      email: new FormControl('', [Validators.required, Validators.email]),
    });
  }

  ngOnInit() {
    this.loadFormData();
  }

  loadFormData() {
    const user = this.authService.getUserTokenData();
    if (user) {
      this.form.patchValue({
        name: user.name,
        email: user.email,
      });
    }
  }

  register() {
    this.form.markAllAsTouched();
    if (this.form.valid) {
      this.userService
        .register(this.form.value)
        .pipe(
          tap(() => {
            this.router.navigate(['/']);
          }),
          takeUntil(this.destroy$))
        .subscribe();
    }
  }
}
