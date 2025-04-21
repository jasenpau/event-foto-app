export interface ModalData {
  body: string;
  confirm?: string;
  cancel?: string;
}

export enum ModalActions {
  Confirm = 'Confirm',
  Cancel = 'Cancel',
}
