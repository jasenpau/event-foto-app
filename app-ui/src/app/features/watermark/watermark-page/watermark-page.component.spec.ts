import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WatermarkPageComponent } from './watermark-page.component';

describe('WatermarkPageComponent', () => {
  let component: WatermarkPageComponent;
  let fixture: ComponentFixture<WatermarkPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WatermarkPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WatermarkPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
