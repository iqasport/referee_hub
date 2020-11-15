import Faker from 'faker'
import { Factory } from 'fishery'

import { Datum } from '../schemas/getCertificationsSchema'

export default Factory.define<Datum>(({ sequence }) => ({
  attributes: {
    level: 'assistant',
    version: 'twenty'
  },
  id: sequence.toString(),
  type: 'certification'
}))
