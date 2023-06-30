import { faker } from "@faker-js/faker";
import { Factory } from "fishery";

import { Datum } from "../schemas/getLanguagesSchema";

export default Factory.define<Datum>(({ sequence }) => ({
  attributes: {
    longName: faker.location.country(),
    shortName: faker.location.countryCode(),
    shortRegion: faker.location.state(),
  },
  id: sequence.toString(),
  type: "language",
}));
