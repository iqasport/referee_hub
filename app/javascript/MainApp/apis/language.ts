import { Datum, GetLanguagesSchema } from 'MainApp/schemas/getLanguagesSchema';

import { baseAxios } from './utils'

export interface LanguagesResponse {
  languages: Datum[];
}

export async function getLanguages(): Promise<LanguagesResponse> {
  const url = 'languages'

  try {
    const languagesResponse = await baseAxios.get<GetLanguagesSchema>(url)

    return {
      languages: languagesResponse.data.data
    }
  } catch (err) {
    throw err
  }
}
