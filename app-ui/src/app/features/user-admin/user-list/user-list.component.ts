import { Component, OnDestroy, OnInit } from '@angular/core';
import { PagedDataTable } from '../../../components/paged-table/paged-table';
import { AppGroupsDto, UserData } from '../../../services/user/user.types';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../../services/user/user.service';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { debounceTime, takeUntil, tap } from 'rxjs';
import { ButtonComponent } from '../../../components/button/button.component';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { NgForOf, NgIf } from '@angular/common';
import { ButtonType } from '../../../components/button/button.types';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';

const USER_TABLE_PAGE_SIZE = 20;

@Component({
  selector: 'app-user-list',
  imports: [
    ButtonComponent,
    InputFieldComponent,
    NgForOf,
    NgIf,
    ReactiveFormsModule,
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

  protected userTableData: PagedDataTable<string, UserData>;
  protected searchControl = new FormControl('', [Validators.max(100)]);
  protected appGroups?: AppGroupsDto;

  constructor(private userService: UserService) {
    super();
    this.userTableData = new PagedDataTable<string, UserData>(
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
    this.userService
      .getAppGroups()
      .pipe(
        tap((groups) => (this.appGroups = groups)),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  protected getUserGroupName(groupId: string): string {
    if (!this.appGroups) return '';

    switch (groupId) {
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

  protected previousPage() {
    this.userTableData.loadPreviousPage();
  }

  protected nextPage() {
    this.userTableData.loadNextPage();
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
