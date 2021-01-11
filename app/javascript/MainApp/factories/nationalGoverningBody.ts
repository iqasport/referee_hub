import Faker from "faker";
import { Factory } from "fishery";
import { Datum } from "MainApp/schemas/getNationalGoverningBodiesSchema";
import { Region, MembershipStatus } from "MainApp/schemas/getNationalGoverningBodySchema";

export default Factory.define<Datum>(
  ({ sequence }): Datum => {
    const ngbCountry = Faker.address.country();
    return {
      attributes: {
        name: `${ngbCountry} Quidditch`,
        website: Faker.internet.url(),
        country: ngbCountry,
        acronym: `${ngbCountry[0].toUpperCase()}Q`,
        playerCount: Faker.random.number(500),
        region: Region.Europe,
        logoUrl: Faker.internet.avatar(),
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
