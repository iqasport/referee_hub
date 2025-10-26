// To parse this data:
//
//   import { Convert, GetRefereesSchema } from "./file";
//
//   const getRefereesSchema = Convert.toGetRefereesSchema(json);
//
// These functions will throw an error if the JSON doesn't
// match the expected interface, even if the JSON is valid.

export interface GetRefereesSchema {
  data: Datum[];
  included: Included[];
  meta: Meta;
}

export interface Datum {
  id: string;
  type: Type;
  attributes: DatumAttributes;
  relationships: DatumRelationships;
}

export interface DatumAttributes {
  firstName: string;
  lastName: string;
  bio: null | string;
  showPronouns: boolean;
  submittedPaymentAt: null | string;
  exportName: boolean;
  avatarUrl: null | string;
  createdAt: string;
  isEditable: boolean;
  hasPendingPolicies: boolean;
  pronouns: string | null;
}

export interface DatumRelationships {
  refereeLocations: Relationship;
  nationalGoverningBodies: Relationship;
  refereeCertifications: Relationship;
  certifications: Relationship;
  testResults: Relationship;
  testAttempts: Relationship;
  teams: Relationship;
  refereeTeams: Relationship;
}

export interface Relationship {
  data: DAT[];
}

export interface DAT {
  id: string;
  type: Type;
}

export enum Type {
  Certification = "certification",
  NationalGoverningBody = "nationalGoverningBody",
  Referee = "referee",
  RefereeCertification = "refereeCertification",
  RefereeLocation = "refereeLocation",
  RefereeTeam = "refereeTeam",
  SocialAccount = "socialAccount",
  Team = "team",
}

export interface Included {
  id: string;
  type: Type;
  attributes: IncludedAttributes;
  relationships?: IncludedRelationships;
}

export interface IncludedAttributes {
  name?: string;
  website?: string;
  acronym?: string;
  playerCount?: number;
  region?: string;
  country?: string;
  logoUrl?: string;
  refereeId?: number;
  associationType?: AssociationType;
  nationalGoverningBodyId?: number;
  teamId?: number;
  city?: string;
  groupAffiliation?: string;
  state?: string;
  status?: string;
  level?: string;
  version?: string;
  needsRenewalAt?: null | string;
  receivedAt?: null | string;
  renewedAt?: null | string;
  revokedAt?: null | string;
  certificationId?: number;
}

export enum AssociationType {
  Primary = "primary",
  Secondary = "secondary",
  Player = "player",
  Coach = "coach",
}

export interface IncludedRelationships {
  socialAccounts?: Relationship;
  teams?: Relationship;
  referees?: Relationship;
  nationalGoverningBody?: Relationship;
}

export interface Meta {
  page: string;
  total: number;
}

// Converts JSON strings to/from your types
// and asserts the results of JSON.parse at runtime
export class Convert {
  public static toGetRefereesSchema(json: string): GetRefereesSchema {
    return cast(JSON.parse(json), r("GetRefereesSchema"));
  }

  public static getRefereesSchemaToJson(value: GetRefereesSchema): string {
    return JSON.stringify(uncast(value, r("GetRefereesSchema")), null, 2);
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
      } catch (_) {}
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
  GetRefereesSchema: o(
    [
      { json: "data", js: "data", typ: a(r("Datum")) },
      { json: "included", js: "included", typ: a(r("Included")) },
      { json: "meta", js: "meta", typ: r("Meta") },
    ],
    false
  ),
  Datum: o(
    [
      { json: "id", js: "id", typ: "" },
      { json: "type", js: "type", typ: r("Type") },
      { json: "attributes", js: "attributes", typ: r("DatumAttributes") },
      { json: "relationships", js: "relationships", typ: r("DatumRelationships") },
    ],
    false
  ),
  DatumAttributes: o(
    [
      { json: "firstName", js: "firstName", typ: "" },
      { json: "lastName", js: "lastName", typ: "" },
      { json: "bio", js: "bio", typ: null },
      { json: "showPronouns", js: "showPronouns", typ: true },
      { json: "submittedPaymentAt", js: "submittedPaymentAt", typ: null },
      { json: "exportName", js: "exportName", typ: true },
      { json: "avatarUrl", js: "avatarUrl", typ: null },
      { json: "createdAt", js: "createdAt", typ: "" },
      { json: "isEditable", js: "isEditable", typ: true },
      { json: "hasPendingPolicies", js: "hasPendingPolicies", typ: true },
    ],
    false
  ),
  DatumRelationships: o(
    [
      { json: "refereeLocations", js: "refereeLocations", typ: r("Certifications") },
      { json: "nationalGoverningBodies", js: "nationalGoverningBodies", typ: r("Certifications") },
      { json: "refereeCertifications", js: "refereeCertifications", typ: r("Certifications") },
      { json: "certifications", js: "certifications", typ: r("Certifications") },
      { json: "teams", js: "teams", typ: r("Certifications") },
      { json: "refereeTeams", js: "refereeTeams", typ: r("Certifications") },
    ],
    false
  ),
  Certifications: o([{ json: "data", js: "data", typ: a(r("DAT")) }], false),
  DAT: o(
    [
      { json: "id", js: "id", typ: "" },
      { json: "type", js: "type", typ: r("Type") },
    ],
    false
  ),
  Included: o(
    [
      { json: "id", js: "id", typ: "" },
      { json: "type", js: "type", typ: r("Type") },
      { json: "attributes", js: "attributes", typ: r("IncludedAttributes") },
      { json: "relationships", js: "relationships", typ: u(undefined, r("IncludedRelationships")) },
    ],
    false
  ),
  IncludedAttributes: o(
    [
      { json: "name", js: "name", typ: u(undefined, "") },
      { json: "website", js: "website", typ: u(undefined, "") },
      { json: "acronym", js: "acronym", typ: u(undefined, "") },
      { json: "playerCount", js: "playerCount", typ: u(undefined, 0) },
      { json: "region", js: "region", typ: u(undefined, "") },
      { json: "country", js: "country", typ: u(undefined, "") },
      { json: "logoUrl", js: "logoUrl", typ: u(undefined, "") },
      { json: "refereeId", js: "refereeId", typ: u(undefined, 0) },
      { json: "associationType", js: "associationType", typ: u(undefined, r("AssociationType")) },
      { json: "nationalGoverningBodyId", js: "nationalGoverningBodyId", typ: u(undefined, 0) },
      { json: "teamId", js: "teamId", typ: u(undefined, 0) },
      { json: "city", js: "city", typ: u(undefined, "") },
      { json: "groupAffiliation", js: "groupAffiliation", typ: u(undefined, "") },
      { json: "state", js: "state", typ: u(undefined, "") },
      { json: "status", js: "status", typ: u(undefined, "") },
      { json: "level", js: "level", typ: u(undefined, "") },
      { json: "version", js: "version", typ: u(undefined, "") },
      { json: "needsRenewalAt", js: "needsRenewalAt", typ: u(undefined, null) },
      { json: "receivedAt", js: "receivedAt", typ: u(undefined, "") },
      { json: "renewedAt", js: "renewedAt", typ: u(undefined, u(null, "")) },
      { json: "revokedAt", js: "revokedAt", typ: u(undefined, null) },
      { json: "certificationId", js: "certificationId", typ: u(undefined, 0) },
    ],
    false
  ),
  IncludedRelationships: o(
    [
      { json: "socialAccounts", js: "socialAccounts", typ: u(undefined, r("Certifications")) },
      { json: "teams", js: "teams", typ: u(undefined, r("Certifications")) },
      { json: "referees", js: "referees", typ: u(undefined, r("Certifications")) },
      {
        json: "nationalGoverningBody",
        js: "nationalGoverningBody",
        typ: u(undefined, r("NationalGoverningBody")),
      },
    ],
    false
  ),
  NationalGoverningBody: o([{ json: "data", js: "data", typ: r("DAT") }], false),
  Meta: o(
    [
      { json: "page", js: "page", typ: 0 },
      { json: "total", js: "total", typ: 0 },
    ],
    false
  ),
  Type: [
    "certification",
    "nationalGoverningBody",
    "referee",
    "refereeCertification",
    "refereeLocation",
    "refereeTeam",
    "socialAccount",
    "team",
  ],
  AssociationType: ["player", "primary"],
};
