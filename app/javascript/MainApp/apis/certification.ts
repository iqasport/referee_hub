import { Datum, GetCertificationsSchema } from 'MainApp/schemas/getCertificationsSchema'

import { baseAxios } from './utils'

export interface CertificationsResponse {
  certifications: Datum[];
}

export async function getCertifications(): Promise<CertificationsResponse> {
  const url = 'certifications'

  try {
    const certResponse = await baseAxios.get<GetCertificationsSchema>(url)

    return {
      certifications: certResponse.data.data
    }
  } catch (err) {
    throw err
  }
}
