// To parse this data:
//
//   import { Convert, GetRefereeSchema } from "./file";
//
//   const getRefereeSchema = Convert.toGetRefereeSchema(json);
//
// These functions will throw an error if the JSON doesn't
// match the expected interface, even if the JSON is valid.

export interface GetRefereeSchema {
    data:     Data;
    included: Included[];
}

export interface Data {
    id:            string;
    type:          string;
    attributes:    DataAttributes;
    relationships: Relationships;
}

export interface DataAttributes {
    firstName:                 string | null;
    lastName:                  string | null;
    bio:                       string | null;
    showPronouns:              boolean;
    submittedPaymentAt:        Date | string | null;
    exportName:                boolean;
    pronouns:                  string | null;
    isEditable:                boolean;
    hasPendingPolicies:        boolean;
    avatarUrl:                 string | null;
    createdAt:                 string;
}

export interface Relationships {
    refereeLocations:        Relationship;
    nationalGoverningBodies: Relationship;
    refereeCertifications:   Relationship;
    certifications:          Relationship;
    testResults:             Relationship;
    testAttempts:            Relationship;
}

export interface Relationship {
    data: Datum[];
}

export interface Datum {
    id:   string;
    type: string;
}

export interface Included {
    id:         string;
    type:       string;
    attributes: IncludedAttributes;
}

export enum AssociationType {
    Primary = "primary",
    Secondary = "secondary",
    Player = "player",
    Coach = "coach",
}

export interface IncludedAttributes {
    level?:                   string;
    name?:                    string;
    website?:                 string;
    needsRenewalAt?:          null;
    receivedAt?:              null | string;
    renewedAt?:               null;
    revokedAt?:               null;
    refereeId?:               number;
    certificationId?:         number;
    associationType?:         AssociationType;
    nationalGoverningBodyId?: number;
    nextAttemptAt?:           string;
    testLevel?:               string;
    duration?:                null;
    minimumPassPercentage?:   number;
    passed?:                  boolean;
    percentage?:              number;
    pointsAvailable?:         number;
    pointsScored?:            number;
    timeFinished?:            string;
    timeStarted?:             string;
    teamId?:                  number;
    version?:                 string;
    createdAt?:               string;
}

// Converts JSON strings to/from your types
// and asserts the results of JSON.parse at runtime
export class Convert {
    public static toGetRefereeSchema(json: string): GetRefereeSchema {
        return cast(JSON.parse(json), r("GetRefereeSchema"));
    }

    public static getRefereeSchemaToJson(value: GetRefereeSchema): string {
        return JSON.stringify(uncast(value, r("GetRefereeSchema")), null, 2);
    }
}

function invalidValue(typ: any, val: any): never {
    throw Error(`Invalid value ${JSON.stringify(val)} for type ${JSON.stringify(typ)}`);
}

function jsonToJSProps(typ: any): any {
    if (typ.jsonToJS === undefined) {
        var map: any = {};
        typ.props.forEach((p: any) => map[p.json] = { key: p.js, typ: p.typ });
        typ.jsonToJS = map;
    }
    return typ.jsonToJS;
}

function jsToJSONProps(typ: any): any {
    if (typ.jsToJSON === undefined) {
        var map: any = {};
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
        return val.map(el => transform(el, typ, getProps));
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
            : typ.hasOwnProperty("arrayItems")    ? transformArray(typ.arrayItems, val)
            : typ.hasOwnProperty("props")         ? transformObject(getProps(typ), typ.additional, val)
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
    "GetRefereeSchema": o([
        { json: "data", js: "data", typ: r("Data") },
        { json: "included", js: "included", typ: a(r("Included")) },
    ], false),
    "Data": o([
        { json: "id", js: "id", typ: "" },
        { json: "type", js: "type", typ: "" },
        { json: "attributes", js: "attributes", typ: r("DataAttributes") },
        { json: "relationships", js: "relationships", typ: r("Relationships") },
    ], false),
    "DataAttributes": o([
        { json: "firstName", js: "firstName", typ: "" },
        { json: "lastName", js: "lastName", typ: "" },
        { json: "bio", js: "bio", typ: "" },
        { json: "showPronouns", js: "showPronouns", typ: true },
        { json: "submittedPaymentAt", js: "submittedPaymentAt", typ: null },
        { json: "gettingStartedDismissedAt", js: "gettingStartedDismissedAt", typ: null },
        { json: "exportName", js: "exportName", typ: true },
        { json: "pronouns", js: "pronouns", typ: "" },
        { json: "isEditable", js: "isEditable", typ: true },
        { json: "hasPendingPolicies", js: "hasPendingPolicies", typ: true },
    ], false),
    "Relationships": o([
        { json: "refereeLocations", js: "refereeLocations", typ: r("Certifications") },
        { json: "nationalGoverningBodies", js: "nationalGoverningBodies", typ: r("Certifications") },
        { json: "refereeCertifications", js: "refereeCertifications", typ: r("Certifications") },
        { json: "certifications", js: "certifications", typ: r("Certifications") },
        { json: "testResults", js: "testResults", typ: r("Certifications") },
        { json: "testAttempts", js: "testAttempts", typ: r("Certifications") },
    ], false),
    "Certifications": o([
        { json: "data", js: "data", typ: a(r("Datum")) },
    ], false),
    "Datum": o([
        { json: "id", js: "id", typ: "" },
        { json: "type", js: "type", typ: "" },
    ], false),
    "Included": o([
        { json: "id", js: "id", typ: "" },
        { json: "type", js: "type", typ: "" },
        { json: "attributes", js: "attributes", typ: r("IncludedAttributes") },
    ], false),
    "IncludedAttributes": o([
        { json: "level", js: "level", typ: u(undefined, "") },
        { json: "name", js: "name", typ: u(undefined, "") },
        { json: "website", js: "website", typ: u(undefined, "") },
        { json: "needsRenewalAt", js: "needsRenewalAt", typ: u(undefined, null) },
        { json: "receivedAt", js: "receivedAt", typ: u(undefined, u(null, "")) },
        { json: "renewedAt", js: "renewedAt", typ: u(undefined, null) },
        { json: "revokedAt", js: "revokedAt", typ: u(undefined, null) },
        { json: "refereeId", js: "refereeId", typ: u(undefined, 0) },
        { json: "certificationId", js: "certificationId", typ: u(undefined, 0) },
        { json: "associationType", js: "associationType", typ: u(undefined, "") },
        { json: "nationalGoverningBodyId", js: "nationalGoverningBodyId", typ: u(undefined, 0) },
        { json: "nextAttemptAt", js: "nextAttemptAt", typ: u(undefined, "") },
        { json: "testLevel", js: "testLevel", typ: u(undefined, "") },
        { json: "duration", js: "duration", typ: u(undefined, null) },
        { json: "minimumPassPercentage", js: "minimumPassPercentage", typ: u(undefined, 0) },
        { json: "passed", js: "passed", typ: u(undefined, true) },
        { json: "percentage", js: "percentage", typ: u(undefined, 0) },
        { json: "pointsAvailable", js: "pointsAvailable", typ: u(undefined, 0) },
        { json: "pointsScored", js: "pointsScored", typ: u(undefined, 0) },
        { json: "timeFinished", js: "timeFinished", typ: u(undefined, "") },
        { json: "timeStarted", js: "timeStarted", typ: u(undefined, "") },
    ], false),
};
