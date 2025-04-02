import { Component, OnDestroy, OnInit } from '@angular/core';
import { EventService } from '../../../services/event/event.service';
import { EventListDto } from '../../../services/event/event.types';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { NgForOf, NgIf } from '@angular/common';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { UserService } from '../../../services/user/user.service';
import { UserGroup } from '../../../globals/userGroups';
import { formatLithuanianDate } from '../../../helpers/formatLithuanianDate';
import { EventBadgeComponent } from '../../../components/event-badge/event-badge.component';
import { PagedDataTable } from '../../../components/paged-table/paged-table.types';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime, takeUntil, tap } from 'rxjs';

const EVENT_TABLE_PAGE_SIZE = 20;

@Component({
  selector: 'app-event-list',
  imports: [
    ButtonComponent,
    NgForOf,
    NgIf,
    EventBadgeComponent,
    InputFieldComponent,
    ReactiveFormsModule,
  ],
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss',
})
export class EventListComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  protected buttonType = ButtonType;
  protected buttonIcon = SvgIconSrc;

  protected eventsTableData: PagedDataTable<string, EventListDto>;
  protected searchControl: FormControl = new FormControl('', [
    Validators.max(100),
  ]);
  protected showCreateEvent = false;

  constructor(
    private eventService: EventService,
    private userService: UserService,
  ) {
    super();
    this.eventsTableData = new PagedDataTable<string, EventListDto>(
      (searchTerm, keyOffset, pageSize) => {
        return this.eventService.searchEvents(searchTerm, keyOffset, pageSize);
      },
      (item) => item.name,
      '',
      EVENT_TABLE_PAGE_SIZE,
    );
  }

  ngOnInit() {
    this.userService.userGroupsCallback((groups) => {
      this.updateViewPermissions(groups);
    });
    this.initializeSearch();
  }

  protected formatDate(dateString: string) {
    return formatLithuanianDate(new Date(dateString));
  }

  protected previousPage() {
    this.eventsTableData.loadPreviousPage();
  }

  protected nextPage() {
    this.eventsTableData.loadNextPage();
  }

  private initializeSearch() {
    this.eventsTableData.initialize();
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        tap((value) => {
          this.eventsTableData.setSearchTerm(value);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private updateViewPermissions(groups: UserGroup[]) {
    this.showCreateEvent = groups.includes(UserGroup.EventAdmin);
  }
}
