import { Component, Input } from '@angular/core';
import { ButtonComponent } from '../button/button.component';
import { ButtonType } from '../button/button.types';
import { PagedDataTable } from '../paged-table/paged-table';

@Component({
  selector: 'app-pagination-controls',
  imports: [ButtonComponent],
  templateUrl: './pagination-controls.component.html',
  styleUrl: './pagination-controls.component.scss',
})
export class PaginationControlsComponent {
  @Input({ required: true }) tableData!: PagedDataTable<any, any>;

  protected readonly ButtonType = ButtonType;

  previousPage() {
    this.tableData.loadPreviousPage();
  }

  nextPage() {
    this.tableData.loadNextPage();
  }
}
