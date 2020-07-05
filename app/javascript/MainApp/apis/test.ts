import { Attributes, Data, GetTestSchema } from '../schemas/getTestSchema'
import { Datum, GetTestsSchema, IncludedAttributes } from '../schemas/getTestsSchema'

import { baseAxios } from './utils'

export interface IdAttributes extends IncludedAttributes {
  id: string;
}

export interface TestsResponse {
  tests: Datum[];
  certifications: IdAttributes[]
}

export interface TestResponse {
  test: Data;
  certification: IdAttributes;
}

export interface UpdateTestRequest extends Omit<Attributes, 'updatedAt'> {}

export async function getTests(): Promise<TestsResponse> {
  const url = 'tests'

  try {
    const testsResponse = await baseAxios.get<GetTestsSchema>(url)
    const certifications = testsResponse.data.included.map((cert) => ({ id: cert.id, ...cert.attributes }))

    return {
      certifications,
      tests: testsResponse.data.data,
    }
  } catch (err) {
    throw err
  }
}

export async function getTest(id: string): Promise<TestResponse> {
  const url = `tests/${id}`

  try {
    const testResponse = await baseAxios.get<GetTestSchema>(url)
    const certification = testResponse.data.included.map((cert) => ({ id: cert.id, ...cert.attributes }))[0]

    return {
      certification,
      test: testResponse.data.data,
    }
  } catch (err) {
    throw err
  }
}

export async function updateTest(id: string, updatedTest: UpdateTestRequest): Promise<TestResponse> {
  const url = `tests/${id}`

  try {
    const testResponse = await baseAxios.patch<GetTestSchema>(url, {...updatedTest})
    const certification = testResponse.data.included.map((cert) => ({ id: cert.id, ...cert.attributes }))[0]

    return {
      certification,
      test: testResponse.data.data,
    }
  } catch (err) {
    throw err
  }
}

export async function createTest(newTest: UpdateTestRequest): Promise<TestResponse> {
  const url = 'tests'

  try {
    const testResponse = await baseAxios.post<GetTestSchema>(url, { ...newTest})
    const certification = testResponse.data.included.map((cert) => ({ id: cert.id, ...cert.attributes }))[0]

    return {
      certification,
      test: testResponse.data.data,
    }
  } catch (err) {
    throw err
  }
}
