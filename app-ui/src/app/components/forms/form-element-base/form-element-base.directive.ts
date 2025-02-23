import { Directive, inject, Injector, OnInit } from '@angular/core';
import {
  ControlContainer,
  ControlValueAccessor,
  FormControl,
  FormControlDirective,
  FormControlName,
  FormGroup,
  NG_VALUE_ACCESSOR,
  NgControl,
} from '@angular/forms';

@Directive({
  standalone: true,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: FormElementBaseDirective,
    },
  ],
})
export class FormElementBaseDirective implements ControlValueAccessor, OnInit {
  control: FormControl = new FormControl();

  private injector = inject(Injector);

  ngOnInit() {
    const ngControl = this.injector.get(NgControl, null, {
      self: true,
      optional: true,
    });

    if (ngControl instanceof FormControlDirective) {
      this.control = ngControl.control;
    } else if (ngControl instanceof FormControlName) {
      if (ngControl.name == null) throw new Error('Form control name is null');
      const container = this.injector.get(ControlContainer)
        .control as FormGroup;
      this.control = container.controls[ngControl.name] as FormControl;
    } else {
      console.warn(
        `Unsupported ngControl injected. Type: ${ngControl?.constructor.name}`,
      );
      this.control = new FormControl();
    }
  }

  /* eslint-disable @typescript-eslint/no-empty-function */
  writeValue() {}
  registerOnChange() {}
  registerOnTouched() {}
}
