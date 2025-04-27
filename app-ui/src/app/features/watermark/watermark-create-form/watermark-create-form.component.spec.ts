import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WatermarkCreateFormComponent } from './watermark-create-form.component';

describe('WatermarkCreateFormComponent', () => {
  let component: WatermarkCreateFormComponent;
  let fixture: ComponentFixture<WatermarkCreateFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WatermarkCreateFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WatermarkCreateFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
