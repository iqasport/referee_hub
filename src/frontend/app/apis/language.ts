import { Datum, GetLanguagesSchema } from "../schemas/getLanguagesSchema";

import { baseAxios } from "./utils";

export interface LanguagesResponse {
  languages: Datum[];
}

export async function getLanguages(): Promise<LanguagesResponse> {
  const url = "languages";

  const languagesResponse = await baseAxios.get<GetLanguagesSchema>(url);

  return {
    languages: languagesResponse.data.data,
  };
}
