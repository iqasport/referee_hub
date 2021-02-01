import Faker from "faker";
import { Factory } from "fishery";

import { Datum } from "../schemas/getCertificationsSchema";

const levels = ["assistant", "snitch", "head", "field", "scorekeeper"];
const versions = ["eighteen", "twenty"];

export default Factory.define<Datum>(({ sequence }) => ({
  attributes: {
    level: Faker.helpers.randomize(levels),
    version: Faker.helpers.randomize(versions),
  },
  id: sequence.toString(),
  type: "certification",
}));
