import { DateTime } from "luxon"
import { GetJobSchema } from "../schemas/getJobSchema"
import { baseAxios } from "./utils"

export interface JobResponse {
  jobId: string;
}

export type RefereeAnswer = {
  question_id: string;
  answer_id: string;
}

export interface FinishTestRequest {
  startedAt: DateTime;
  finishedAt: DateTime;
  refereeAnswers: RefereeAnswer[]
}

export async function exportNgbTeams(ngbId: string) {
  const url = 'ngb-admin/teams_export'

  try {
    const jobResponse = await baseAxios.get<GetJobSchema>(url, {
      params: {
        'national_governing_bodies': [parseInt(ngbId, 10)]
      }
    })

    return {
      jobId: jobResponse.data.data.job_id,
    }
  } catch (err) {
    throw err
  }
}

export async function exportNgbReferees(ngbId: string) {
  const url = 'referees_export'

  try {
    const jobResponse = await baseAxios.get<GetJobSchema>(url, {
      params: {
        'national_governing_bodies': [parseInt(ngbId, 10)]
      }
    })

    return {
      jobId: jobResponse.data.data.job_id,
    }
  } catch (err) {
    throw err
  }
}

export async function finishTest(testId: string, request: FinishTestRequest): Promise<JobResponse> {
  const url = `tests/${testId}/finish`

  try {
    const jobResponse = await baseAxios.post<GetJobSchema>(url, request)

    return {
      jobId: jobResponse.data.data.job_id
    }
  } catch (err) {
    throw err
  }
}

export async function exportTest(testId: string): Promise<JobResponse> {
  const url = `tests/${testId}/export`

  try {
    const jobResponse = await baseAxios.get<GetJobSchema>(url)

    return {
      jobId: jobResponse.data.data.job_id
    }
  } catch (err) {
    throw err
  }
}