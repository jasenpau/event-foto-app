import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WatermarkSearchComponent } from './watermark-search.component';

describe('WatermarkSearchComponent', () => {
  let component: WatermarkSearchComponent;
  let fixture: ComponentFixture<WatermarkSearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WatermarkSearchComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WatermarkSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
