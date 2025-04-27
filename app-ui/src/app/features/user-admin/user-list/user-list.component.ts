import { Component, OnDestroy, OnInit } from '@angular/core';
import { PagedDataTable } from '../../../components/paged-table/paged-table';
import { AppGroupsDto, UserListDto } from '../../../services/user/user.types';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../../services/user/user.service';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { debounceTime, takeUntil, tap } from 'rxjs';
import { ButtonComponent } from '../../../components/button/button.component';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { NgForOf, NgIf } from '@angular/common';
import { ButtonType } from '../../../components/button/button.types';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { SpinnerComponent } from '../../../components/spinner/spinner.component';
import { PaginationControlsComponent } from '../../../components/pagination-controls/pagination-controls.component';
import { SideViewComponent } from '../../../components/side-view/side-view.component';
import { UserInviteFormComponent } from '../user-invite-form/user-invite-form.component';
import { formatLithuanianDate } from '../../../helpers/formatLithuanianDate';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';

const USER_TABLE_PAGE_SIZE = 20;

@Component({
  selector: 'app-user-list',
  imports: [
    ButtonComponent,
    InputFieldComponent,
    NgForOf,
    NgIf,
    ReactiveFormsModule,
    SpinnerComponent,
    PaginationControlsComponent,
    SideViewComponent,
    UserInviteFormComponent,
    PageHeaderComponent,
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss',
})
export class UserListComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  protected readonly buttonType = ButtonType;
  protected readonly buttonIcon = SvgIconSrc;

  protected userTableData: PagedDataTable<string, UserListDto>;
  protected searchControl = new FormControl('', [Validators.max(100)]);
  protected appGroups: AppGroupsDto;
  protected showUserInviteForm = false;

  constructor(private userService: UserService) {
    super();
    this.appGroups = this.userService.getAppGroups();
    this.userTableData = new PagedDataTable<string, UserListDto>(
      (searchTerm, keyOffset, pageSize) => {
        return this.userService.searchUsers(searchTerm, keyOffset, pageSize);
      },
      (item) => item.name,
      '',
      USER_TABLE_PAGE_SIZE,
    );
  }

  ngOnInit() {
    this.initializeSearch();
  }

  protected getUserGroupName(groupId?: string): string {
    if (!this.appGroups) return '';

    switch (groupId) {
      case undefined:
        return 'Žiūrėtojas';
      case this.appGroups.systemAdministrators:
        return 'Sistemos administratorius';
      case this.appGroups.eventAdministrators:
        return 'Renginių Administratorius';
      case this.appGroups.photographers:
        return 'Fotografas';
      default:
        return '';
    }
  }

  protected handleUserInviteFormEvent($event: string) {
    if ($event === 'invited' || $event === 'cancel') {
      this.showUserInviteForm = false;
      this.userTableData.setSearchTerm('');
    }
  }

  protected openInviteForm() {
    this.showUserInviteForm = true;
  }

  protected getInviteDate(dateString?: string) {
    if (dateString) {
      const date = new Date(dateString);
      date.setDate(date.getDate() + 7);
      return `Galioja iki ${formatLithuanianDate(date)}`;
    }

    return 'Nėra pakvietimo';
  }

  private initializeSearch() {
    this.userTableData.initialize();
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        tap((value) => {
          this.userTableData.setSearchTerm(value);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
