import Faker from 'faker'
import { Factory } from 'fishery'

import { Data, TestLevel } from 'MainApp/schemas/getTestSchema'

export default Factory.define<Data>(({ sequence }): Data => ({
  attributes: {
    active: true,
    certificationId: 1,
    description: Faker.lorem.sentence(),
    level: TestLevel.Assistant,
    minimumPassPercentage: 80,
    name: `${Faker.name.jobType()} Test`,
    negativeFeedback: Faker.lorem.sentence(),
    newLanguageId: 1,
    positiveFeedback: Faker.lorem.sentence(),
    recertification: false,
    testableQuestionCount: 10,
    timeLimit: 18,
    updatedAt: Faker.date.recent().toString(),
  },
  id: sequence.toString(),
  type: 'test',
}))
