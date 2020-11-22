import axios, { AxiosResponse } from 'axios';
import {
  CurrentUserSchema,
  DataAttributes,
  Included,
} from '../schemas/currentUserSchema'
import { baseAxios } from './utils';

export interface UserResponse {
  user: DataAttributes;
  roles: string[];
  certificationPayments: number[];
  language: Included;
  id: string;
}

export interface UpdatedUserRequest {
  languageId: number;
}

const formatUserResponse = (response: AxiosResponse<CurrentUserSchema>): UserResponse => {
  const roles = response.data.included.map((role: Included): string | null => {
    if (role.type === 'role') {
      return role.attributes.accessType
    }
    return null
  })
  const paidCerts = response.data.included.map((certPayment: Included): number | null => {
    if (certPayment.type === 'certificationPayment') {
      return certPayment.attributes.certificationId
    }
    return null
  })
  const language = response.data.included.find((lang: Included): boolean => {
    if (lang.type === 'language') return true
    return false
  })

  return {
    certificationPayments: paidCerts,
    id: response.data.data.id,
    language,
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

export async function updateAvatar(userId: string, avatar: File): Promise<UserResponse> {
  const url = `/api/v1/users/${userId}/update_avatar`

  try {
    const data = new FormData()
    data.append('avatar', avatar)

    const userResponse = await axios.post(url, data)
    return formatUserResponse(userResponse)
  } catch (err) {
    throw err
  }
}

export async function updateUser(userId: string, user: UpdatedUserRequest): Promise<UserResponse> {
  const url = `users/${userId}`

  try {
    const userResponse = await baseAxios.patch(url, {...user})
    return formatUserResponse(userResponse)
  } catch (err) {
    throw err
  }
}
