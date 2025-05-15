import { Component, OnDestroy, OnInit } from '@angular/core';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { ButtonComponent } from '../../../components/button/button.component';
import {
  ButtonSize,
  ButtonType,
} from '../../../components/button/button.types';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { UserService } from '../../../services/user/user.service';
import { takeUntil, tap } from 'rxjs';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { Router } from '@angular/router';
import { LoaderService } from '../../../services/loader/loader.service';
import { useLoader } from '../../../helpers/useLoader';

@Component({
  selector: 'app-register',
  imports: [InputFieldComponent, ButtonComponent, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  buttonType = ButtonType.Filled;
  buttonSize = ButtonSize.Wide;

  form: FormGroup;

  constructor(
    private userService: UserService,
    private router: Router,
    private loaderService: LoaderService,
  ) {
    super();
    this.form = new FormGroup({
      name: new FormControl({ value: '', disabled: true }, [
        Validators.required,
        Validators.maxLength(255),
      ]),
      email: new FormControl({ value: '', disabled: true }, [
        Validators.required,
        Validators.email,
      ]),
    });
  }

  ngOnInit() {
    this.loadFormData();
  }

  loadFormData() {
    const user = this.userService.getCurrentUserData();
    if (user) {
      this.form.patchValue({
        name: user.name,
        email: user.email,
      });
    }
  }

  register() {
    this.userService
      .register()
      .pipe(
        useLoader('register-loading', this.loaderService),
        tap(() => {
          this.router.navigate(['/']);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
