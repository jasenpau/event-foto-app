import { Component } from '@angular/core';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { ButtonComponent } from '../../../components/actions/button/button.component';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ButtonType } from '../../../components/actions/button/button.types';

@Component({
  selector: 'app-login',
  imports: [InputFieldComponent, ButtonComponent, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  buttonType = ButtonType.Wide;

  form: FormGroup = new FormGroup({
    email: new FormControl('', []),
    password: new FormControl('', []),
  });
}
