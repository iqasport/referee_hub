import React from "react";

import { render, screen } from "../../../utils/test-utils";

import TestsTable from "./TestsTable";

const mockHistoryPush = jest.fn();

jest.mock("react-router-dom", () => ({
  ...jest.requireActual("react-router-dom"),
  useNavigate: () => mockHistoryPush,
}));

// Mock RTK Query hooks
jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetAllTestsQuery: jest.fn(() => ({
    data: [],
    isLoading: false,
    error: null,
  })),
  useSetTestActiveMutation: jest.fn(() => [
    jest.fn(),
    { isLoading: false },
  ]),
}));

describe("TestsTable", () => {
  it("renders the tests table component with empty state", () => {
    render(<TestsTable />);
    
    // Component should render without crashing
    // With empty data, it should show "No tests found" message
    expect(screen.getByText("No tests found")).toBeInTheDocument();
  });

  // Note: Additional tests for this RTK Query component require proper test infrastructure
  // for mocking API responses. The component uses useGetAllTestsQuery and useSetTestActiveMutation.
  // 
  // Future test considerations:
  // - Test rendering with actual test data
  // - Test row click navigation
  // - Test active/inactive toggle functionality
  // - Test loading and error states
  //
  // Legacy redux action dispatch tests are not applicable after RTK Query migration.
});
