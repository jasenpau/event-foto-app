import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  EventCreateDto,
  EventListDto,
  EventDto,
  PhotographerAssignment,
  EventSearchParamsDto,
} from './event.types';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import { PagedData } from '../../components/paged-table/paged-table.types';
import { EnvService } from '../environment/env.service';

@Injectable({
  providedIn: 'root',
})
export class EventService {
  private readonly apiBaseUrl;

  constructor(
    private readonly http: HttpClient,
    private readonly envService: EnvService,
  ) {
    this.apiBaseUrl = this.envService.getConfig().apiBaseUrl;
  }

  getEventDetails(id: number): Observable<EventDto> {
    return this.http.get<EventDto>(
      `${this.apiBaseUrl}/event/${id}`,
      getAuthHeaders(),
    );
  }

  getEventPhotographers(id: number): Observable<PhotographerAssignment[]> {
    return this.http.get<PhotographerAssignment[]>(
      `${this.apiBaseUrl}/event/${id}/photographers`,
      getAuthHeaders(),
    );
  }

  assignPhotographerToEvent(
    eventId: number,
    galleryId: number,
    userId: string,
  ) {
    return this.http.post(
      `${this.apiBaseUrl}/event/${eventId}/photographers`,
      { userId, galleryId },
      getAuthHeaders(),
    );
  }

  unassignPhotographerFromEvent(eventId: number, userId: string) {
    return this.http.delete(
      `${this.apiBaseUrl}/event/${eventId}/photographers/${userId}`,
      getAuthHeaders(),
    );
  }

  createEvent(event: EventCreateDto): Observable<EventDto> {
    return this.http.post<EventDto>(
      `${this.apiBaseUrl}/event`,
      event,
      getAuthHeaders(),
    );
  }

  updateEvent(id: number, event: EventCreateDto): Observable<EventDto> {
    return this.http.put<EventDto>(
      `${this.apiBaseUrl}/event/${id}`,
      event,
      getAuthHeaders(),
    );
  }

  searchEvents(searchParams: EventSearchParamsDto) {
    let params = new HttpParams();
    if (searchParams.searchTerm)
      params = params.append('query', searchParams.searchTerm);
    if (searchParams.keyOffset)
      params = params.append('keyOffset', searchParams.keyOffset);
    if (searchParams.pageSize)
      params = params.append('pageSize', searchParams.pageSize);
    if (searchParams.fromDate)
      params = params.append('fromDate', searchParams.fromDate.toISOString());
    if (searchParams.toDate)
      params = params.append('toDate', searchParams.toDate.toISOString());
    if (searchParams.showArchived)
      params = params.append('showArchived', searchParams.showArchived);

    return this.http.get<PagedData<string, EventListDto>>(
      `${this.apiBaseUrl}/event/search`,
      {
        ...getAuthHeaders(),
        params,
      },
    );
  }

  archiveEvent(eventId: number) {
    return this.http.post<boolean>(
      `${this.apiBaseUrl}/event/${eventId}/archive`,
      {},
      getAuthHeaders(),
    );
  }
}
