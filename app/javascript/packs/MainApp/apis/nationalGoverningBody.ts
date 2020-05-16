import axios, { AxiosResponse } from 'axios'

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

const formatNgbResponse = (response: AxiosResponse<GetNationalGoverningBodySchema>): NgbResponse => {
  const socialAccounts = response.data.included.map((account): IncludedAttributes => account.attributes)
  const teamCount = response.data.data.relationships.teams.data.length
  const refereeCount = response.data.data.relationships.referees.data.length

  return {
    id: response.data.data.id,
    nationalGoverningBody: response.data.data.attributes,
    refereeCount,
    socialAccounts,
    teamCount,
  }
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
    return formatNgbResponse(ngbResponse)
  } catch (err) {
    throw err
  }
}

export async function updateLogo(ngbId: string, logo: File): Promise<NgbResponse> {
  const url = `/api/v1/national_governing_bodies/${ngbId}/update_logo`

  try {
    const data = new FormData()
    data.append('logo', logo)

    const ngbResponse = await axios.post(url, data)
    return formatNgbResponse(ngbResponse)
  } catch (err) {
    throw err
  }
}
