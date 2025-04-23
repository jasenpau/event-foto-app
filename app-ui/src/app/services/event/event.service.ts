import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  EventCreateDto,
  EventListDto,
  EventDto,
  EventPhotographer,
  EventSearchParamsDto,
} from './event.types';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import { PagedData } from '../../components/paged-table/paged-table.types';
import { EnvService } from '../environment/env.service';
import { GalleryDto } from '../gallery/gallery.types';

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

  getEventPhotographers(id: number): Observable<EventPhotographer[]> {
    return this.http.get<EventPhotographer[]>(
      `${this.apiBaseUrl}/event/${id}/photographers`,
      getAuthHeaders(),
    );
  }

  assignPhotographerToEvent(
    eventId: number,
    userId: string,
  ): Observable<EventPhotographer[]> {
    return this.http.post<EventPhotographer[]>(
      `${this.apiBaseUrl}/event/${eventId}/photographers`,
      { userId },
      getAuthHeaders(),
    );
  }

  unassignPhotographerFromEvent(
    eventId: number,
    userId: string,
  ): Observable<EventPhotographer[]> {
    return this.http.delete<EventPhotographer[]>(
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

  getEventGalleries(eventId: number) {
    return this.http.get<GalleryDto[]>(
      `${this.apiBaseUrl}/event/${eventId}/gallery`,
      {
        ...getAuthHeaders(),
      },
    );
  }
}
