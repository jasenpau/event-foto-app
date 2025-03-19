import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable, of, tap } from 'rxjs';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import { ApiBaseUrl } from '../../globals/variables';
import { AppGroupsDto, RegisterDto, UserData } from './user.types';
import { AuthService } from '../auth/auth.service';
import { UserGroup } from '../../globals/userGroups';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private currentUser: UserData | null = null;
  private appGroups: AppGroupsDto | null = null;

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

  getAppGroups(): Observable<AppGroupsDto> {
    if (this.appGroups) return of(this.appGroups);

    return this.http.get<AppGroupsDto>(`${ApiBaseUrl}/user/groups`, getAuthHeaders())
      .pipe(tap(appGrounds => {
        this.appGroups = appGrounds;
      }));
  }

  userGroups(): Observable<UserGroup[]> {
    const user = this.authService.getUserTokenData();
    if (!user) return of([]);

    return this.getAppGroups()
      .pipe(map(groups => {
        if (user.groups.includes(groups.systemAdministrators)) {
          return [UserGroup.SystemAdmin, UserGroup.EventAdmin, UserGroup.Photographer];
        } else if (user.groups.includes(groups.eventAdministrators)) {
          return [UserGroup.EventAdmin, UserGroup.Photographer];
        } else if (user.groups.includes(groups.photographers)) {
          return [UserGroup.Photographer];
        }
        return [];
      }));
  }

  register(userDetails: RegisterDto): Observable<UserData> {
    return this.http.post<UserData>(`${ApiBaseUrl}/user/register`, userDetails, getAuthHeaders())
      .pipe(tap(result => {
        this.currentUser = result
      }));
  }
}
