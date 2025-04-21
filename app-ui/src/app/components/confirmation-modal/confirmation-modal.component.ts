import { Component, OnDestroy, OnInit } from '@angular/core';
import { ModalService } from '../../services/modal/modal.service';
import { DisposableComponent } from '../disposable/disposable.component';
import { takeUntil, tap } from 'rxjs';
import { ModalData } from '../../services/modal/modal.types';
import { NgIf } from '@angular/common';
import { ButtonComponent } from '../button/button.component';
import { ButtonType } from '../button/button.types';

@Component({
  selector: 'app-confirmation-modal',
  imports: [NgIf, ButtonComponent],
  templateUrl: './confirmation-modal.component.html',
  styleUrl: './confirmation-modal.component.scss',
})
export class ConfirmationModalComponent
  extends DisposableComponent
  implements OnInit, OnDestroy
{
  protected modalData: ModalData | null = null;

  constructor(private readonly modalService: ModalService) {
    super();
  }

  ngOnInit() {
    this.modalService.modalData$
      .pipe(
        tap((data) => {
          this.modalData = data;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  confirmAction() {
    this.modalService.confirmAction();
  }

  cancelAction() {
    this.modalService.cancelAction();
  }

  protected readonly ButtonType = ButtonType;
}
