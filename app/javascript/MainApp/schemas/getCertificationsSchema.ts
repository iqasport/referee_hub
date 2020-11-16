export interface GetCertificationsSchema {
  data: Datum[];
}

export interface Datum {
  id: string;
  type: string;
  attributes: Attributes;
}

export interface Attributes {
  level: string;
  version: string;
}
