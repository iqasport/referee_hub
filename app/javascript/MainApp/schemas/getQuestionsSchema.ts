// To parse this data:
//
//   import { Convert, GetQuestionsSchema } from "./file";
//
//   const getQuestionsSchema = Convert.toGetQuestionsSchema(json);
//
// These functions will throw an error if the JSON doesn't
// match the expected interface, even if the JSON is valid.

export interface GetQuestionsSchema {
  data: GetQuestionsSchemaDatum[];
  included: Included[];
  meta?: Meta;
}

export interface GetQuestionsSchemaDatum {
  id: string;
  type: string;
  attributes: DatumAttributes;
  relationships: Relationships;
}

export interface DatumAttributes {
  description: string;
  feedback: string;
  testId: number;
  pointsAvailable: number;
}

export interface Relationships {
  answers: Answers;
}

export interface Answers {
  data: AnswersDatum[];
}

export interface AnswersDatum {
  id: string;
  type: DatumType;
}

export enum DatumType {
  Answer = "answer",
}

export interface Included {
  id: string;
  type: IncludedType;
  attributes: IncludedAttributes;
}

export interface IncludedAttributes {
  description: string;
  questionId: number;
  correct: boolean;
}

export enum IncludedType {
  AdminAnswer = "adminAnswer",
}

export interface Meta {
  page: number;
  total: number;
}

// Converts JSON strings to/from your types
// and asserts the results of JSON.parse at runtime
export class Convert {
  public static toGetQuestionsSchema(json: string): GetQuestionsSchema {
    return cast(JSON.parse(json), r("GetQuestionsSchema"));
  }

  public static getQuestionsSchemaToJson(value: GetQuestionsSchema): string {
    return JSON.stringify(uncast(value, r("GetQuestionsSchema")), null, 2);
  }
}

function invalidValue(typ: any, val: any): never {
  throw Error(`Invalid value ${JSON.stringify(val)} for type ${JSON.stringify(typ)}`);
}

function jsonToJSProps(typ: any): any {
  if (typ.jsonToJS === undefined) {
    const map: any = {};
    typ.props.forEach((p: any) => map[p.json] = { key: p.js, typ: p.typ });
    typ.jsonToJS = map;
  }
  return typ.jsonToJS;
}

function jsToJSONProps(typ: any): any {
  if (typ.jsToJSON === undefined) {
    const map: any = {};
    typ.props.forEach((p: any) => map[p.js] = { key: p.json, typ: p.typ });
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
      } catch (_) { }
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
    return val.map(el => transform(el, typ, getProps));
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
    Object.getOwnPropertyNames(props).forEach(key => {
      const prop = props[key];
      const v = Object.prototype.hasOwnProperty.call(val, key) ? val[key] : undefined;
      result[prop.key] = transform(v, prop.typ, getProps);
    });
    Object.getOwnPropertyNames(val).forEach(key => {
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
    return typ.hasOwnProperty("unionMembers") ? transformUnion(typ.unionMembers, val)
      : typ.hasOwnProperty("arrayItems") ? transformArray(typ.arrayItems, val)
        : typ.hasOwnProperty("props") ? transformObject(getProps(typ), typ.additional, val)
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
  "GetQuestionsSchema": o([
    { json: "data", js: "data", typ: a(r("GetQuestionsSchemaDatum")) },
    { json: "included", js: "included", typ: a(r("Included")) },
  ], false),
  "GetQuestionsSchemaDatum": o([
    { json: "id", js: "id", typ: "" },
    { json: "type", js: "type", typ: "" },
    { json: "attributes", js: "attributes", typ: r("DatumAttributes") },
    { json: "relationships", js: "relationships", typ: r("Relationships") },
  ], false),
  "DatumAttributes": o([
    { json: "description", js: "description", typ: "" },
    { json: "feedback", js: "feedback", typ: "" },
    { json: "testId", js: "testId", typ: 0 },
    { json: "pointsAvailable", js: "pointsAvailable", typ: 0 },
  ], false),
  "Relationships": o([
    { json: "answers", js: "answers", typ: r("Answers") },
  ], false),
  "Answers": o([
    { json: "data", js: "data", typ: a(r("AnswersDatum")) },
  ], false),
  "AnswersDatum": o([
    { json: "id", js: "id", typ: "" },
    { json: "type", js: "type", typ: r("DatumType") },
  ], false),
  "Included": o([
    { json: "id", js: "id", typ: "" },
    { json: "type", js: "type", typ: r("IncludedType") },
    { json: "attributes", js: "attributes", typ: r("IncludedAttributes") },
  ], false),
  "IncludedAttributes": o([
    { json: "description", js: "description", typ: "" },
    { json: "questionId", js: "questionId", typ: 0 },
    { json: "correct", js: "correct", typ: true },
  ], false),
  "DatumType": [
    "answer",
  ],
  "IncludedType": [
    "adminAnswer",
  ],
};
