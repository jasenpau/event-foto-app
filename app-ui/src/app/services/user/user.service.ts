import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable, of, tap } from 'rxjs';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import {
  AppGroupsDto,
  RegisterDto,
  UserData,
  UserGroupsCallback,
} from './user.types';
import { AuthService } from '../auth/auth.service';
import { UserGroup } from '../../globals/userGroups';
import { PagedData } from '../../components/paged-table/paged-table.types';
import { EnvService } from '../environment/env.service';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly apiBaseUrl;
  private currentUser: UserData | null = null;
  private appGroups: AppGroupsDto | null = null;

  constructor(
    private readonly http: HttpClient,
    private readonly authService: AuthService,
    private readonly envService: EnvService,
  ) {
    this.apiBaseUrl = this.envService.getConfig().apiBaseUrl;
  }

  fetchCurrentUserData(): Observable<UserData> {
    const userId = this.authService.getUserTokenData()?.uniqueId;
    if (!userId) {
      this.authService.msalLogin();
    }

    return this.http
      .get<UserData>(`${this.apiBaseUrl}/user/${userId}`, getAuthHeaders())
      .pipe(
        tap((userData) => {
          this.currentUser = userData;
        }),
      );
  }

  getCurrentUserData(): UserData | null {
    return this.currentUser;
  }

  getAppGroups(): Observable<AppGroupsDto> {
    if (this.appGroups) return of(this.appGroups);

    return this.http
      .get<AppGroupsDto>(`${this.apiBaseUrl}/user/groups`, getAuthHeaders())
      .pipe(
        tap((appGrounds) => {
          this.appGroups = appGrounds;
        }),
      );
  }

  getUserGroups(): Observable<UserGroup[]> {
    const user = this.authService.getUserTokenData();
    if (!user) return of([]);

    return this.getAppGroups().pipe(
      map((groups) => {
        if (user.groups.includes(groups.systemAdministrators)) {
          return [
            UserGroup.SystemAdmin,
            UserGroup.EventAdmin,
            UserGroup.Photographer,
          ];
        } else if (user.groups.includes(groups.eventAdministrators)) {
          return [UserGroup.EventAdmin, UserGroup.Photographer];
        } else if (user.groups.includes(groups.photographers)) {
          return [UserGroup.Photographer];
        }
        return [];
      }),
    );
  }

  userGroupsCallback(callback: UserGroupsCallback) {
    this.getUserGroups().subscribe((groups) => {
      callback(groups);
    });
  }

  register(userDetails: RegisterDto): Observable<UserData> {
    return this.http
      .post<UserData>(
        `${this.apiBaseUrl}/user/register`,
        userDetails,
        getAuthHeaders(),
      )
      .pipe(
        tap((result) => {
          this.currentUser = result;
        }),
      );
  }

  searchUsers(
    searchTerm: string | null,
    keyOffset: string | null,
    pageSize?: number,
    excludeEventId?: number,
  ) {
    let params = new HttpParams();
    if (searchTerm) params = params.append('query', searchTerm);
    if (keyOffset) params = params.append('keyOffset', keyOffset.toString());
    if (pageSize) params = params.append('pageSize', pageSize.toString());
    if (excludeEventId)
      params = params.append('excludeEventId', excludeEventId.toString());
    return this.http.get<PagedData<string, UserData>>(
      `${this.apiBaseUrl}/user/search`,
      {
        ...getAuthHeaders(),
        params,
      },
    );
  }
}
