import { Component, OnDestroy, OnInit } from '@angular/core';
import { EventService } from '../../../services/event/event.service';
import {
  EventListDto,
  EventSearchParamsDto,
} from '../../../services/event/event.types';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { NgForOf, NgIf } from '@angular/common';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { UserService } from '../../../services/user/user.service';
import { UserGroup } from '../../../globals/userGroups';
import { formatLithuanianDate } from '../../../helpers/formatLithuanianDate';
import { EventBadgeComponent } from '../../../components/event-badge/event-badge.component';
import { PagedDataTable } from '../../../components/paged-table/paged-table';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { debounceTime, takeUntil, tap } from 'rxjs';
import { DatePickerComponent } from '../../../components/forms/date-picker/date-picker.component';
import { PagedDataLoader } from '../../../components/paged-table/paged-table.types';
import { getStartOfDay } from '../../../helpers/getStartOfDay';
import { CheckboxComponent } from '../../../components/forms/checkbox/checkbox.component';
import { SpinnerComponent } from '../../../components/spinner/spinner.component';

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
    DatePickerComponent,
    CheckboxComponent,
    SpinnerComponent,
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
  protected searchForm: FormGroup;
  protected showCreateEvent = false;
  protected minFromDate: Date = new Date(0);

  constructor(
    private eventService: EventService,
    private userService: UserService,
  ) {
    super();
    this.eventsTableData = new PagedDataTable<string, EventListDto>(
      (searchTerm, keyOffset, pageSize) => {
        return this.searchEvents(searchTerm, keyOffset, pageSize);
      },
      (item) => `${item.startDate}|${item.id}`,
      '',
      EVENT_TABLE_PAGE_SIZE,
    );
    this.searchForm = new FormGroup({
      search: new FormControl(null, [Validators.maxLength(100)]),
      fromDate: new FormControl(getStartOfDay(new Date()), []),
      toDate: new FormControl(null, []),
      showArchived: new FormControl(null, []),
    });
  }

  ngOnInit() {
    this.updateViewPermissions();
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
    this.searchForm.valueChanges
      .pipe(
        debounceTime(300),
        tap((value) => {
          if (value['fromDate'] instanceof Date) {
            this.minFromDate = value['fromDate'];
            const toDate = this.searchForm.value['toDate'];
            if (toDate instanceof Date && value['fromDate'] > toDate) {
              this.searchForm.patchValue({ toDate: null });
              return;
            }
          }

          const searchTerm: string | null = value['search'];
          this.eventsTableData.setSearchTerm(searchTerm);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private searchEvents: PagedDataLoader<string, EventListDto> = (
    searchTerm,
    keyOffset,
    pageSize,
  ) => {
    const formValue = this.searchForm.value;
    const searchParams: EventSearchParamsDto = {
      searchTerm,
      keyOffset,
      pageSize,
    };

    if (formValue['fromDate']) {
      searchParams['fromDate'] = new Date(formValue['fromDate']);
      searchParams['fromDate']?.setHours(0, 0, 0, 0);
    }
    if (formValue['toDate']) {
      searchParams['toDate'] = new Date(formValue['toDate']);
      searchParams['toDate'].setHours(23, 59, 59, 999);
    }
    if (formValue['showArchived']) {
      searchParams['showArchived'] = formValue['showArchived'];
    }

    return this.eventService.searchEvents(searchParams);
  };

  private updateViewPermissions() {
    const groups = this.userService.getUserGroups();
    this.showCreateEvent = groups.includes(UserGroup.EventAdmin);
  }
}
