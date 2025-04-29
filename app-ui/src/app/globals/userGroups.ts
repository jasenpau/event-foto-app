export enum UserGroup {
  SystemAdmin = 'systemAdministrators',
  EventAdmin = 'eventAdministrators',
  Photographer = 'photographers',
}

export interface ViewPermissions {
  photographer: boolean;
  eventAdmin: boolean;
  systemAdmin: boolean;
}
