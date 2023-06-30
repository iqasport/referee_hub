import certificationFactory from "./certification";
import currentUserFactory from "./currentUser";
import languageFactory from "./language";
import singleTestFactory from "./singleTest";
import teamFactory from "./team";
import ngbFactory from "./nationalGoverningBody";

export default {
  certification: certificationFactory,
  currentUser: currentUserFactory,
  ngb: ngbFactory,
  language: languageFactory,
  test: singleTestFactory,
  team: teamFactory,
};
