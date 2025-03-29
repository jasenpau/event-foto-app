import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgForOf } from '@angular/common';

@Component({
  selector: 'app-event-calendar',
  imports: [FormsModule, NgForOf],
  templateUrl: './event-calendar.component.html',
  styleUrl: './event-calendar.component.scss',
})
export class EventCalendarComponent implements OnInit {
  months = [
    'Sausis',
    'Vasaris',
    'Kovas',
    'Balandis',
    'Gegužė',
    'Birželis',
    'Liepa',
    'Rugpjūtis',
    'Rugsėjis',
    'Spalis',
    'Lapkritis',
    'Gruodis',
  ];
  years: number[] = [];
  selectedMonth: number = new Date().getMonth();
  selectedYear: number = new Date().getFullYear();
  weeks: { day: number; currentMonth: boolean; today: boolean }[][] = [];

  ngOnInit() {
    this.populateYears();
    this.generateCalendar();
  }

  populateYears() {
    const currentYear = new Date().getFullYear();
    for (let i = currentYear - 10; i <= currentYear + 10; i++) {
      this.years.push(i);
    }
  }

  generateCalendar() {
    this.weeks = [];
    const today = new Date();
    const firstDayDate = new Date(this.selectedYear, this.selectedMonth, 1);
    const firstDay = (firstDayDate.getDay() + 6) % 7;
    const daysInMonth = new Date(
      this.selectedYear,
      this.selectedMonth + 1,
      0,
    ).getDate();
    const daysInPrevMonth = new Date(
      this.selectedYear,
      this.selectedMonth,
      0,
    ).getDate();
    let day = 1;
    let week = [];

    for (let i = 0; i < 6; i++) {
      week = [];
      for (let j = 0; j < 7; j++) {
        const cell = { day: 0, currentMonth: true, today: false };

        if (i === 0 && j < firstDay) {
          cell.day = daysInPrevMonth - (firstDay - 1) + j;
          cell.currentMonth = false;
        } else if (day > daysInMonth) {
          cell.day = day - daysInMonth;
          cell.currentMonth = false;
          day++;
        } else {
          cell.day = day;
          cell.currentMonth = true;
          if (
            day === today.getDate() &&
            this.selectedMonth === today.getMonth() &&
            this.selectedYear === today.getFullYear()
          ) {
            cell.today = true;
          }
          day++;
        }
        week.push(cell);
      }
      this.weeks.push(week);
      if (day > daysInMonth) break;
    }
  }
}
