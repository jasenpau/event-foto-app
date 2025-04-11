import { map, of, Subject, tap } from 'rxjs';
import {
  PagedData,
  PagedDataLoader,
  PagedDataTableEvent,
} from './paged-table.types';

export class PagedDataTable<TKey extends string | number, TData> {
  private readonly loaderFunction: PagedDataLoader<TKey, TData>;
  private readonly keySelector: (data: TData) => TKey;
  private readonly pageSize: number;
  private readonly startingOffset: TKey;

  private internalList: Record<string | number, PagedData<TKey, TData>> = {};
  private offsetList: TKey[];
  private searchTerm: string | null = null;
  private currentPage: PagedData<TKey, TData> | null = null;
  private currentOffsetIndex: number;
  private isLoading = false;

  public readonly events$ = new Subject<PagedDataTableEvent>();

  constructor(
    loaderFunction: PagedDataLoader<TKey, TData>,
    keySelector: (data: TData) => TKey,
    startingOffset: TKey,
    pageSize = 20,
  ) {
    this.loaderFunction = loaderFunction;
    this.keySelector = keySelector;
    this.startingOffset = startingOffset;
    this.offsetList = [startingOffset];
    this.currentOffsetIndex = 0;
    this.pageSize = pageSize;
  }

  public get showPrevious() {
    return this.currentOffsetIndex !== 0;
  }

  public get showNext() {
    return this.currentPage?.hasNextPage ?? false;
  }

  public get loading() {
    return this.isLoading;
  }

  public get pageData() {
    return this.currentPage?.data;
  }

  public setSearchTerm(searchTerm: string | null): void {
    this.searchTerm = searchTerm;
    this.currentPage = null;
    this.offsetList = [this.startingOffset];
    this.currentOffsetIndex = 0;
    this.internalList = {};
    this.initialize();
  }

  public initialize() {
    this.loadData(this.startingOffset)
      .pipe(
        tap((result) => {
          this.currentPage = result.data;
          this.events$.next(PagedDataTableEvent.Loaded);
        }),
      )
      .subscribe();
  }

  public loadNextPage() {
    if (this.currentPage === null || !this.currentPage.hasNextPage) {
      console.error('Current page is null, cannot load next page.');
      this.events$.next(PagedDataTableEvent.Error);
      return;
    }

    const lastItem = this.currentPage.data[this.currentPage.data.length - 1];
    const lastItemKey = this.keySelector(lastItem);
    this.loadData(lastItemKey)
      .pipe(
        tap((result) => {
          this.currentPage = result.data;
          if (!result.cached) this.offsetList.push(lastItemKey);
          this.currentOffsetIndex++;
          this.events$.next(PagedDataTableEvent.Loaded);
        }),
      )
      .subscribe();
  }

  public loadPreviousPage() {
    if (this.currentPage === null) {
      console.error('Current page is null, cannot load previous page.');
      this.events$.next(PagedDataTableEvent.Error);
      return;
    }

    if (this.currentOffsetIndex === 0) {
      console.error('Already at the first page, cannot go further back.');
    }

    const prevKey = this.offsetList[this.currentOffsetIndex - 1];
    this.loadData(prevKey)
      .pipe(
        tap((result) => {
          this.currentPage = result.data;
          this.currentOffsetIndex--;
          this.events$.next(PagedDataTableEvent.Loaded);
        }),
      )
      .subscribe();
  }

  private loadData(keyOffset: TKey) {
    if (this.internalList[keyOffset]) {
      return of({ data: this.internalList[keyOffset], cached: true });
    }

    return this.loaderFunction(this.searchTerm, keyOffset, this.pageSize).pipe(
      map((data) => {
        this.internalList[keyOffset] = data;
        return {
          data,
          cached: false,
        };
      }),
    );
  }
}
