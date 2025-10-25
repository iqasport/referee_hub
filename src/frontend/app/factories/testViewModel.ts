import { faker } from "@faker-js/faker";
import { Factory } from "fishery";

import { TestViewModel, CertificationLevel, CertificationVersion } from "../store/serviceApi";

export default Factory.define<TestViewModel>(
  ({ sequence }): TestViewModel => ({
    testId: sequence.toString(),
    title: `${faker.person.jobType()} Test`,
    description: faker.lorem.sentence(),
    language: "en-US",
    awardedCertification: {
      level: "assistant" as CertificationLevel,
      version: "twentyfour" as CertificationVersion,
    },
    timeLimit: 18,
    passPercentage: 80,
    questionsCount: 10,
    recertification: false,
    positiveFeedback: faker.lorem.sentence(),
    negativeFeedback: faker.lorem.sentence(),
    active: true,
  })
);
