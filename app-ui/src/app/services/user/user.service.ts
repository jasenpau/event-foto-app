import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import { ApiBaseUrl } from '../../globals/variables';
import { RegisterDto, UserData } from './user.types';
import { AuthService } from '../auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private currentUser: UserData | null = null;

  constructor(private http: HttpClient,
              private authService: AuthService) { }

  fetchCurrentUserData(): Observable<UserData> {
    const userId = this.authService.getUserTokenData()?.uniqueId;
    if (!userId) {
      this.authService.msalLogin();
    }

    return this.http.get<UserData>(`${ApiBaseUrl}/user/${userId}`, getAuthHeaders())
      .pipe(tap(userData => {
        this.currentUser = userData;
      }));
  }

  getCurrentUserData(): UserData | null {
    return this.currentUser;
  }

  register(userDetails: RegisterDto): Observable<UserData> {
    return this.http.post<UserData>(`${ApiBaseUrl}/user/register`, userDetails, getAuthHeaders())
      .pipe(tap(result => {
        this.currentUser = result
      }));
  }
}
