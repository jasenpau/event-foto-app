import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { PagedDataTable } from '../../../components/paged-table/paged-table.types';
import { UserData } from '../../../services/user/user.types';
import { EventService } from '../../../services/event/event.service';
import { UserService } from '../../../services/user/user.service';
import { debounceTime, takeUntil, tap } from 'rxjs';
import { NgForOf, NgIf } from '@angular/common';

const USER_TABLE_PAGE_SIZE = 10;

@Component({
  selector: 'app-assign-photographer-form',
  imports: [
    InputFieldComponent,
    ButtonComponent,
    NgIf,
    NgForOf,
    ReactiveFormsModule,
  ],
  templateUrl: './assign-photographer-form.component.html',
  styleUrl: './assign-photographer-form.component.scss',
})
export class AssignPhotographerFormComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  @Input({ required: true }) eventId!: number;
  @Output() formEvent = new EventEmitter<string>();
  protected readonly ButtonType = ButtonType;

  protected userTableData: PagedDataTable<string, UserData>;
  protected searchControl = new FormControl('', [Validators.max(100)]);

  constructor(
    private eventService: EventService,
    private userService: UserService,
  ) {
    super();
    this.userTableData = new PagedDataTable<string, UserData>(
      (searchTerm, keyOffset, pageSize) => {
        return this.userService.searchUsers(
          searchTerm,
          keyOffset,
          pageSize,
          this.eventId,
        );
      },
      (item) => item.name,
      '',
      USER_TABLE_PAGE_SIZE,
    );
  }

  ngOnInit() {
    this.initializeSearch();
  }

  protected cancel() {
    this.formEvent.emit('cancel');
  }

  protected assignUser(id: string) {
    this.eventService
      .assignPhotographerToEvent(this.eventId, id)
      .pipe(
        tap(() => {
          this.formEvent.emit('assigned');
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
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
