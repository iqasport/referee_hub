import Faker from "faker";
import { Factory } from "fishery";

import { Data } from "../schemas/currentUserSchema";

export default Factory.define<Data>(({ sequence }) => ({
  attributes: {
    avatarUrl: Faker.image.imageUrl(),
    enabledFeatures: [],
    firstName: Faker.name.firstName(),
    hasPendingPolicies: false,
    lastName: Faker.name.lastName(),
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
