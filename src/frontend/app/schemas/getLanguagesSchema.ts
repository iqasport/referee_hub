// To parse this data:
//
//   import { Convert, GetLanguagesSchema } from "./file";
//
//   const getLanguagesSchema = Convert.toGetLanguagesSchema(json);

export interface GetLanguagesSchema {
  data: Datum[];
}

export interface Datum {
  id: string;
  type: string;
  attributes: Attributes;
}

export interface Attributes {
  longName?: string;
  shortName?: string;
  shortRegion?: null | string;
}

// Converts JSON strings to/from your types
export class Convert {
  public static toGetLanguagesSchema(json: string): GetLanguagesSchema {
    return JSON.parse(json);
  }

  public static getLanguagesSchemaToJson(value: GetLanguagesSchema): string {
    return JSON.stringify(value);
  }
}
