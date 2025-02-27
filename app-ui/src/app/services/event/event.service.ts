import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiBaseUrl } from '../../globals/variables';
import { EventData, EventDto } from './event.types';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';

@Injectable({
  providedIn: 'root'
})
export class EventService {

  constructor(private http: HttpClient) { }

  getEvents(): Observable<EventData[]> {
    return this.http.get<EventDto[]>(`${ApiBaseUrl}/event`, getAuthHeaders());
  }
}
