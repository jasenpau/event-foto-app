import { Injectable } from '@angular/core';
import { ModalActions, ModalData } from './modal.types';
import { BehaviorSubject, Subject, take } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ModalService {
  private readonly defaultModalData: ModalData = {
    body: 'Ar tikrai norite atlikti šį veiksmą?',
    confirm: 'Patvirtinti',
    cancel: 'Atšaukti',
  };

  private modalActionsSubject = new Subject<ModalActions>();
  private modalDataSubject = new BehaviorSubject<ModalData | null>(null);

  public get modalData$() {
    return this.modalDataSubject.asObservable();
  }

  public openConfirmModal(modalData: ModalData) {
    const filledModalData = { ...this.defaultModalData, ...modalData };
    this.modalDataSubject.next(filledModalData);
    return this.modalActionsSubject.asObservable().pipe(take(1));
  }

  public cancelAction() {
    this.modalActionsSubject.next(ModalActions.Cancel);
    this.modalDataSubject.next(null);
  }

  public confirmAction() {
    this.modalActionsSubject.next(ModalActions.Confirm);
    this.modalDataSubject.next(null);
  }
}
