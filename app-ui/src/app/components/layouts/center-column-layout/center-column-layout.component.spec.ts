import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CenterColumnLayoutComponent } from './center-column-layout.component';

describe('CenterColumnLayoutComponent', () => {
  let component: CenterColumnLayoutComponent;
  let fixture: ComponentFixture<CenterColumnLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CenterColumnLayoutComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CenterColumnLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
