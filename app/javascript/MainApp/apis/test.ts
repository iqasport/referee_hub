import axios from 'axios'
import { transform } from 'lodash'

import { HeadersMap } from 'MainApp/pages/ImportWizard/MapStep'
import { GetQuestionsSchema } from 'MainApp/schemas/getQuestionsSchema'
import { Attributes, Data, GetTestSchema } from '../schemas/getTestSchema'
import { Datum, GetTestsSchema, IncludedAttributes } from '../schemas/getTestsSchema'

import { QuestionsResponse } from './question'
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
    const certifications = testsResponse?.data?.included?.map((cert) => ({ id: cert.id, ...cert.attributes }))

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
    const certification = testResponse?.data?.included?.map((cert) => ({ id: cert.id, ...cert.attributes }))[0]

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
    const certification = testResponse?.data?.included?.map((cert) => ({ id: cert.id, ...cert.attributes }))[0]

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
    const certification = testResponse?.data?.included?.map((cert) => ({ id: cert.id, ...cert.attributes }))[0]

    return {
      certification,
      test: testResponse.data.data,
    }
  } catch (err) {
    throw err
  }
}

export async function importTests(file: File, mappedData: HeadersMap, testId: string): Promise<QuestionsResponse> {
  const url = `/api/v1/tests/${testId}/import`
  const reversedMap = transform(mappedData, (acc, value, key) => {
    acc[value] = key
    return acc
  }, {})

  try {
    const data = new FormData()
    data.append('file', file)
    data.append('mapped_headers', JSON.stringify(reversedMap))

    const questionsResponse = await axios.post<GetQuestionsSchema>(url, data)

    return {
      answers: questionsResponse.data.included,
      meta: questionsResponse.data.meta,
      questions: questionsResponse.data.data,
    }
  } catch (err) {
    throw err
  }
}

export async function deleteTest(id: string): Promise<TestResponse> {
  const url = `tests/${id}`

  try {
    const testResponse = await baseAxios.delete<GetTestSchema>(url)

    return {
      certification: null,
      test: testResponse.data.data,
    }
  } catch (err) {
    throw err
  }
};

export async function getRefereeTests(refId: string): Promise<TestsResponse> {
  const url = `referees/${refId}/tests`

  try {
    const testsResponse = await baseAxios.get<GetTestsSchema>(url)
    const certifications = testsResponse?.data?.included?.map((cert) => ({ id: cert.id, ...cert.attributes }))

    return {
      certifications,
      tests: testsResponse.data.data,
    }
  } catch (err) {
    throw err
  }
}
