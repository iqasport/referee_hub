import axios, { AxiosResponse } from 'axios'
import { transform } from 'lodash';

import { HeadersMap } from 'MainApp/pages/ImportWizard/MapStep';
import { Datum, GetNationalGoverningBodiesSchema, Meta } from 'MainApp/schemas/getNationalGoverningBodiesSchema';
import {
  DataAttributes,
  GetNationalGoverningBodySchema,
  IncludedAttributes
} from 'MainApp/schemas/getNationalGoverningBodySchema';

import { baseAxios } from './utils'

export interface AnnotatedNgb extends Datum {
  teamCount: number;
  refereeCount: number;
}

export interface NgbsResponse {
  nationalGoverningBodies: AnnotatedNgb[];
  meta: Meta;
}

export interface NgbResponse {
  nationalGoverningBody: DataAttributes;
  id: string;
  socialAccounts: IncludedAttributes[];
  teamCount: number;
  refereeCount: number;
  stats: IncludedAttributes[];
}

type OmittedAttributes = 'logoUrl'
export interface UpdateNgbRequest extends Omit<DataAttributes, OmittedAttributes> {
  urls: string[];
}

const formatNgbResponse = (response: AxiosResponse<GetNationalGoverningBodySchema>): NgbResponse => {
  const socialAccounts = response.data.included
    .filter(record => record.type === 'socialAccount')
    .map((account): IncludedAttributes => account.attributes)
  const stats = response.data.included
    .filter(record => record.type === 'nationalGoverningBodyStat')
    .map(stat => stat.attributes)
  const teamCount = response.data.data.relationships.teams.data.length
  const refereeCount = response.data.data.relationships.referees.data.length

  return {
    id: response.data.data.id,
    nationalGoverningBody: response.data.data.attributes,
    refereeCount,
    socialAccounts,
    stats,
    teamCount,
  }
}

const formatNgbs = (data: Datum[]): AnnotatedNgb[] => {
  return data.map((ngb: Datum) => {
    const teamCount = ngb.relationships.teams.data.length
    const refereeCount = ngb.relationships.referees.data.length

    return {
      ...ngb,
      refereeCount,
      teamCount,
    }
  })
}

export async function getNationalGoverningBodies(): Promise<NgbsResponse> {
  const url = 'national_governing_bodies'

  try {
    const ngbResponse = await baseAxios.get<GetNationalGoverningBodiesSchema>(url)
    const formattedNgbs = formatNgbs(ngbResponse.data.data)

    return {
      meta: ngbResponse.data.meta,
      nationalGoverningBodies: formattedNgbs,
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

export async function updateNationalGoverningBody(id: number, ngb: UpdateNgbRequest): Promise<NgbResponse> {
  const url = `national_governing_bodies/${id}`

  try {
    const ngbResponse = await baseAxios.put<GetNationalGoverningBodySchema>(url, {...ngb})
    return formatNgbResponse(ngbResponse)
  } catch (err) {
    throw err
  }
}

export async function importNgbs(file: File, mappedData: HeadersMap): Promise<NgbsResponse> {
  const url = '/api/v1/national_governing_bodies/import'
  const reversedMap = transform(mappedData, (acc, value, key) => {
    acc[value] = key
    return acc
  }, {})

  try {
    const data = new FormData()
    data.append('file', file)
    data.append('mapped_headers', JSON.stringify(reversedMap))

    const ngbsResponse = await axios.post<GetNationalGoverningBodiesSchema>(url, data)

    return {
      meta: ngbsResponse.data.meta,
      nationalGoverningBodies: formatNgbs(ngbsResponse.data.data)
    }
  } catch (err) {
    throw err
  }
}
