import { faker } from "@faker-js/faker";
import { Factory } from "fishery";
import { DateTime } from "luxon";

import { Data, TestLevel } from "../schemas/getTestSchema";

export default Factory.define<Data>(
  ({ sequence }): Data => ({
    attributes: {
      active: true,
      certificationId: 1,
      description: faker.lorem.sentence(),
      level: TestLevel.Assistant,
      minimumPassPercentage: 80,
      name: `${faker.person.jobType()} Test`,
      negativeFeedback: faker.lorem.sentence(),
      newLanguageId: 1,
      positiveFeedback: faker.lorem.sentence(),
      recertification: false,
      testableQuestionCount: 10,
      timeLimit: 18,
      updatedAt: DateTime.local().toString(),
    },
    id: sequence.toString(),
    type: "test",
  })
);
