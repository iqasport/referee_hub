import React from "react";
import { render, screen } from "../../utils/test-utils-rtk";

import factories from "../../factories";

import Details, { DetailsProps } from "./Details";

describe("Details", () => {
  const test = factories.testViewModel.build({
    title: "Test Title",
    description: "Test description for the test",
    language: "en-US",
  });
  const languages = ["en-US", "es-ES", "fr-FR"];
  const defaultProps: DetailsProps = {
    languages,
    test,
  };

  it("renders the test details", () => {
    render(<Details {...defaultProps} />);
    
    // Should render title
    expect(screen.getByText(test.title)).toBeInTheDocument();
    // Should render description
    expect(screen.getByText(test.description)).toBeInTheDocument();
  });

  it("renders the language", () => {
    render(<Details {...defaultProps} />);
    
    // Language should be displayed
    expect(screen.getByText(test.language)).toBeInTheDocument();
  });

  it("handles test with minimal data", () => {
    const minimalTest = factories.testViewModel.build({
      title: "Minimal Test",
      description: "",
    });
    const minimalProps = {
      ...defaultProps,
      test: minimalTest,
    };

    render(<Details {...minimalProps} />);
    
    expect(screen.getByText("Minimal Test")).toBeInTheDocument();
  });
});
