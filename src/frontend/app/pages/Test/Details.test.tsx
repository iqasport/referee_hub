import React from "react";
import { render, screen } from "../../utils/test-utils";

import Details, { DetailsProps } from "./Details";
import { TestViewModel } from "../../store/serviceApi";

describe("Details", () => {
  const testData: TestViewModel = {
    testId: "test-1",
    title: "Assistant Referee Test",
    description: "This is a test description for assistant referees",
    language: "en-US",
    awardedCertification: {
      level: "assistant",
      version: "twentyfour",
    },
    timeLimit: 18,
    passPercentage: 80,
    questionsCount: 10,
    recertification: false,
    positiveFeedback: "Great job!",
    negativeFeedback: "Keep studying",
    active: true,
  };

  const languages = ["en-US", "es-ES", "fr-FR"];
  
  const defaultProps: DetailsProps = {
    languages,
    test: testData,
  };

  it("renders the test details", () => {
    render(<Details {...defaultProps} />);
    
    // Check that description is rendered
    expect(screen.getByText(testData.description)).toBeInTheDocument();
    
    // Check that other details are rendered
    expect(screen.getByText("title")).toBeInTheDocument();
    expect(screen.getByText(testData.title)).toBeInTheDocument();
  });

  it("renders numeric fields correctly", () => {
    render(<Details {...defaultProps} />);
    
    // Check numeric fields are displayed
    expect(screen.getByText("time limit")).toBeInTheDocument();
    expect(screen.getByText("18")).toBeInTheDocument();
    
    expect(screen.getByText("pass percentage")).toBeInTheDocument();
    expect(screen.getByText("80")).toBeInTheDocument();
    
    expect(screen.getByText("questions count")).toBeInTheDocument();
    expect(screen.getByText("10")).toBeInTheDocument();
  });

  it("renders boolean fields as strings", () => {
    render(<Details {...defaultProps} />);
    
    expect(screen.getByText("recertification")).toBeInTheDocument();
    expect(screen.getByText("false")).toBeInTheDocument();
  });
});
