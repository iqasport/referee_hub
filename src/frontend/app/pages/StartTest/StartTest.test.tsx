import React from "react";
import { render, screen } from "../../utils/test-utils";

import StartTest from "./StartTest";

// Mock the RTK Query hooks
const mockUseGetTestDetailsQuery = jest.fn();
const mockUseStartTestMutation = jest.fn();
const mockUseSubmitTestMutation = jest.fn();

jest.mock("../../store/serviceApi", () => ({
  ...jest.requireActual("../../store/serviceApi"),
  useGetTestDetailsQuery: (params: any) => mockUseGetTestDetailsQuery(params),
  useStartTestMutation: () => mockUseStartTestMutation(),
  useSubmitTestMutation: () => mockUseSubmitTestMutation(),
}));

// Mock navigation utilities
const mockNavigate = jest.fn();
jest.mock("../../utils/navigationUtils", () => ({
  useNavigate: () => mockNavigate,
  useNavigationParams: () => ({ refereeId: "ref-123", testId: "test-123" }),
}));

// Mock the test data
const mockTest = {
  testId: "test-123",
  title: "Assistant Referee Test",
  description: "Test your knowledge of assistant referee rules",
  language: "en-US",
  awardedCertification: {
    level: "assistant",
    version: "twentyfour",
  },
  timeLimit: "01:00:00",
  passPercentage: 80,
  questionsCount: 20,
  recertification: false,
  positiveFeedback: "Great job!",
  negativeFeedback: "Keep studying",
  active: true,
};

const mockSubmittedTestPassed = {
  passed: true,
  passPercentage: 80,
  scoredPercentage: 85,
  awardedCertifications: null,
};

const mockSubmittedTestFailed = {
  passed: false,
  passPercentage: 80,
  scoredPercentage: 65,
  awardedCertifications: null,
};

describe("StartTest - Finish Page", () => {
  const mockStartTest = jest.fn();
  const mockSubmitTest = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();

    mockUseGetTestDetailsQuery.mockReturnValue({
      currentData: mockTest,
      isLoading: false,
      error: null,
    });

    mockUseStartTestMutation.mockReturnValue([
      mockStartTest,
      { data: null, error: null },
    ]);
  });

  it("displays passed status when test is passed", () => {
    mockUseSubmitTestMutation.mockReturnValue([
      mockSubmitTest,
      { data: mockSubmittedTestPassed, error: null },
    ]);

    render(<StartTest />);

    expect(screen.getByText("Passed")).toBeInTheDocument();
    expect(screen.getByText("Your Score: 85%")).toBeInTheDocument();
  });

  it("displays failed status when test is failed", () => {
    mockUseSubmitTestMutation.mockReturnValue([
      mockSubmitTest,
      { data: mockSubmittedTestFailed, error: null },
    ]);

    render(<StartTest />);

    expect(screen.getByText("Failed")).toBeInTheDocument();
    expect(screen.getByText("Your Score: 65%")).toBeInTheDocument();
    expect(screen.getByText("Required to Pass: 80%")).toBeInTheDocument();
  });

  it("does not show required percentage when test is passed", () => {
    mockUseSubmitTestMutation.mockReturnValue([
      mockSubmitTest,
      { data: mockSubmittedTestPassed, error: null },
    ]);

    render(<StartTest />);

    expect(screen.queryByText("Required to Pass: 80%")).not.toBeInTheDocument();
  });

  it("displays email notification message on finish", () => {
    mockUseSubmitTestMutation.mockReturnValue([
      mockSubmitTest,
      { data: mockSubmittedTestPassed, error: null },
    ]);

    render(<StartTest />);

    expect(screen.getByText(/Results will be emailed to you/)).toBeInTheDocument();
  });
});
