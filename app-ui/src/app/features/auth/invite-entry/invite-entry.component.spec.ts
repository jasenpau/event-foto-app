import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InviteEntryComponent } from './invite-entry.component';

describe('InviteEntryComponent', () => {
  let component: InviteEntryComponent;
  let fixture: ComponentFixture<InviteEntryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InviteEntryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InviteEntryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
