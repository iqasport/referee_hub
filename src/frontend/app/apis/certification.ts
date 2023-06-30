import { Datum, GetCertificationsSchema } from "../schemas/getCertificationsSchema";
import { Data, GetRefereeCertificationSchema } from "../schemas/getRefereeCertificationSchema";

import { baseAxios } from "./utils";

export interface CertificationsResponse {
  certifications: Datum[];
}

export interface RefereeCertificationResponse {
  certification: Data;
}

export interface CreateCertificationRequest {
  refereeId: string;
  certificationId: string;
  receivedAt: string;
}

export interface UpdateCertificationRequest {
  refereeId: string;
  revokedAt: string;
}

export async function getCertifications(): Promise<CertificationsResponse> {
  const url = "certifications";

  try {
    const certResponse = await baseAxios.get<GetCertificationsSchema>(url);

    return {
      certifications: certResponse.data.data,
    };
  } catch (err) {
    throw err;
  }
}

export async function createCertification(
  certification: CreateCertificationRequest
): Promise<RefereeCertificationResponse> {
  const url = "referee_certifications";

  try {
    const refCertResponse = await baseAxios.post<GetRefereeCertificationSchema>(url, {
      ...certification,
    });

    return {
      certification: refCertResponse.data.data,
    };
  } catch (err) {
    throw err;
  }
}

export async function revokeCertification(
  certification: UpdateCertificationRequest,
  certificationId: string
): Promise<RefereeCertificationResponse> {
  const url = `referee_certifications/${certificationId}`;

  try {
    const refCertResponse = await baseAxios.patch<GetRefereeCertificationSchema>(url, {
      ...certification,
    });

    return {
      certification: refCertResponse.data.data,
    };
  } catch (err) {
    throw err;
  }
}
