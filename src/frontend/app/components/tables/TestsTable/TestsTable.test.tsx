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

  it("renders table with test data in correct structure", () => {
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

    // Verify the table renders both test titles
    expect(screen.getByText("Assistant Test")).toBeInTheDocument();
    expect(screen.getByText("Head Referee Test")).toBeInTheDocument();

    // Verify certification levels are properly capitalized in the table
    // Using getAllByText because levels could appear in multiple contexts
    const assistantElements = screen.getAllByText("Assistant");
    const headElements = screen.getAllByText("Head");
    
    // Should have at least one instance of each level text
    expect(assistantElements.length).toBeGreaterThanOrEqual(1);
    expect(headElements.length).toBeGreaterThanOrEqual(1);
    
    // Verify languages are displayed
    expect(screen.getByText("en-US")).toBeInTheDocument();
    expect(screen.getByText("es-ES")).toBeInTheDocument();
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

  it("displays active status correctly for different tests", () => {
    const tests: TestViewModel[] = [
      createTestData({ testId: "1", title: "Active Test", active: true }),
      createTestData({ testId: "2", title: "Inactive Test", active: false }),
    ];

    mockUseGetAllTestsQuery.mockReturnValue({ data: tests, isLoading: false });

    const { container } = render(<TestsTable />);

    // Count the number of active (green) icons - should be exactly 1
    const greenIcons = container.querySelectorAll('.text-green');
    expect(greenIcons).toHaveLength(1);
    
    // Count gray icons that are specifically for inactive status
    // The component uses both text-gray-500 for all icons, then adds text-green for active
    const allIcons = container.querySelectorAll('.fa-circle');
    expect(allIcons).toHaveLength(2); // One for each test
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

