import Faker from "faker";
import { Factory } from "fishery";
import { DateTime } from "luxon";

import { Datum, DatumType, GroupAffiliation, Status } from "MainApp/schemas/getTeamsSchema";

export default Factory.define<Datum>(
  ({ sequence }): Datum => ({
    attributes: {
      city: Faker.address.city(),
      country: Faker.address.countryCode(),
      name: Faker.company.companyName(),
      groupAffiliation: GroupAffiliation.Community,
      state: Faker.address.state(),
      status: Status.Competitive,
      joinedAt: DateTime.local().toString(),
    },
    id: sequence.toString(),
    type: DatumType.Team,
  })
);
