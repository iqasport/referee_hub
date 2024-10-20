import React from "react";
import { render, screen } from "../../utils/test-utils";

import languageFactory from "../../factories/language";
import singleTestFactory from "../../factories/singleTest";

import Details, { DetailsProps } from "./Details";

/*describe("Details", () => {
  const test = singleTestFactory.build();
  const languages = languageFactory.buildList(5);
  const defaultProps: DetailsProps = {
    languages,
    test,
  };

  it("renders the details", () => {
    render(<Details {...defaultProps} />);
    screen.getByText(test.attributes.description);
  });

  describe("with an associated language", () => {
    const language = languageFactory.build({ id: "1" });
    const langProps = {
      ...defaultProps,
      languages: [language],
    };

    it("renders the correct language", () => {
      render(<Details {...langProps} />);
      screen.getByText(`${language.attributes.longName} - ${language.attributes.shortRegion}`);
    });
  });
});*/
