import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiBaseUrl } from '../../globals/variables';
import {
  EventCreateDto,
  EventListDto,
  EventDto,
  EventPhotographer,
} from './event.types';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import { PagedData } from '../../components/paged-table/paged-table.types';

@Injectable({
  providedIn: 'root',
})
export class EventService {
  constructor(private http: HttpClient) {}

  getEvents(): Observable<EventListDto[]> {
    return this.http.get<EventListDto[]>(
      `${ApiBaseUrl}/event`,
      getAuthHeaders(),
    );
  }

  getEventDetails(id: number): Observable<EventDto> {
    return this.http.get<EventDto>(
      `${ApiBaseUrl}/event/${id}`,
      getAuthHeaders(),
    );
  }

  getEventPhotographers(id: number): Observable<EventPhotographer[]> {
    return this.http.get<EventPhotographer[]>(
      `${ApiBaseUrl}/event/${id}/photographers`,
      getAuthHeaders(),
    );
  }

  assignPhotographerToEvent(
    eventId: number,
    userId: string,
  ): Observable<EventPhotographer[]> {
    return this.http.post<EventPhotographer[]>(
      `${ApiBaseUrl}/event/${eventId}/photographers`,
      { userId },
      getAuthHeaders(),
    );
  }

  unassignPhotographerFromEvent(
    eventId: number,
    userId: string,
  ): Observable<EventPhotographer[]> {
    return this.http.delete<EventPhotographer[]>(
      `${ApiBaseUrl}/event/${eventId}/photographers/${userId}`,
      getAuthHeaders(),
    );
  }

  createEvent(event: EventCreateDto): Observable<EventDto> {
    return this.http.post<EventDto>(
      `${ApiBaseUrl}/event`,
      event,
      getAuthHeaders(),
    );
  }

  getEventPhotos(): Observable<string[]> {
    return this.http.get<string[]>(
      `${ApiBaseUrl}/imaging/photos`,
      getAuthHeaders(),
    );
  }

  searchEvents(
    searchTerm: string | null,
    keyOffset: string | null,
    pageSize?: number,
  ) {
    let params = new HttpParams();
    if (searchTerm) params = params.append('query', searchTerm);
    if (keyOffset) params = params.append('keyOffset', keyOffset.toString());
    if (pageSize) params = params.append('pageSize', pageSize.toString());
    return this.http.get<PagedData<string, EventListDto>>(
      `${ApiBaseUrl}/event/search`,
      {
        ...getAuthHeaders(),
        params,
      },
    );
  }
}
