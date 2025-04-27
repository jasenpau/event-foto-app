import { Component, Input } from '@angular/core';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-card-item',
  imports: [NgIf],
  templateUrl: './card-item.component.html',
  styleUrl: './card-item.component.scss',
})
export class CardItemComponent {
  @Input() imageUrl = '/assets/default-cover.jpg';
  @Input() title = '';
  @Input() description = '';
  @Input() hasContent = false;
}
