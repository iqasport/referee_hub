import certificationFactory from './certification'
import currentUserFactory from './currentUser'
import languageFactory from './language'
import singleTestFactory from './singleTest'

export default {
  certification: certificationFactory,
  currentUser: currentUserFactory,
  language: languageFactory,
  test: singleTestFactory,
}
