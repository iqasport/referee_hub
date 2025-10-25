import userEvent from "@testing-library/user-event";
import { capitalize } from "lodash";
import React from "react";

import factories from "../../../factories";
import { render, screen } from "../../../utils/test-utils-rtk";
import { createQuerySuccess, createMutation } from "../../../utils/test-rtk-helpers";

import { toDateTime } from "../../../utils/dateUtils";

import TestsTable from "./TestsTable";

const mockHistoryPush = jest.fn();

jest.mock("react-router-dom", () => ({
  ...jest.requireActual("react-router-dom"),
  useNavigate: () => mockHistoryPush,
}));

// Mock the RTK Query hooks
jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetAllTestsQuery: jest.fn(),
  useGetLanguagesQuery: jest.fn(),
  useSetTestActiveMutation: jest.fn(),
}));

import {
  useGetAllTestsQuery,
  useGetLanguagesQuery,
  useSetTestActiveMutation,
} from "../../../store/serviceApi";

describe("TestsTable", () => {
  const tests = factories.testViewModel.buildList(5);
  const languages = ["en-US", "es-ES", "fr-FR"];

  beforeEach(() => {
    // Setup default mock responses
    (useGetAllTestsQuery as jest.Mock).mockReturnValue(createQuerySuccess(tests));
    (useGetLanguagesQuery as jest.Mock).mockReturnValue(createQuerySuccess(languages));
    (useSetTestActiveMutation as jest.Mock).mockReturnValue(createMutation());
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it("renders all of the tests", () => {
    render(<TestsTable />);

    tests.forEach((test) => {
      expect(screen.getAllByText(test.title).length).toBeGreaterThan(0);
    });
  });

  it("renders the expected text in the first row", () => {
    render(<TestsTable />);

    const firstTest = tests[0];
    expect(screen.getAllByText(firstTest.title).length).toBeGreaterThan(0);
    expect(screen.getAllByText(capitalize(firstTest.awardedCertification.level)).length).toBeGreaterThan(0);
    expect(screen.getAllByText(firstTest.language).length).toBeGreaterThan(0);
  });

  it("goes to the test view on row click", async () => {
    render(<TestsTable />);

    const firstTestRow = screen.getAllByText(tests[0].title)[0];

    await userEvent.click(firstTestRow);

    expect(mockHistoryPush).toHaveBeenCalledWith(`/admin/tests/${tests[0].testId}`, expect.anything());
  });

  it("handles empty tests array", () => {
    (useGetAllTestsQuery as jest.Mock).mockReturnValue(createQuerySuccess([]));

    render(<TestsTable />);

    // Table should render even with no tests
    expect(screen.getByRole("table")).toBeInTheDocument();
  });

  it("handles loading state", () => {
    (useGetAllTestsQuery as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
      isSuccess: false,
    });

    render(<TestsTable />);

    // Component should handle loading gracefully
    expect(screen.getByRole("table")).toBeInTheDocument();
  });
});
