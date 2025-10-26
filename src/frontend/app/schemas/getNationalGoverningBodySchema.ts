// To parse this data:
//
//   import { Convert, GetNationalGoverningBodySchema } from "./file";
//
//   const getNationalGoverningBodySchema = Convert.toGetNationalGoverningBodySchema(json);
//
// These functions will throw an error if the JSON doesn't
// match the expected interface, even if the JSON is valid.

export interface GetNationalGoverningBodySchema {
  data: Data;
  included: Included[];
}

export interface Data {
  id: string;
  type: string;
  attributes: DataAttributes;
  relationships: Relationships;
}

export enum Region {
  NorthAmerica = "north_america",
  SouthAmerica = "south_america",
  Europe = "europe",
  Africa = "africa",
  Asia = "asia",
}

export enum MembershipStatus {
  AreaOfInterest = "area_of_interest",
  Emerging = "emerging",
  Developing = "developing",
  Full = "full",
}

export interface DataAttributes {
  name: string;
  website: string;
  acronym: string;
  playerCount: number;
  region: Region;
  country: string;
  logoUrl: string | null;
  membershipStatus: MembershipStatus;
}

export interface Relationships {
  socialAccounts: Relationship;
  teams: Relationship;
  referees: Relationship;
  stats: Relationship;
}

export interface Relationship {
  data: Datum[];
}

export interface Datum {
  id: string;
  type: string;
}

export interface Included {
  id: string;
  type: string;
  attributes: IncludedAttributes;
}

export interface IncludedAttributes {
  accountType?: string;
  url?: string;
  assistantRefereesCount?: number;
  communityTeamsCount?: number;
  competitiveTeamsCount?: number;
  developingTeamsCount?: number;
  headRefereesCount?: number;
  inactiveTeamsCount?: number;
  snitchRefereesCount?: number;
  teamStatusChangeCount?: number;
  totalRefereesCount?: number;
  totalTeamsCount?: number;
  uncertifiedCount?: number;
  universityTeamsCount?: number;
  youthTeamsCount?: number;
  start?: string;
  endTime?: string;
}

// Converts JSON strings to/from your types
// and asserts the results of JSON.parse at runtime
export class Convert {
  public static toGetNationalGoverningBodySchema(json: string): GetNationalGoverningBodySchema {
    return cast(JSON.parse(json), r("GetNationalGoverningBodySchema"));
  }

  public static getNationalGoverningBodySchemaToJson(
    value: GetNationalGoverningBodySchema
  ): string {
    return JSON.stringify(uncast(value, r("GetNationalGoverningBodySchema")), null, 2);
  }
}

function invalidValue(typ: any, val: any): never {
  throw Error(`Invalid value ${JSON.stringify(val)} for type ${JSON.stringify(typ)}`);
}

function jsonToJSProps(typ: any): any {
  if (typ.jsonToJS === undefined) {
    const map: any = {};
    typ.props.forEach((p: any) => (map[p.json] = { key: p.js, typ: p.typ }));
    typ.jsonToJS = map;
  }
  return typ.jsonToJS;
}

function jsToJSONProps(typ: any): any {
  if (typ.jsToJSON === undefined) {
    const map: any = {};
    typ.props.forEach((p: any) => (map[p.js] = { key: p.json, typ: p.typ }));
    typ.jsToJSON = map;
  }
  return typ.jsToJSON;
}

function transform(val: any, typ: any, getProps: any): any {
  function transformPrimitive(typ: string, val: any): any {
    if (typeof typ === typeof val) return val;
    return invalidValue(typ, val);
  }

  function transformUnion(typs: any[], val: any): any {
    // val must validate against one typ in typs
    const l = typs.length;
    for (let i = 0; i < l; i++) {
      const typ = typs[i];
      try {
        return transform(val, typ, getProps);
      } catch (_) {
        // Ignore error and try next type
      }
    }
    return invalidValue(typs, val);
  }

  function transformEnum(cases: string[], val: any): any {
    if (cases.indexOf(val) !== -1) return val;
    return invalidValue(cases, val);
  }

  function transformArray(typ: any, val: any): any {
    // val must be an array with no invalid elements
    if (!Array.isArray(val)) return invalidValue("array", val);
    return val.map((el) => transform(el, typ, getProps));
  }

  function transformDate(val: any): any {
    if (val === null) {
      return null;
    }
    const d = new Date(val);
    if (isNaN(d.valueOf())) {
      return invalidValue("Date", val);
    }
    return d;
  }

  function transformObject(props: { [k: string]: any }, additional: any, val: any): any {
    if (val === null || typeof val !== "object" || Array.isArray(val)) {
      return invalidValue("object", val);
    }
    const result: any = {};
    Object.getOwnPropertyNames(props).forEach((key) => {
      const prop = props[key];
      const v = Object.prototype.hasOwnProperty.call(val, key) ? val[key] : undefined;
      result[prop.key] = transform(v, prop.typ, getProps);
    });
    Object.getOwnPropertyNames(val).forEach((key) => {
      if (!Object.prototype.hasOwnProperty.call(props, key)) {
        result[key] = transform(val[key], additional, getProps);
      }
    });
    return result;
  }

  if (typ === "any") return val;
  if (typ === null) {
    if (val === null) return val;
    return invalidValue(typ, val);
  }
  if (typ === false) return invalidValue(typ, val);
  while (typeof typ === "object" && typ.ref !== undefined) {
    typ = typeMap[typ.ref];
  }
  if (Array.isArray(typ)) return transformEnum(typ, val);
  if (typeof typ === "object") {
    return Object.prototype.hasOwnProperty.call(typ, "unionMembers")
      ? transformUnion(typ.unionMembers, val)
      : Object.prototype.hasOwnProperty.call(typ, "arrayItems")
      ? transformArray(typ.arrayItems, val)
      : Object.prototype.hasOwnProperty.call(typ, "props")
      ? transformObject(getProps(typ), typ.additional, val)
      : invalidValue(typ, val);
  }
  // Numbers can be parsed by Date but shouldn't be.
  if (typ === Date && typeof val !== "number") return transformDate(val);
  return transformPrimitive(typ, val);
}

function cast<T>(val: any, typ: any): T {
  return transform(val, typ, jsonToJSProps);
}

function uncast<T>(val: T, typ: any): any {
  return transform(val, typ, jsToJSONProps);
}

function a(typ: any) {
  return { arrayItems: typ };
}

function u(...typs: any[]) {
  return { unionMembers: typs };
}

function o(props: any[], additional: any) {
  return { props, additional };
}

function m(additional: any) {
  return { props: [], additional };
}

function r(name: string) {
  return { ref: name };
}

const typeMap: any = {
  GetNationalGoverningBodySchema: o(
    [
      { json: "data", js: "data", typ: r("Data") },
      { json: "included", js: "included", typ: a(r("Included")) },
    ],
    false
  ),
  Data: o(
    [
      { json: "id", js: "id", typ: "" },
      { json: "type", js: "type", typ: "" },
      { json: "attributes", js: "attributes", typ: r("DataAttributes") },
      { json: "relationships", js: "relationships", typ: r("Relationships") },
    ],
    false
  ),
  DataAttributes: o(
    [
      { json: "name", js: "name", typ: "" },
      { json: "website", js: "website", typ: "" },
      { json: "acronym", js: "acronym", typ: "" },
      { json: "playerCount", js: "playerCount", typ: 0 },
      { json: "region", js: "region", typ: null },
      { json: "country", js: "country", typ: null },
    ],
    false
  ),
  Relationships: o(
    [
      { json: "socialAccounts", js: "socialAccounts", typ: r("Referees") },
      { json: "teams", js: "teams", typ: r("Referees") },
      { json: "referees", js: "referees", typ: r("Referees") },
    ],
    false
  ),
  Referees: o([{ json: "data", js: "data", typ: a(r("Datum")) }], false),
  Datum: o(
    [
      { json: "id", js: "id", typ: "" },
      { json: "type", js: "type", typ: "" },
    ],
    false
  ),
  Included: o(
    [
      { json: "id", js: "id", typ: "" },
      { json: "type", js: "type", typ: "" },
      { json: "attributes", js: "attributes", typ: r("IncludedAttributes") },
    ],
    false
  ),
  IncludedAttributes: o(
    [
      { json: "accountType", js: "accountType", typ: "" },
      { json: "url", js: "url", typ: "" },
    ],
    false
  ),
};
