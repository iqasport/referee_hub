import { DataAttributes, GetRefereeSchema, Included, IncludedAttributes } from '../schemas/getRefereeSchema';
import { baseAxios } from './utils'

export interface GetRefereeResponse {
  referee: DataAttributes | null;
  id: string | null;
  ngbs: IncludedAttributes[] | null;
  testAttempts: IncludedAttributes[] | null;
  testResults: IncludedAttributes[] | null;
  certifications: IncludedAttributes[] | null;
}

const mapAttributes = (record: Included) => record.attributes

export async function getReferee(refereeId: string | number): Promise<GetRefereeResponse> {
  const url = `referees/${refereeId}`

  try {
    const refereeResponse = await baseAxios.get<GetRefereeSchema>(url)
    const ngbs = refereeResponse.data.included
      .filter((record: Included) => record.type === 'national_governing_body')
      .map(mapAttributes)
    const certifications = refereeResponse.data.included
      .filter((record: Included) => record.type === 'certifications')
      .map(mapAttributes)
    const testAttempts = refereeResponse.data.included
      .filter((record: Included) => record.type === 'test_attempts')
      .map(mapAttributes)
    const testResults = refereeResponse.data.included
      .filter((record: Included) => record.type === 'test_results')
      .map(mapAttributes)

    return {
      certifications,
      id: refereeResponse.data.data.id,
      ngbs,
      referee: {
        ...refereeResponse.data.data.attributes
      },
      testAttempts,
      testResults,
    }
  } catch (err) {
    throw err
  }
}
