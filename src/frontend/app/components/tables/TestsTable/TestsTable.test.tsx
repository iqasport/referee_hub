import userEvent from "@testing-library/user-event";
import React from "react";

import { render, screen } from "../../../utils/test-utils";
import { TestViewModel } from "../../../store/serviceApi";

import TestsTable from "./TestsTable";

const mockHistoryPush = jest.fn();
const mockUseGetAllTestsQuery = jest.fn();
const mockUseSetTestActiveMutation = jest.fn();
const mockUpdateTestActive = jest.fn();

jest.mock("react-router-dom", () => ({
  ...jest.requireActual("react-router-dom"),
  useNavigate: () => mockHistoryPush,
}));

// Mock RTK Query hooks
jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetAllTestsQuery: () => mockUseGetAllTestsQuery(),
  useSetTestActiveMutation: () => mockUseSetTestActiveMutation(),
}));

// Helper to create test data
const createTestData = (overrides: Partial<TestViewModel> = {}): TestViewModel => ({
  testId: "test-1",
  title: "Assistant Referee Test",
  description: "Test for assistant referees",
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
  ...overrides,
});

describe("TestsTable", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockHistoryPush.mockClear();
    mockUpdateTestActive.mockClear();
    
    // Default mock implementation
    mockUseSetTestActiveMutation.mockReturnValue([mockUpdateTestActive, { isLoading: false }]);
  });

  it("renders empty state when no tests are available", () => {
    mockUseGetAllTestsQuery.mockReturnValue({
      data: [],
      isLoading: false,
      error: null,
    });

    render(<TestsTable />);
    
    expect(screen.getByText("No tests found")).toBeInTheDocument();
  });

  it("renders table with test data", () => {
    const tests: TestViewModel[] = [
      createTestData({
        testId: "1",
        title: "Assistant Test",
        awardedCertification: { level: "assistant", version: "twentyfour" },
        language: "en-US",
        active: true,
      }),
      createTestData({
        testId: "2",
        title: "Head Referee Test",
        awardedCertification: { level: "head", version: "twentytwo" },
        language: "es-ES",
        active: false,
      }),
    ];

    mockUseGetAllTestsQuery.mockReturnValue({ data: tests, isLoading: false });

    render(<TestsTable />);

    // Check that test titles are rendered
    expect(screen.getByText("Assistant Test")).toBeInTheDocument();
    expect(screen.getByText("Head Referee Test")).toBeInTheDocument();

    // Check that certification levels are displayed
    expect(screen.getByText("Assistant")).toBeInTheDocument();
    expect(screen.getByText("Head")).toBeInTheDocument();
  });

  it("navigates to test detail on row click", async () => {
    const tests: TestViewModel[] = [
      createTestData({ testId: "test-123", title: "Test Item" }),
    ];

    mockUseGetAllTestsQuery.mockReturnValue({ data: tests, isLoading: false });

    const user = userEvent.setup();
    render(<TestsTable />);

    const testTitle = screen.getByText("Test Item");
    await user.click(testTitle);

    expect(mockHistoryPush).toHaveBeenCalledWith("/admin/tests/test-123", expect.anything());
  });

  it("displays active status with green icon for active tests", () => {
    const tests: TestViewModel[] = [
      createTestData({ testId: "1", title: "Active Test", active: true }),
    ];

    mockUseGetAllTestsQuery.mockReturnValue({ data: tests, isLoading: false });

    render(<TestsTable />);

    // Check for green icon indicating active status
    const activeIcon = document.querySelector('.text-green');
    expect(activeIcon).toBeInTheDocument();
  });

  it("renders snitch level as 'Flag'", () => {
    const tests: TestViewModel[] = [
      createTestData({
        testId: "1",
        title: "Snitch Test",
        awardedCertification: { level: "snitch", version: "twentyfour" },
      }),
    ];

    mockUseGetAllTestsQuery.mockReturnValue({ data: tests, isLoading: false });

    render(<TestsTable />);

    // Snitch level should be displayed as "Flag"
    expect(screen.getByText("Flag")).toBeInTheDocument();
  });
});

