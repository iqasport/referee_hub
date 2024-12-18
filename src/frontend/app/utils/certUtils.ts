import { IdAttributes } from "../apis/single_test";
import { IncludedAttributes } from "../schemas/getRefereeSchema";

export const getVersion = (version: string): string => {
  switch (version) {
    case "eighteen":
      return "2018-2020";
    case "twenty":
      return "2020-2021";
    case "twentytwo":
      return "2022-2023";
    case "twentyfour":
      return "2024";
    default:
      return "Unknown";
  }
};

export const getTestCertVersion = (certId: number, certifications: IdAttributes[]): string => {
  const certVersion = certifications.find((cert) => cert.id === certId.toString()).version;

  return getVersion(certVersion);
};

export const getRefereeCertVersion = (certification: IncludedAttributes): string => {
  return getVersion(certification.version);
};
