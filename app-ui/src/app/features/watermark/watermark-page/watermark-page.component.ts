import { Component, OnDestroy } from '@angular/core';
import { ButtonComponent } from '../../../components/button/button.component';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';
import { SvgIconSrc } from '../../../components/svg-icon/svg-icon.types';
import { WatermarkSearchComponent } from '../watermark-search/watermark-search.component';
import { WatermarkCreateFormComponent } from '../watermark-create-form/watermark-create-form.component';
import { SideViewComponent } from '../../../components/side-view/side-view.component';
import { NgIf } from '@angular/common';
import { takeUntil, tap } from 'rxjs';
import { WatermarkService } from '../../../services/watermark/watermark.service';
import { DisposableComponent } from '../../../components/disposable/disposable.component';
import { useLocalLoader } from '../../../helpers/useLoader';
import { LoaderOverlayComponent } from '../../../components/loader-overlay/loader-overlay.component';
import { SnackbarService } from '../../../services/snackbar/snackbar.service';
import { SnackbarType } from '../../../services/snackbar/snackbar.types';

@Component({
  selector: 'app-watermark-page',
  imports: [
    ButtonComponent,
    PageHeaderComponent,
    WatermarkSearchComponent,
    WatermarkCreateFormComponent,
    SideViewComponent,
    NgIf,
    LoaderOverlayComponent,
  ],
  templateUrl: './watermark-page.component.html',
  styleUrl: './watermark-page.component.scss',
})
export class WatermarkPageComponent
  extends DisposableComponent
  implements OnDestroy
{
  protected readonly SvgIconSrc = SvgIconSrc;

  protected showCreateForm = false;
  protected refreshEvent = '';
  protected isLoading = false;

  constructor(
    private watermarkService: WatermarkService,
    private readonly snackbarService: SnackbarService,
  ) {
    super();
  }

  protected openCreateForm() {
    this.showCreateForm = true;
  }

  protected handleCreateFormEvent(event: string) {
    if (event === 'created') {
      this.showCreateForm = false;
      this.snackbarService.addSnackbar(
        SnackbarType.Success,
        'Vandens ženklas sėkmingai sukurtas',
      );
      this.refreshEvent = `${event}-${Date.now()}`;
    } else if (event === 'cancel') {
      this.showCreateForm = false;
    }
  }

  protected handleAction(input: number | null) {
    if (!input) return;
    this.deleteWatermark(input);
  }

  private deleteWatermark(id: number) {
    this.watermarkService
      .deleteWatermark(id)
      .pipe(
        useLocalLoader((value) => (this.isLoading = value)),
        tap(() => {
          this.snackbarService.addSnackbar(
            SnackbarType.Info,
            'Vandens sėkmingai pašalintas',
          );
          this.refreshEvent = `deleted-${Date.now()}`;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }
}
