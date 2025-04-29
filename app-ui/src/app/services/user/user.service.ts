import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { getAuthHeaders } from '../../helpers/getAuthHeaders';
import { AppGroupsDto, RegisterDto, UserData, UserListDto } from './user.types';
import { AuthService } from '../auth/auth.service';
import { ViewPermissions, UserGroup } from '../../globals/userGroups';
import { PagedData } from '../../components/paged-table/paged-table.types';
import { EnvService } from '../environment/env.service';
import { LoaderService } from '../loader/loader.service';
import { useLoader } from '../../helpers/useLoader';

const USER_LOADING_KEY = 'user-loading';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly apiBaseUrl;
  private readonly appGroups: AppGroupsDto;
  private currentUser: UserData | null = null;

  constructor(
    private readonly http: HttpClient,
    private readonly authService: AuthService,
    private readonly envService: EnvService,
    private readonly loaderService: LoaderService,
  ) {
    this.apiBaseUrl = this.envService.getConfig().apiBaseUrl;
    this.appGroups = this.envService.getConfig().groups;
  }

  fetchCurrentUserData() {
    const userId = this.authService.getUserTokenData()?.uniqueId;
    if (!userId) {
      this.authService.msalLogin();
    }

    return this.http
      .get<UserData>(`${this.apiBaseUrl}/user/current`, getAuthHeaders())
      .pipe(
        useLoader(USER_LOADING_KEY, this.loaderService),
        tap((userData) => {
          this.currentUser = userData;
        }),
      );
  }

  getCurrentUserData(): UserData | null {
    return this.currentUser;
  }

  getAppGroups(): AppGroupsDto {
    return this.appGroups;
  }

  getUserGroups(): UserGroup[] {
    const user = this.authService.getUserTokenData();
    if (!user) return [];

    if (user.groups.includes(this.appGroups.systemAdministrators)) {
      return [
        UserGroup.SystemAdmin,
        UserGroup.EventAdmin,
        UserGroup.Photographer,
      ];
    } else if (user.groups.includes(this.appGroups.eventAdministrators)) {
      return [UserGroup.EventAdmin, UserGroup.Photographer];
    } else if (user.groups.includes(this.appGroups.photographers)) {
      return [UserGroup.Photographer];
    }
    return [];
  }

  getViewPermissions(): ViewPermissions {
    const groups = this.getUserGroups();
    return {
      photographer: groups.includes(UserGroup.Photographer),
      eventAdmin: groups.includes(UserGroup.EventAdmin),
      systemAdmin: groups.includes(UserGroup.SystemAdmin),
    };
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
    return this.http.get<PagedData<string, UserListDto>>(
      `${this.apiBaseUrl}/user/search`,
      {
        ...getAuthHeaders(),
        params,
      },
    );
  }

  inviteUser(props: {
    name: string;
    email: string;
    groupAssignment: string | null;
  }) {
    return this.http.post<string>(`${this.apiBaseUrl}/user/invite`, props, {
      ...getAuthHeaders(),
    });
  }

  validateInvite(inviteKey: string) {
    return this.http.post<boolean>(`${this.apiBaseUrl}/user/validate-invite`, {
      inviteKey,
    });
  }
}
