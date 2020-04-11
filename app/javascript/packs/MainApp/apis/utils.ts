import axios from 'axios'

const BASE_URL = "/api/v1/";
export const baseAxios = axios.create({
  baseURL: BASE_URL,
});
