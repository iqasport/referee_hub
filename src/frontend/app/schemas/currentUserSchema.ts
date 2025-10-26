// To parse this data:
//
//   import { Convert, CurrentUserSchema } from "./file";
//
//   const currentUserSchema = Convert.toCurrentUserSchema(json);
//
// These functions will throw an error if the JSON doesn't
// match the expected interface, even if the JSON is valid.

export interface CurrentUserSchema {
  data: Data;
  included: Included[];
}

export interface Data {
  id: string;
  type: string;
  attributes: DataAttributes;
  relationships: Relationships;
}

export interface DataAttributes {
  firstName: string;
  lastName: string;
  hasPendingPolicies: boolean;
  avatarUrl: string | null;
  enabledFeatures: string[];
  ownedNgbId: number | null;
  languageId?: number;
}

export interface Relationships {
  roles: Relationship;
  certificationPayments: Relationship;
  language: Relationship;
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
  accessType?: string;
  userId: number;
  certificationId?: number;
  stripeSessionId?: string;
  longName?: string;
  shortName?: string;
  shortRegion?: string;
}

// Converts JSON strings to/from your types
// and asserts the results of JSON.parse at runtime
export class Convert {
  public static toCurrentUserSchema(json: string): CurrentUserSchema {
    return cast(JSON.parse(json), r("CurrentUserSchema"));
  }

  public static currentUserSchemaToJson(value: CurrentUserSchema): string {
    return JSON.stringify(uncast(value, r("CurrentUserSchema")), null, 2);
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

  function transformDate(typ: any, val: any): any {
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
  if (typ === Date && typeof val !== "number") return transformDate(typ, val);
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
  CurrentUserSchema: o(
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
      { json: "firstName", js: "firstName", typ: "" },
      { json: "lastName", js: "lastName", typ: "" },
      { json: "hasPendingPolicies", js: "hasPendingPolicies", typ: true },
    ],
    false
  ),
  Relationships: o([{ json: "roles", js: "roles", typ: r("Roles") }], false),
  Roles: o([{ json: "data", js: "data", typ: a(r("Datum")) }], false),
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
      { json: "accessType", js: "accessType", typ: "" },
      { json: "userId", js: "userId", typ: 0 },
    ],
    false
  ),
};
