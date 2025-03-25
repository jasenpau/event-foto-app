import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiBaseUrl } from '../../globals/variables';
import { EventCreateDto, EventListDto, EventDto } from './event.types';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';

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
}
