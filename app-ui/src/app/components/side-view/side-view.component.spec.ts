import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SideViewComponent } from './side-view.component';

describe('SideViewComponent', () => {
  let component: SideViewComponent;
  let fixture: ComponentFixture<SideViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SideViewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SideViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
