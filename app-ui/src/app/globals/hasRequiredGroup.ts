import { UserGroup } from './userGroups';
import { AppGroupsDto } from '../services/user/user.types';

export const hasRequiredGroup = (
  requiredGroup: UserGroup,
  userGroups: string[],
  appGroups: AppGroupsDto,
) => {
  switch (requiredGroup) {
    case UserGroup.SystemAdmin:
      return userGroups.some(
        (group) => group === appGroups.systemAdministrators,
      );
    case UserGroup.EventAdmin:
      return userGroups.some(
        (group) =>
          group === appGroups.systemAdministrators ||
          group === appGroups.eventAdministrators,
      );
    case UserGroup.Photographer:
      return userGroups.some(
        (group) =>
          group === appGroups.systemAdministrators ||
          group === appGroups.eventAdministrators ||
          group === appGroups.photographers,
      );
  }
};
