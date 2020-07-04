import { Attributes, Data, GetTestSchema } from '../schemas/getTestSchema'
import { Datum, GetTestsSchema } from '../schemas/getTestsSchema'

import { baseAxios } from './utils'

export interface TestsResponse {
  tests: Datum[];
}

export interface TestResponse {
  test: Data
}

export interface UpdateTestRequest extends Omit<Attributes, 'updatedAt'> {}

export async function getTests(): Promise<TestsResponse> {
  const url = 'tests'

  try {
    const testsResponse = await baseAxios.get<GetTestsSchema>(url)

    return {
      tests: testsResponse.data.data
    }
  } catch (err) {
    throw err
  }
}

export async function getTest(id: string): Promise<TestResponse> {
  const url = `tests/${id}`

  try {
    const testResponse = await baseAxios.get<GetTestSchema>(url)

    return {
      test: testResponse.data.data
    }
  } catch (err) {
    throw err
  }
}

export async function updateTest(id: string, updatedTest: UpdateTestRequest): Promise<TestResponse> {
  const url = `tests/${id}`

  try {
    const testResponse = await baseAxios.patch<GetTestSchema>(url, {...updatedTest})

    return {
      test: testResponse.data.data
    }
  } catch (err) {
    throw err
  }
}

export async function createTest(newTest: UpdateTestRequest): Promise<TestResponse> {
  const url = 'tests'

  try {
    const testResponse = await baseAxios.post<GetTestSchema>(url, { ...newTest})

    return {
      test: testResponse.data.data
    }
  } catch (err) {
    throw err
  }
}
