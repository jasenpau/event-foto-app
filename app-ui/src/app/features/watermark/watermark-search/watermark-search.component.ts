import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { InputFieldComponent } from '../../../components/forms/input-field/input-field.component';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { PaginationControlsComponent } from '../../../components/pagination-controls/pagination-controls.component';
import { PagedDataTable } from '../../../components/paged-table/paged-table';
import { WatermarkDto } from '../../../services/watermark/watermark.types';
import { WatermarkService } from '../../../services/watermark/watermark.service';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { debounceTime, takeUntil, tap } from 'rxjs';
import { NgClass, NgForOf, NgIf } from '@angular/common';
import { SpinnerComponent } from '../../../components/spinner/spinner.component';
import { ButtonComponent } from '../../../components/button/button.component';
import { ButtonType } from '../../../components/button/button.types';
import { BlobService } from '../../../services/blob/blob.service';
import { SasUri } from '../../../services/image/image.types';
import { forkJoin, map } from 'rxjs';

const WATERMARK_TABLE_PAGE_SIZE = 5;

@Component({
  selector: 'app-watermark-search',
  standalone: true,
  imports: [
    InputFieldComponent,
    ReactiveFormsModule,
    PaginationControlsComponent,
    NgForOf,
    NgIf,
    SpinnerComponent,
    ButtonComponent,
    NgClass,
  ],
  templateUrl: './watermark-search.component.html',
  styleUrl: './watermark-search.component.scss',
})
export class WatermarkSearchComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  @Input() set refresh(event: string) {
    if (event.startsWith('created') || event.startsWith('deleted')) {
      this.watermarkTableData.setSearchTerm(this.searchControl.value);
    }
  }
  @Input() actionButtonLabel = '';
  @Input() showLarge = false;
  @Output() actionWatermarkId = new EventEmitter<number | null>();

  protected readonly buttonType = ButtonType;
  protected watermarkTableData: PagedDataTable<number, WatermarkDto>;
  protected searchControl = new FormControl('', [Validators.max(100)]);
  protected sasUri?: SasUri;

  constructor(
    private watermarkService: WatermarkService,
    private readonly blobService: BlobService,
  ) {
    super();
    this.watermarkTableData = new PagedDataTable<number, WatermarkDto>(
      (searchTerm, keyOffset, pageSize) => {
        return this.searchWatermarks(searchTerm, keyOffset, pageSize);
      },
      (item) => item.id,
      0,
      WATERMARK_TABLE_PAGE_SIZE,
    );
  }

  ngOnInit() {
    this.initializeSearch();
  }

  protected triggerActionEvent(id: number) {
    this.actionWatermarkId.emit(id);
  }

  protected getWatermarkUrl(watermark: WatermarkDto) {
    return `${this.sasUri?.baseUri}/${this.watermarkService.watermarksContainer}/${watermark.filename}?${this.sasUri?.params}`;
  }

  private initializeSearch() {
    this.watermarkTableData.initialize();
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        tap((value) => {
          this.watermarkTableData.setSearchTerm(value);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  private searchWatermarks = (
    searchTerm: string | null,
    keyOffset: number | null,
    pageSize?: number,
  ) => {
    const sas = this.blobService.getReadOnlySasUri();
    const watermarks = this.watermarkService.searchWatermarks(
      searchTerm,
      keyOffset,
      pageSize,
    );

    return forkJoin([sas, watermarks]).pipe(
      map(([sas, watermarks]) => {
        this.sasUri = sas;
        return watermarks;
      }),
    );
  };
}
