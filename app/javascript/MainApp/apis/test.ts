import { Datum, GetTestsSchema } from '../schemas/getTestsSchema'
import { baseAxios } from './utils'

export interface TestsResponse {
  tests: Datum[];
}

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
