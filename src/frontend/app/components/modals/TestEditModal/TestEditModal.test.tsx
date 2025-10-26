import React from "react";

import { render, screen } from "../../../utils/test-utils";

import TestEditModal from "./TestEditModal";

// Mock RTK Query hooks
jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetAllTestsQuery: jest.fn(() => ({
    data: [],
    isLoading: false,
  })),
  useGetLanguagesQuery: jest.fn(() => ({
    data: [],
    isLoading: false,
  })),
  useCreateNewTestMutation: jest.fn(() => [
    jest.fn(),
    { isLoading: false },
  ]),
  useEditTestMutation: jest.fn(() => [
    jest.fn(),
    { isLoading: false },
  ]),
}));

describe("TestEditModal", () => {
  const defaultProps = {
    onClose: jest.fn(),
    open: true,
    showClose: true,
  };

  it("renders an empty form for new test", () => {
    render(<TestEditModal {...defaultProps} />);

    expect(screen.getByText("New Test")).toBeInTheDocument();
  });

  // Note: Additional tests for this RTK Query component require proper test infrastructure
  // for mocking API responses. The component uses useGetAllTestsQuery, useGetLanguagesQuery,
  // useCreateNewTestMutation, and useEditTestMutation.
  //
  // Future test considerations:
  // - Test rendering with language data from useGetLanguagesQuery
  // - Test edit mode with existing test data
  // - Test form input changes and validation
  // - Test form submission with mutations
  // - Test loading and error states
  //
  // Legacy redux action dispatch tests are not applicable after RTK Query migration.
});
