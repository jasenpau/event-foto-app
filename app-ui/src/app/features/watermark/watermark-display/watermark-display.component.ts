import {
  Component,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import { NgIf } from '@angular/common';
import { WatermarkService } from '../../../services/watermark/watermark.service';
import { BlobService } from '../../../services/blob/blob.service';
import { WatermarkDto } from '../../../services/watermark/watermark.types';
import { SasUri } from '../../../services/image/image.types';
import { forkJoin, map, of, takeUntil, tap } from 'rxjs';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { useLocalLoader } from '../../../helpers/useLoader';
import { SpinnerComponent } from '../../../components/spinner/spinner.component';

@Component({
  selector: 'app-watermark-display',
  standalone: true,
  imports: [NgIf, SpinnerComponent],
  templateUrl: './watermark-display.component.html',
  styleUrl: './watermark-display.component.scss',
})
export class WatermarkDisplayComponent
  extends DisposableComponent
  implements OnInit, OnChanges, OnDestroy
{
  @Input() watermarkId?: number | null;
  @Input() watermarkDto?: WatermarkDto | null;

  protected watermark?: WatermarkDto;
  protected sasUri?: SasUri;
  protected loading = false;

  constructor(
    private readonly watermarkService: WatermarkService,
    private readonly blobService: BlobService,
  ) {
    super();
  }

  ngOnInit() {
    this.loadData();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['watermarkId'] || changes['watermarkDto']) {
      this.loadData();
    }
  }

  protected getWatermarkUrl() {
    if (!this.watermark || !this.sasUri) return '';
    return `${this.sasUri.baseUri}/watermarks/${this.watermark.filename}?${this.sasUri.params}`;
  }

  private fetchWatermark() {
    if (this.watermarkDto) {
      this.watermark = this.watermarkDto;
      return of(this.watermark);
    } else if (this.watermarkId) {
      return this.watermarkService.getWatermark(this.watermarkId).pipe(
        tap((watermark) => {
          this.watermark = watermark;
        }),
        takeUntil(this.destroy$),
      );
    } else {
      this.watermark = undefined;
      this.loading = false;
      return of(null);
    }
  }

  private loadData() {
    const sas$ = this.blobService.getReadOnlySasUri();
    const watermark$ = this.fetchWatermark();
    forkJoin([sas$, watermark$])
      .pipe(
        useLocalLoader((value) => (this.loading = value)),
        map(([sas, watermark]) => {
          this.sasUri = sas;
          if (watermark) this.watermark = watermark;
          else this.watermark = undefined;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
