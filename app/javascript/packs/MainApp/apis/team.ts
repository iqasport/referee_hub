import axios from 'axios'
import { transform } from 'lodash'

import { HeadersMap } from '../pages/ImportWizard/MapStep';
import { Datum, GetTeamsSchema, GroupAffiliation, Meta, Status } from '../schemas/getTeamsSchema';
import { baseAxios, camelToSnake } from './utils'

export interface TeamResponse {
  teams: Datum[];
  meta: Meta;
}

export interface GetTeamsFilter {
  nationalGoverningBodies?: number[];
  status?: Status[];
  q?: string;
  groupAffiliation?: GroupAffiliation[];
}

export async function getTeams(filter: GetTeamsFilter) {
  const url = 'teams'
  const transformedFilter = camelToSnake(filter)

  try {
    const teamsResponse = await baseAxios.get<GetTeamsSchema>(url, { params: transformedFilter })

    return {
      meta: teamsResponse.data.meta,
      teams: teamsResponse.data.data,
    }
  } catch (err) {
    throw err   
  }
}

export async function importNgbTeams(file: File, mappedData: HeadersMap) {
  const url = '/api/v1/ngb-admin/teams_import'
  const reversedMap = transform(mappedData, (acc, value, key) => {
    acc[value] = key
    return acc
  }, {})

  try {
    const data = new FormData()
    data.append('file', file)
    data.append('mapped_headers', JSON.stringify(reversedMap))

    const teamsResponse = await axios.post<GetTeamsSchema>(url, data)

    return {
      meta: teamsResponse.data.meta,
      teams: teamsResponse.data.data,
    }
  } catch (err) {
    throw err
  }
}
