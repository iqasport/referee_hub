// To parse this data:
//
//   import { Convert, GetTeamsSchema } from "./file";
//
//   const getTeamsSchema = Convert.toGetTeamsSchema(json);
//
// These functions will throw an error if the JSON doesn't
// match the expected interface, even if the JSON is valid.

export interface GetTeamsSchema {
  data: Datum[];
  meta: Meta;
}

export interface Datum {
  id: string;
  type: DatumType;
  attributes: Attributes;
  relationships?: Relationships;
}

export interface Attributes {
  city: string;
  country: string;
  groupAffiliation: GroupAffiliation;
  name: string;
  state: string;
  status: Status;
  joinedAt: string;
}

export enum GroupAffiliation {
  University = "university",
  Community = "community",
  Youth = "youth",
}

export enum Status {
  Competitive = "competitive",
  Developing = "developing",
  Inactive = "inactive",
}

export interface Relationships {
  nationalGoverningBody: NationalGoverningBody;
}

export interface NationalGoverningBody {
  data: Data;
}

export interface Data {
  id: string;
  type: DataType;
}

export enum DataType {
  NationalGoverningBody = "nationalGoverningBody",
}

export enum DatumType {
  Team = "team",
}

export interface Meta {
  page: string;
  total: number;
}

// Converts JSON strings to/from your types
// and asserts the results of JSON.parse at runtime
export class Convert {
  public static toGetTeamsSchema(json: string): GetTeamsSchema {
    return cast(JSON.parse(json), r("GetTeamsSchema"));
  }

  public static getTeamsSchemaToJson(value: GetTeamsSchema): string {
    return JSON.stringify(uncast(value, r("GetTeamsSchema")), null, 2);
  }
}

function invalidValue(typ: any, val: any): never {
  throw Error(`Invalid value ${JSON.stringify(val)} for type ${JSON.stringify(typ)}`);
}

function jsonToJSProps(typ: any): any {
  if (typ.jsonToJS === undefined) {
    var map: any = {};
    typ.props.forEach((p: any) => (map[p.json] = { key: p.js, typ: p.typ }));
    typ.jsonToJS = map;
  }
  return typ.jsonToJS;
}

function jsToJSONProps(typ: any): any {
  if (typ.jsToJSON === undefined) {
    var map: any = {};
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
    var l = typs.length;
    for (var i = 0; i < l; i++) {
      var typ = typs[i];
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
    var result: any = {};
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
    return typ.hasOwnProperty("unionMembers")
      ? transformUnion(typ.unionMembers, val)
      : typ.hasOwnProperty("arrayItems")
      ? transformArray(typ.arrayItems, val)
      : typ.hasOwnProperty("props")
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
  GetTeamsSchema: o(
    [
      { json: "data", js: "data", typ: a(r("Datum")) },
      { json: "meta", js: "meta", typ: r("Meta") },
    ],
    false
  ),
  Datum: o(
    [
      { json: "id", js: "id", typ: "" },
      { json: "type", js: "type", typ: r("DatumType") },
      { json: "attributes", js: "attributes", typ: r("Attributes") },
      { json: "relationships", js: "relationships", typ: r("Relationships") },
    ],
    false
  ),
  Attributes: o(
    [
      { json: "city", js: "city", typ: "" },
      { json: "country", js: "country", typ: "" },
      { json: "groupAffiliation", js: "groupAffiliation", typ: r("GroupAffiliation") },
      { json: "name", js: "name", typ: r("Name") },
      { json: "state", js: "state", typ: "" },
      { json: "status", js: "status", typ: r("Status") },
    ],
    false
  ),
  Relationships: o(
    [
      {
        json: "nationalGoverningBody",
        js: "nationalGoverningBody",
        typ: r("NationalGoverningBody"),
      },
    ],
    false
  ),
  NationalGoverningBody: o([{ json: "data", js: "data", typ: r("Data") }], false),
  Data: o(
    [
      { json: "id", js: "id", typ: "" },
      { json: "type", js: "type", typ: r("DataType") },
    ],
    false
  ),
  Meta: o(
    [
      { json: "page", js: "page", typ: 0 },
      { json: "total", js: "total", typ: 0 },
    ],
    false
  ),
  GroupAffiliation: ["university"],
  Name: ["Virginia Academy of Administration Quidditch Club"],
  Status: ["competitive"],
  DataType: ["nationalGoverningBody"],
  DatumType: ["team"],
};
