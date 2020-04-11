import { AxiosResponse } from 'axios';
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

const formatUserResponse = (response: AxiosResponse<CurrentUserSchema>): UserResponse => {
  const mapRolesAttributes = (role: Included): string => role.attributes.accessType;
  const roles = Object.values(response.data.included).map(mapRolesAttributes)

  return {
    id: response.data.data.id,
    roles,
    user: {
      ...response.data.data.attributes,
    },
  };
}

export async function getCurrentUser(): Promise<UserResponse> {
  const url = "users/current_user";

  try {
    const userResponse = await baseAxios.get<CurrentUserSchema>(url);
    return formatUserResponse(userResponse)
  } catch (err) {
    throw err;
  }
}

export async function updatePolicyAcceptance(userId: string, type: string): Promise<UserResponse> {
  const url = `users/${userId}/${type}_policies`

  try {
    const userResponse = await baseAxios.post(url, {id: userId})
    return formatUserResponse(userResponse)
  } catch (err) {
    throw err
  }
}
