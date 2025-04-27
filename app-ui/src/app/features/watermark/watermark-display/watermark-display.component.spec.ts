import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WatermarkDisplayComponent } from './watermark-display.component';

describe('WatermarkDisplayComponent', () => {
  let component: WatermarkDisplayComponent;
  let fixture: ComponentFixture<WatermarkDisplayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WatermarkDisplayComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WatermarkDisplayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
