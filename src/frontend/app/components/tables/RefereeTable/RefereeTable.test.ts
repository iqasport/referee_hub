import { RefereeViewModel } from "../../../store/serviceApi";
import { findHighestCert } from "./RefereeTable";

describe("findHighestCert", () => {
  test("keeps assistant above flagrunner in same version", () => {
    const referee: RefereeViewModel = {
      acquiredCertifications: [
        { level: "assistant", version: "twentytwo" },
        { level: "flagrunner", version: "twentytwo" },
      ],
    };

    expect(findHighestCert(referee)).toBe("Assistant 2022-2023");
  });

  test("keeps flagrunner above scorekeeper in same version", () => {
    const referee: RefereeViewModel = {
      acquiredCertifications: [
        { level: "scorekeeper", version: "twentytwo" },
        { level: "flagrunner", version: "twentytwo" },
      ],
    };

    expect(findHighestCert(referee)).toBe("Flagrunner 2022-2023");
  });

  test("keeps head above field in same version", () => {
    const referee: RefereeViewModel = {
      acquiredCertifications: [
        { level: "head", version: "twentytwo" },
        { level: "field", version: "twentytwo" },
      ],
    };

    expect(findHighestCert(referee)).toBe("Head 2022-2023");
  });
});
