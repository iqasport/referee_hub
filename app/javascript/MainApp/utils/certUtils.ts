import { IdAttributes } from "MainApp/apis/single_test";
import { IncludedAttributes } from "MainApp/schemas/getRefereeSchema";

export const getVersion = (version: string): string => {
  switch (version) {
    case "eighteen":
      return "2018-2020";
    case "twenty":
      return "2020-2022";
    case "twentytwo":
      return "2022-2023";
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
