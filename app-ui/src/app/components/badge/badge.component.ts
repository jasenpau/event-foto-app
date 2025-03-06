import { Component, Input } from '@angular/core';
import { BadgeType } from './badge.types';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-badge',
  imports: [NgClass],
  templateUrl: './badge.component.html',
  styleUrl: './badge.component.scss',
})
export class BadgeComponent {
  @Input({ required: true }) public type!: BadgeType;

  badgeType = BadgeType;
}
