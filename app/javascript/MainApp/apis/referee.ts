import { AxiosResponse } from 'axios';

import { DataAttributes, GetRefereeSchema, Included, IncludedAttributes, Relationships } from '../schemas/getRefereeSchema';
import { GetRefereesSchema, Meta } from '../schemas/getRefereesSchema';
import { baseAxios, camelToSnake } from './utils'

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

export interface RefereesResponse {
  referees: RefereeResponse[]
  meta: Meta;
}

export interface AssociationData {
  [key: string]: string;
}
type ForbiddenUpdates = 'isEditable' | 'hasPendingPolicies' | 'avatarUrl' | 'createdAt'
export interface UpdateRefereeRequest extends Omit<DataAttributes, ForbiddenUpdates> {
  teamsData: AssociationData | null;
  ngbData: AssociationData | null;
}

export interface GetRefereesFilter {
  nationalGoverningBodies?: number[];
  certifications?: string[];
  q?: string;
}

const mapAttributes = (record: Included) => ({ id: record.id, ...record.attributes })

const formatRefereeResponse = (
  included: Included[], 
  attributes: DataAttributes, 
  relationships: Relationships, 
  id: string
): RefereeResponse => {
  const locations = included
    .filter((record: Included) => record.type === "refereeLocation" && relationships.refereeLocations.data.find(loc => loc.id === record.id))
    .map(mapAttributes);
  const certifications = included
    .filter((record: Included) => record.type === "certification" && relationships.certifications.data.find(cert => cert.id === record.id))
    .map(mapAttributes);
  const testAttempts = included
    .filter((record: Included) => record.type === "testAttempt" && relationships.testAttempts.data.find(attempt => attempt.id === record.id))
    .map(mapAttributes);
  const testResults = included
    .filter((record: Included) => record.type === "testResult" && relationships.testResults.data.find(result => result.id === record.id))
    .map(mapAttributes);
  const ngbs = included
    .filter((record: Included) => record.type === "nationalGoverningBody" && relationships.nationalGoverningBodies.data.find(ngb => ngb.id === record.id))
    .map((record: Included): IdAttributes => ({...record.attributes, nationalGoverningBodyId: parseInt(record.id, 10), id: record.id }))
  const teamsData = included
    .filter((record: Included) => record.type === "team" && relationships.teams.data.find(team => team.id === record.id))
    .map((record: Included): IdAttributes => ({...record.attributes, teamId: parseInt(record.id, 10), id: record.id }))
  const teams = included
    .filter((record: Included) => record.type === "refereeTeam" && relationships.refereeTeams.data.find(refTeam => refTeam.id === record.id))
    .map((refereeTeam: Included): IdAttributes => {
      const team = teamsData.find((teamData: IdAttributes) => teamData.teamId === refereeTeam.attributes.teamId)
      return {...team, associationType: refereeTeam.attributes.associationType}
    })

  return {
    certifications,
    id,
    locations,
    ngbs,
    referee: {
      ...attributes,
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
    const formattedResponse = formatRefereeResponse(
      refereeResponse.data.included, 
      refereeResponse.data.data.attributes,
      refereeResponse.data.data.relationships,
      refereeResponse.data.data.id
    )

    return formattedResponse
  } catch (err) {
    throw err
  }
}

export async function updateReferee(updatedReferee: UpdateRefereeRequest, refereeId: string | number): Promise<RefereeResponse> {
  const url = `/referees/${refereeId}`
  
  try {
    const refereeResponse = await baseAxios.patch<GetRefereeSchema>(url, updatedReferee)
    return formatRefereeResponse(
      refereeResponse.data.included,
      refereeResponse.data.data.attributes,
      refereeResponse.data.data.relationships,
      refereeResponse.data.data.id
    )
  } catch (err) {
    throw err
  }
}

export async function getReferees(filter: GetRefereesFilter): Promise<RefereesResponse> {
  const url = 'referees'
  const transformedFilter = camelToSnake(filter)

  try {
    const refereeResponse = await baseAxios.get<GetRefereesSchema>(url, { params: transformedFilter })
    const included = refereeResponse.data.included
    const referees = refereeResponse.data.data.map((ref) => {
      return formatRefereeResponse(
        included,
        ref.attributes,
        ref.relationships,
        ref.id
      )
    })

    return {
      meta: refereeResponse.data.meta,
      referees,
    }
  } catch (err) {
    throw err
  }
}
