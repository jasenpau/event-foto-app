import { AbstractControl, ValidatorFn } from '@angular/forms';

export const invalidValues = (
  valueList: unknown[],
  customMessage?: string,
): ValidatorFn => {
  return (control: AbstractControl) => {
    let fieldValue = control.value;
    if (typeof control.value === 'string' || control.value instanceof String) {
      fieldValue = control.value.trim();
    }

    if (valueList.includes(fieldValue)) {
      return { invalidValue: control.value, customMessage };
    }
    return null;
  };
};
