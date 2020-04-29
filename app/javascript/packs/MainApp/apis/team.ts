import { Datum, GetTeamsSchema, GroupAffiliation, Meta, Status } from '../schemas/getTeamsSchema';
import { baseAxios } from './utils'

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

  try {
    const teamsResponse = await baseAxios.get<GetTeamsSchema>(url, { params: filter })

    return {
      meta: teamsResponse.data.meta,
      teams: teamsResponse.data.data,
    }
  } catch (err) {
    throw err   
  }
}
