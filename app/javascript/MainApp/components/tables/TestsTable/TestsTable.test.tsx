import userEvent from "@testing-library/user-event";
import { capitalize } from "lodash";
import React from "react";

import factories from "../../../factories";
import { mockedStore, render, screen } from "../../../utils/test-utils";

import { toDateTime } from "MainApp/utils/dateUtils";
import { formatLanguage } from "MainApp/utils/langUtils";

import TestsTable from "./TestsTable";

const mockHistoryPush = jest.fn();

jest.mock("react-router-dom", () => ({
  ...jest.requireActual("react-router-dom"),
  useHistory: () => ({
    push: mockHistoryPush,
  }),
}));

describe("TestsTable", () => {
  const tests = factories.test.buildList(5);
  const languages = factories.language.buildList(5);
  const certifications = factories.certification.buildList(3);
  const defaultStore = {
    languages: {
      languages,
    },
    tests: {
      certifications,
      isLoading: false,
      tests,
    },
  };

  const mockStore = mockedStore(defaultStore);

  it("renders all of the tests", () => {
    render(<TestsTable />, mockStore);

    tests.forEach((test) => {
      screen.getAllByText(test.attributes.name);
    });
  });

  it("renders the expected text rows", () => {
    render(<TestsTable />, mockStore);

    screen.getAllByText(tests[0].attributes.name);
    screen.getAllByText(capitalize(tests[0].attributes.level));
    screen.getAllByText("Unknown");
    screen.getAllByText(
      formatLanguage(
        languages.find((lang) => lang.id === tests[0].attributes.newLanguageId.toString())
      )
    );
    screen.getAllByText(toDateTime(tests[0].attributes.updatedAt).toFormat("D"));
  });

  it("goes to the test view on row click", () => {
    render(<TestsTable />, mockStore);

    const firstTestRow = screen.getAllByText(tests[0].attributes.name)[0];

    userEvent.click(firstTestRow);

    expect(mockHistoryPush).toHaveBeenCalledWith(`/admin/tests/${tests[0].id}`);
  });

  it("dispatches getLanguages call", () => {
    const emptyLangs = { ...defaultStore, languages: { languages: [] } };
    const emptyLangStore = mockedStore(emptyLangs);

    render(<TestsTable />, emptyLangStore);

    expect(emptyLangStore.getActions()).toEqual([
      {
        payload: undefined,
        type: "languages/getLanguagesStart",
      },
    ]);
  });

  it("dispatches getTests call", () => {
    const emptyTests = { ...defaultStore, tests: { tests: [] } };
    const emptyTestsStore = mockedStore(emptyTests);

    render(<TestsTable />, emptyTestsStore);

    expect(emptyTestsStore.getActions()).toEqual([
      {
        payload: undefined,
        type: "tests/getTestsStart",
      },
    ]);
  });
});
