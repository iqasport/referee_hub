import { faker } from "@faker-js/faker";
import { Factory } from "fishery";
import { Datum } from "../schemas/getNationalGoverningBodiesSchema";
import { Region, MembershipStatus } from "../schemas/getNationalGoverningBodySchema";

export default Factory.define<Datum>(
  ({ sequence }): Datum => {
    const ngbCountry = faker.location.country();
    return {
      attributes: {
        name: `${ngbCountry} Quidditch`,
        website: faker.internet.url(),
        country: ngbCountry,
        acronym: `${ngbCountry[0].toUpperCase()}Q`,
        playerCount: faker.number.int(500),
        region: Region.Europe,
        logoUrl: faker.internet.avatar(),
        membershipStatus: MembershipStatus.Emerging,
      },
      id: sequence.toString(),
      relationships: {
        socialAccounts: { data: [] },
        teams: { data: [] },
        referees: { data: [] },
        stats: { data: [] },
      },
      type: "nationalGoverningBody",
    };
  }
);
