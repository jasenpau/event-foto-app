import { UserGroup } from '../../globals/userGroups';

export interface UserData {
  id: string;
  email: string;
  name: string;
}

export interface AppGroupsDto {
  systemAdministrators: string;
  eventAdministrators: string;
  photographers: string;
}

export interface RegisterDto {
  email: string;
  name: string;
}

export type UserGroupsCallback = (groups: UserGroup[]) => void;
