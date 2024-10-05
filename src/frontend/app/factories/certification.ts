import { faker } from "@faker-js/faker";
import { Factory } from "fishery";

import { Datum } from "../schemas/getCertificationsSchema";

const levels = ["assistant", "snitch", "head", "field", "scorekeeper"];
const versions = ["eighteen", "twenty", "twentytwo", "twentyfour"];

export default Factory.define<Datum>(({ sequence }) => ({
  attributes: {
    level: faker.helpers.arrayElement(levels),
    version: faker.helpers.arrayElement(versions),
  },
  id: sequence.toString(),
  type: "certification",
}));
