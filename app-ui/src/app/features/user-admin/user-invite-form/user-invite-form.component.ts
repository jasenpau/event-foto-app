import { Component, EventEmitter, OnDestroy, Output } from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import { FormInputSectionComponent } from '../../../components/forms/form-input-section/form-input-section.component';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { NgForOf, NgIf } from '@angular/common';
import { LoaderOverlayComponent } from '../../../components/loader-overlay/loader-overlay.component';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { ButtonType } from '../../../components/button/button.types';
import { UserService } from '../../../services/user/user.service';
import { SnackbarService } from '../../../services/snackbar/snackbar.service';
import { invalidValues } from '../../../components/forms/validators/invalidValues';
import { useLocalLoader } from '../../../helpers/useLoader';
import { takeUntil, tap } from 'rxjs';
import { SnackbarType } from '../../../services/snackbar/snackbar.types';
import { handleApiError } from '../../../helpers/handleApiError';
import { SelectComponent } from '../../../components/forms/select/select.component';
import { EnvService } from '../../../services/environment/env.service';

@Component({
  selector: 'app-invite-user-form',
  imports: [
    ButtonComponent,
    FormInputSectionComponent,
    InputFieldComponent,
    ReactiveFormsModule,
    LoaderOverlayComponent,
    NgIf,
    NgForOf,
    SelectComponent,
  ],
  templateUrl: './user-invite-form.component.html',
  styleUrl: './user-invite-form.component.scss',
})
export class UserInviteFormComponent
  extends DisposableComponent
  implements OnDestroy
{
  @Output() formEvent = new EventEmitter<string>();

  protected readonly ButtonType = ButtonType;
  protected inviteUserForm?: FormGroup;
  protected existingEmails: string[] = [];
  protected isLoading = false;
  protected userGroups: { name: string; id: string }[] = [];

  constructor(
    private readonly userService: UserService,
    private readonly snackbarService: SnackbarService,
    private readonly envService: EnvService,
  ) {
    super();
    this.setupForm();
  }

  setupForm() {
    const groupConfig = this.envService.getConfig().groups;

    this.userGroups = [
      {
        name: 'Žiūrėtojas',
        id: '',
      },
      {
        name: 'Fotografas',
        id: groupConfig.photographers,
      },
      {
        name: 'Renginių administratorius',
        id: groupConfig.eventAdministrators,
      },
      {
        name: 'Sistemos administratorius',
        id: groupConfig.systemAdministrators,
      },
    ];

    this.inviteUserForm = new FormGroup({
      name: new FormControl('', [
        Validators.required,
        Validators.maxLength(255),
      ]),
      email: new FormControl('', [
        Validators.required,
        Validators.email,
        invalidValues(
          this.existingEmails,
          'Naudotojas su šiuo el. paštu jau įvestas sistemoje',
        ),
      ]),
      group: new FormControl('', []),
    });
  }

  onSubmit() {
    if (!this.inviteUserForm) return;

    this.inviteUserForm.markAllAsTouched();
    if (this.inviteUserForm.valid) {
      const formData = this.inviteUserForm.value;
      this.userService
        .inviteUser({
          name: formData.name,
          email: formData.email,
          groupAssignment: formData.group === '' ? null : formData.group,
        })
        .pipe(
          useLocalLoader((value) => (this.isLoading = value)),
          tap((userId) => {
            if (userId) {
              this.snackbarService.addSnackbar(
                SnackbarType.Success,
                'Naudotojas pakviestas',
              );
              this.formEvent.emit('invited');
            }
          }),
          handleApiError((error) => {
            if (error.status === 409 && error.title === 'email-exists') {
              this.addConflictingEmail(formData.email);
            }
          }),
          takeUntil(this.destroy$),
        )
        .subscribe();
    }
  }

  protected cancel() {
    this.formEvent.emit('cancel');
  }

  private addConflictingEmail(email: string) {
    this.existingEmails.push(email.trim());
    this.inviteUserForm?.controls['email'].updateValueAndValidity();
  }
}
