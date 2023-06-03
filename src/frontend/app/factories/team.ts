import { faker } from "@faker-js/faker";
import { Factory } from "fishery";
import { DateTime } from "luxon";

import { Datum, DatumType, GroupAffiliation, Status } from "../schemas/getTeamsSchema";

export default Factory.define<Datum>(
  ({ sequence }): Datum => ({
    attributes: {
      city: faker.location.city(),
      country: faker.location.countryCode(),
      name: faker.company.name(),
      groupAffiliation: GroupAffiliation.Community,
      state: faker.location.state(),
      status: Status.Competitive,
      joinedAt: DateTime.local().toString(),
    },
    id: sequence.toString(),
    type: DatumType.Team,
  })
);
