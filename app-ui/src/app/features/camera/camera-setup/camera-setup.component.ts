import {
  Component,
  EventEmitter,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { NgForOf, NgIf } from '@angular/common';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ButtonComponent } from '../../../components/button/button.component';
import { SelectComponent } from '../../../components/forms/select/select.component';
import {
  saveCameraEvent,
  saveCameraDevice,
  requestCameraPermission,
  getVideoDevices,
  loadCameraDevice,
  loadCameraEvent,
} from '../cameraSettingsHelper';
import {
  EventListDto,
  EventSearchParamsDto,
} from '../../../services/event/event.types';
import { CameraDevice, CameraEvent, CameraSettings } from '../camera.types';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { EventService } from '../../../services/event/event.service';
import { ButtonType } from '../../../components/button/button.types';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { SideViewComponent } from '../../../components/side-view/side-view.component';
import { PagedDataTable } from '../../../components/paged-table/paged-table';
import { PagedDataLoader } from '../../../components/paged-table/paged-table.types';
import { formatLithuanianDate } from '../../../helpers/formatLithuanianDate';
import { debounceTime, takeUntil, tap } from 'rxjs';
import { invalidValues } from '../../../components/forms/validators/invalidValues';
import { PaginationControlsComponent } from '../../../components/pagination-controls/pagination-controls.component';
import { SpinnerComponent } from '../../../components/spinner/spinner.component';

const EVENT_TABLE_PAGE_SIZE = 20;

@Component({
  selector: 'app-camera-setup',
  imports: [
    NgForOf,
    FormsModule,
    ReactiveFormsModule,
    ButtonComponent,
    SelectComponent,
    NgIf,
    InputFieldComponent,
    SideViewComponent,
    PaginationControlsComponent,
    SpinnerComponent,
  ],
  templateUrl: './camera-setup.component.html',
  styleUrl: './camera-setup.component.scss',
})
export class CameraSetupComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  protected readonly ButtonType = ButtonType;

  @Output() settingsUpdated = new EventEmitter<CameraSettings>();

  protected cameraDevices: MediaDeviceInfo[] = [];
  protected showEventSelector = false;
  protected selectedEvent?: EventListDto;
  protected settingsForm?: FormGroup;
  protected searchControl: FormControl = new FormControl(null, [
    Validators.max(100),
  ]);
  protected eventsTableData: PagedDataTable<string, EventListDto>;

  constructor(private readonly eventService: EventService) {
    super();
    this.eventsTableData = new PagedDataTable<string, EventListDto>(
      (searchTerm, keyOffset, pageSize) => {
        return this.searchEvents(searchTerm, keyOffset, pageSize);
      },
      (item) => `${item.startDate}|${item.id}`,
      '',
      EVENT_TABLE_PAGE_SIZE,
    );
  }

  async ngOnInit() {
    this.initForm();
    await requestCameraPermission();
    await this.initCameraOptions();
  }

  saveSettings() {
    if (!this.settingsForm) return;
    this.settingsForm.updateValueAndValidity();
    this.settingsForm.markAllAsTouched();

    if (!this.settingsForm.valid) return;

    let settings: CameraSettings = {};

    if (this.settingsForm.value?.camera) {
      const deviceId = this.settingsForm.value.camera;
      const selectedCamera = this.cameraDevices.find(
        (d) => d.deviceId === deviceId,
      );
      if (!selectedCamera) {
        console.error(`selected device with id '${deviceId}' not found`);
      } else {
        const preferredCamera: CameraDevice = {
          deviceId,
          label: selectedCamera.label,
        };
        saveCameraDevice(preferredCamera);
        settings = { ...settings, device: preferredCamera };
      }
    }

    if (this.selectedEvent) {
      const cameraEvent: CameraEvent = {
        id: this.selectedEvent.id,
        name: this.selectedEvent.name,
      };
      saveCameraEvent(cameraEvent);
      settings = { ...settings, event: cameraEvent };
    }

    if (settings.event || settings.device) {
      this.settingsUpdated.emit(settings);
    }
  }

  cancelSettings() {
    this.settingsUpdated.emit({});
  }

  protected openEventSelector() {
    this.eventsTableData.initialize();
    this.searchControl.patchValue('');
    this.showEventSelector = true;
  }

  protected closeEventSelector() {
    this.showEventSelector = false;
  }

  protected selectEvent(event: EventListDto) {
    this.selectedEvent = event;
    this.settingsForm?.patchValue({ event: event.name });
    this.showEventSelector = false;
  }

  protected formatDate(dateString: string) {
    return formatLithuanianDate(new Date(dateString));
  }

  private initForm() {
    const cameraDevice = loadCameraDevice();
    const cameraEvent = loadCameraEvent();

    this.settingsForm = new FormGroup({
      event: new FormControl(cameraEvent?.name ?? 'Nepasirinktas', [
        Validators.required,
        invalidValues(['Nepasirinktas'], 'Prašome pasirinkti renginį'),
      ]),
      camera: new FormControl(cameraDevice?.deviceId ?? null, [
        Validators.required,
      ]),
    });

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

  private async initCameraOptions() {
    this.cameraDevices = await getVideoDevices();
  }

  private searchEvents: PagedDataLoader<string, EventListDto> = (
    searchTerm,
    keyOffset,
    pageSize,
  ) => {
    const searchParams: EventSearchParamsDto = {
      searchTerm,
      keyOffset,
      pageSize,
    };

    return this.eventService.searchEvents(searchParams);
  };
}
