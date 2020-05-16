import { Datum, GetNationalGoverningBodiesSchema } from '../schemas/getNationalGoverningBodiesSchema';
import { DataAttributes, GetNationalGoverningBodySchema, IncludedAttributes } from '../schemas/getNationalGoverningBodySchema';
import { baseAxios } from './utils'

export interface NgbsResponse {
  nationalGoverningBodies: Datum[];
}

export interface NgbResponse {
  nationalGoverningBody: DataAttributes;
  id: string;
  socialAccounts: IncludedAttributes[];
  teamCount: number;
  refereeCount: number;
}

export async function getNationalGoverningBodies(): Promise<NgbsResponse> {
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

export async function getNationalGoverningBody(id: number): Promise<NgbResponse> {
  const url = `national_governing_bodies/${id}`

  try {
    const ngbResponse = await baseAxios.get<GetNationalGoverningBodySchema>(url)
    const socialAccounts = ngbResponse.data.included.map((account): IncludedAttributes => account.attributes)
    const teamCount = ngbResponse.data.data.relationships.teams.data.length
    const refereeCount = ngbResponse.data.data.relationships.referees.data.length

    return {
      id: ngbResponse.data.data.id,
      nationalGoverningBody: ngbResponse.data.data.attributes,
      refereeCount,
      socialAccounts,
      teamCount,
    }
  } catch (err) {
    throw err
  }
}
