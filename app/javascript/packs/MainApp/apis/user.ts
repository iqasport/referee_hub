import {
  CurrentUserSchema,
  DataAttributes,
  Included,
} from '../schemas/currentUserSchema'
import { baseAxios } from './utils';

export interface UserResponse {
  user: DataAttributes;
  roles: string[];
  id: string;
}

export async function getCurrentUser(): Promise<UserResponse> {
  const url = "users/current_user";

  try {
    const userResponse = await baseAxios.get<CurrentUserSchema>(url);
    const mapRolesAttributes = (role: Included): string =>
      role.attributes.accessType;
    const roles = Object.values(userResponse.data.included).map(
      mapRolesAttributes
    );

    return {
      id: userResponse.data.data.id,
      roles,
      user: {
        ...userResponse.data.data.attributes,
      },
    };
  } catch (err) {
    throw err;
  }
}
