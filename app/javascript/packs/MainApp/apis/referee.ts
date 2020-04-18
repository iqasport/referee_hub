import { AxiosResponse } from 'axios';
import { DataAttributes, GetRefereeSchema, Included, IncludedAttributes } from '../schemas/getRefereeSchema';
import { baseAxios } from './utils'

export interface RefereeResponse {
  referee: DataAttributes | null;
  id: string | null;
  ngbs: IncludedAttributes[] | null;
  testAttempts: IncludedAttributes[] | null;
  testResults: IncludedAttributes[] | null;
  certifications: IncludedAttributes[] | null;
  locations: IncludedAttributes[] | null;
  teams: IncludedAttributes[] | null;
}

export interface AssociationData {
  [key: string]: string;
}
type ForbiddenUpdates = 'firstName' | 'lastName' | 'isEditable' | 'hasPendingPolicies'
export interface UpdateRefereeRequest extends Omit<DataAttributes, ForbiddenUpdates> {
  teamsData: AssociationData | null;
  ngbData: AssociationData | null; 
}

const mapAttributes = (record: Included) => record.attributes

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
    .map((record: Included): IncludedAttributes => ({...record.attributes, nationalGoverningBodyId: parseInt(record.id, 10)}))
  const teamsData = response.data.included
    .filter((record: Included) => record.type === "team")
    .map((record: Included): IncludedAttributes => ({...record.attributes, teamId: parseInt(record.id, 10)}))
  const teams = response.data.included
    .filter((record: Included) => record.type === "refereeTeam")
    .map((refereeTeam: Included): IncludedAttributes => {
      const team = teamsData.find((teamData: IncludedAttributes) => teamData.teamId === refereeTeam.attributes.teamId)
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
  const url = `referees/${refereeId}`

  try {
    const refereeResponse = await baseAxios.patch<GetRefereeSchema>(url, updatedReferee)
    return formatRefereeResponse(refereeResponse)
  } catch (err) {
    throw err
  }
}
