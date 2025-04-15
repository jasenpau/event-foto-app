import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EventGalleryViewComponent } from './event-gallery-view.component';

describe('EventGalleryViewComponent', () => {
  let component: EventGalleryViewComponent;
  let fixture: ComponentFixture<EventGalleryViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EventGalleryViewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EventGalleryViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
