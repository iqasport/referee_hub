import { faker } from "@faker-js/faker";
import { Factory } from "fishery";

import { Data } from "../schemas/currentUserSchema";

export default Factory.define<Data>(({ sequence }) => ({
  attributes: {
    avatarUrl: faker.image.url(),
    enabledFeatures: [],
    firstName: faker.person.firstName(),
    hasPendingPolicies: false,
    lastName: faker.person.lastName(),
    ownedNgbId: null,
  },
  id: sequence.toString(),
  relationships: {
    certificationPayments: {
      data: [],
    },
    language: {
      data: [],
    },
    roles: {
      data: [],
    },
  },
  type: "currentUser",
}));
