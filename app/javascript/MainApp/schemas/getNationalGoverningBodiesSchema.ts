import { Region, MembershipStatus } from "./getNationalGoverningBodySchema";

export interface GetNationalGoverningBodiesSchema {
    data: Datum[];
    meta: Meta;
}

export interface Datum {
    id:         string;
    type:       Type;
    attributes: Attributes;
    relationships: Relationships;
}

export interface Attributes {
    name:        string;
    website:     string;
    acronym:     string;
    playerCount: number;
    region:      Region;
    country:     string;
    logoUrl:     string | null;
    membershipStatus: MembershipStatus;
}

export interface Relationships {
    socialAccounts: Relationship;
    teams: Relationship;
    referees: Relationship;
    stats: Relationship;
}

export interface Relationship {
    data: RelationshipDatum[];
}

export interface RelationshipDatum {
    id: string;
    type: RealtionshipType;
}

export enum RealtionshipType {
    Referee = "referee",
    SocialAccount = "socialAccount",
    Stat = "stat",
    Team = "team",
}

export enum Type {
    NationalGoverningBody = "nationalGoverningBody",
}

export interface Meta {
    page: string;
    total: number;
}
