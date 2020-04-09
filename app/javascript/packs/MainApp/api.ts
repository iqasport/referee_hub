import axios from 'axios'
import { camelCase } from 'lodash'

import * as schema from './schema'

const BASE_URL = '/api/v1/'
const baseAxios = axios.create({
  baseURL: BASE_URL,
  transformResponse: [(data) => {
    const newData = transformAttributes(JSON.parse(data))

    return newData
  }]
})
type JsonPrimitive = string | number | boolean | null;
type JsonValue = JsonPrimitive | JsonObject | JsonArray
type JsonObject = { [key: string]: JsonValue }
interface JsonArray extends Array<JsonValue> {}

interface Data<T> {
  id: string;
  type: string;
  attributes: T;
  relationships?: JsonObject;
}

interface BaseSingleResponse<T> {
  data: Data<T>;
  included?: JsonArray;
}

export interface IUserResponse {
  user: schema.Users;
  roles: string[];
}

// tslint:disable-next-line: no-any
const transformAttributes = (data: any) => {
  return Object.keys(data).reduce((newData, key) => {
    const val: JsonValue = data[key]
    let newVal
    if (!val) {
      newVal = val
    } else if (typeof val === 'object') {
      newVal = transformAttributes(val)
    } else {
      newVal = val
    }
    
    const newKey = camelCase(key)
    
    newData[camelCase(key)] = newVal;
    return newData
  }, {})
}

export async function getCurrentUser(): Promise<IUserResponse> {
  const url = 'users/current_user'

  try {
    const userResponse = await baseAxios.get<BaseSingleResponse<schema.Users>>(url)
    // tslint:disable-next-line: no-any
    const mapRolesAttributes = (role: any): string => role.attributes.accessType
    const roles = Object.values(userResponse.data.included).map(mapRolesAttributes)

    return {
      roles,
      user: {
        id: userResponse.data.data.id,
        ...userResponse.data.data.attributes,
      },
    };
  } catch (err) {
    throw err
  }
}
