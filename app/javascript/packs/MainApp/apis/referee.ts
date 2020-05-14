import { AxiosResponse } from 'axios';

import { DataAttributes, GetRefereeSchema, Included, IncludedAttributes } from '../schemas/getRefereeSchema';
import { baseAxios } from './utils'

export interface IdAttributes extends IncludedAttributes {
  id: string;
}

export interface RefereeResponse {
  referee: DataAttributes | null;
  id: string | null;
  ngbs: IdAttributes[] | null;
  testAttempts: IdAttributes[] | null;
  testResults: IdAttributes[] | null;
  certifications: IdAttributes[] | null;
  locations: IdAttributes[] | null;
  teams: IdAttributes[] | null;
}

export interface AssociationData {
  [key: string]: string;
}
type ForbiddenUpdates = 'isEditable' | 'hasPendingPolicies' | 'avatarUrl' | 'createdAt'
export interface UpdateRefereeRequest extends Omit<DataAttributes, ForbiddenUpdates> {
  teamsData: AssociationData | null;
  ngbData: AssociationData | null;
}

const mapAttributes = (record: Included) => ({ id: record.id, ...record.attributes })

const formatRefereeResponse = (response: AxiosResponse<GetRefereeSchema>): RefereeResponse => {
  const locations = response.data.included
    .filter((record: Included) => record.type === "refereeLocation")
    .map(mapAttributes);
  const certifications = response.data.included
    .filter((record: Included) => record.type === "certification")
    .map(mapAttributes);
  const testAttempts = response.data.included
    .filter((record: Included) => record.type === "testAttempt")
    .map(mapAttributes);
  const testResults = response.data.included
    .filter((record: Included) => record.type === "testResult")
    .map(mapAttributes);
  const ngbs = response.data.included
    .filter((record: Included) => record.type === "nationalGoverningBody")
    .map((record: Included): IdAttributes => ({...record.attributes, nationalGoverningBodyId: parseInt(record.id, 10), id: record.id }))
  const teamsData = response.data.included
    .filter((record: Included) => record.type === "team")
    .map((record: Included): IdAttributes => ({...record.attributes, teamId: parseInt(record.id, 10), id: record.id }))
  const teams = response.data.included
    .filter((record: Included) => record.type === "refereeTeam")
    .map((refereeTeam: Included): IdAttributes => {
      const team = teamsData.find((teamData: IdAttributes) => teamData.teamId === refereeTeam.attributes.teamId)
      return {...team, associationType: refereeTeam.attributes.associationType}
    })

  return {
    certifications,
    id: response.data.data.id,
    locations,
    ngbs,
    referee: {
      ...response.data.data.attributes,
    },
    teams,
    testAttempts,
    testResults,
  };
}

export async function getReferee(refereeId: string | number): Promise<RefereeResponse> {
  const url = `referees/${refereeId}`

  try {
    const refereeResponse = await baseAxios.get<GetRefereeSchema>(url)
    const formattedResponse = formatRefereeResponse(refereeResponse)

    return formattedResponse
  } catch (err) {
    throw err
  }
}

export async function updateReferee(updatedReferee: UpdateRefereeRequest, refereeId: string | number): Promise<RefereeResponse> {
  const url = `/referees/${refereeId}`
  
  try {
    const refereeResponse = await baseAxios.patch<GetRefereeSchema>(url, updatedReferee)
    return formatRefereeResponse(refereeResponse)
  } catch (err) {
    throw err
  }
}
