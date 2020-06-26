import axios from 'axios'
import { snakeCase, transform } from 'lodash'

const BASE_URL = '/api/v1/';

// tslint:disable-next-line: no-any
export const camelToSnake = (data: any) => {
  return transform(data, (acc, value, key) => {
    const newKey = snakeCase(key.toString())
    acc[newKey] = value
    return acc
  }, {})
}

export const baseAxios = axios.create({
  baseURL: BASE_URL,
  transformRequest: [(data, headers) => {
    headers['Content-Type'] = 'application/json';
    const transformedData = camelToSnake(data)

    return JSON.stringify(transformedData)
  }]
});
