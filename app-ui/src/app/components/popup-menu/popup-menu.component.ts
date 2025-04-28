import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  Output,
} from '@angular/core';
import { NgForOf } from '@angular/common';

export interface PopupMenuItem {
  text: string;
  action: () => void;
}

@Component({
  selector: 'app-popup-menu',
  imports: [NgForOf],
  templateUrl: './popup-menu.component.html',
  styleUrl: './popup-menu.component.scss',
})
export class PopupMenuComponent {
  @Input() items: PopupMenuItem[] = [];
  @Output() close = new EventEmitter<void>();

  constructor(private elementRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.close.emit();
    }
  }

  onItemClick(item: PopupMenuItem): void {
    item.action();
    this.close.emit();
  }
}
