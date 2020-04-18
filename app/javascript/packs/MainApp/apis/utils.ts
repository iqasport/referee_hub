import axios from 'axios'
import { snakeCase, transform } from 'lodash'

const BASE_URL = "/api/v1/";

export const baseAxios = axios.create({
  baseURL: BASE_URL,
  transformRequest: [(data, headers) => {
    headers["Content-Type"] = "application/json";
    const transformedData = transform(data, (acc, value, key) => {
      const newKey = snakeCase(key)
      acc[newKey] = value
      return acc
    }, {})

    return JSON.stringify(transformedData)
  }]
});
