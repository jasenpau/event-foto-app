import { Observable } from 'rxjs';

export interface PagedData<TKey extends string | number, TData> {
  data: TData[];
  keyOffset: TKey;
  pageSize: number;
  hasNextPage: boolean;
}

export enum PagedDataTableEvent {
  Loaded = 'loaded',
  Error = 'error',
}

export type PagedDataLoader<TKey extends string | number, TData> = (
  searchTerm: string | null,
  keyOffset: TKey | null,
  pageSize?: number,
) => Observable<PagedData<TKey, TData>>;
