import { GetJobSchema } from "../schemas/getJobSchema"
import { baseAxios } from "./utils"

export interface JobResponse {
  jobId: string;
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
