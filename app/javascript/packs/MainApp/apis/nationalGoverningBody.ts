import { Datum, GetNationalGoverningBodiesSchema } from '../schemas/getNationalGoverningBodiesSchema';
import { baseAxios } from './utils'

export interface NgbResponse {
  nationalGoverningBodies: Datum[];
}

export async function getNationalGoverningBodies(): Promise<NgbResponse> {
  const url = 'national_governing_bodies'

  try {
    const ngbResponse = await baseAxios.get<GetNationalGoverningBodiesSchema>(url)

    return {
      nationalGoverningBodies: ngbResponse.data.data
    }
  } catch (err) {
    throw err
  }
}
