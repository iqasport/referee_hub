import Faker from "faker";
import { Factory } from "fishery";

import { Datum } from "../schemas/getLanguagesSchema";

export default Factory.define<Datum>(({ sequence }) => ({
  attributes: {
    longName: Faker.address.country(),
    shortName: Faker.address.countryCode(),
    shortRegion: Faker.address.state(),
  },
  id: sequence.toString(),
  type: "language",
}));
