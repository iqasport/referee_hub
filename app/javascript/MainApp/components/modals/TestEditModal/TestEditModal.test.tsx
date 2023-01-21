import userEvent from "@testing-library/user-event";
import React from "react";

import factories from "../../../factories";
import { fireEvent, mockedStore, render, screen } from "../../../utils/test-utils";

import TestEditModal from "./TestEditModal";

describe("TestEditModal", () => {
  const languages = factories.language.buildList(5);
  const certifications = factories.certification.buildList(4);
  const defaultStore = {
    certifications: {
      certifications,
    },
    languages: {
      languages,
    },
    test: {
      certification: {},
      test: {},
    },
  };
  const mockStore = mockedStore(defaultStore);
  const defaultProps = {
    onClose: jest.fn(),
    open: true,
    showClose: true,
  };

  it("renders an empty form", async () => {
    render(<TestEditModal {...defaultProps} />, mockStore);

    screen.getByText("New Test");
  });

  it("renders the languages", () => {
    render(<TestEditModal {...defaultProps} />, mockStore);

    const selectElement = screen.getByPlaceholderText("Select the language");
    fireEvent.click(selectElement);
    languages.forEach((lang) => {
      screen.getAllByText(`${lang.attributes.longName} - ${lang.attributes.shortRegion}`);
    });
  });

  describe("with empty store", () => {
    it("dispatches getCertifications call", () => {
      const emptyCerts = { ...defaultStore, certifications: { certifications: [] } };
      const emptyCertStore = mockedStore(emptyCerts);

      render(<TestEditModal {...defaultProps} />, emptyCertStore);

      expect(emptyCertStore.getActions()).toEqual([
        { payload: undefined, type: "certifications/getCertificationsStart" },
      ]);
    });

    it("dispatches getLanguages call", () => {
      const emptyLangs = { ...defaultStore, languages: { languages: [] } };
      const emptyLangStore = mockedStore(emptyLangs);

      render(<TestEditModal {...defaultProps} />, emptyLangStore);

      expect(emptyLangStore.getActions()).toEqual([
        {
          payload: undefined,
          type: "languages/getLanguagesStart",
        },
      ]);
    });
  });

  describe("with a testId", () => {
    const singleTest = factories.test.build();
    const editMockStore = mockedStore({
      ...defaultStore,
      test: {
        certification: certifications[0],
        test: singleTest,
      },
    });
    const editProps = {
      ...defaultProps,
      testId: singleTest.id,
    };

    it("renders the edit form", () => {
      render(<TestEditModal {...editProps} />, editMockStore);

      screen.getByText("Edit Test");
    });

    it("handles input changes", () => {
      render(<TestEditModal {...editProps} />, editMockStore);

      const descInput = screen.getByText(singleTest.attributes.description);
      const newDesc = "A new description";
      userEvent.clear(descInput);
      userEvent.type(descInput, newDesc);

      expect(descInput).toHaveValue(newDesc);
    });

    it("handles dropdown changes", () => {
      render(<TestEditModal {...editProps} />, editMockStore);

      const certElement = screen.getByPlaceholderText("Select the level");
      userEvent.selectOptions(certElement, ["snitch"]);

      expect(certElement).toHaveDisplayValue("Flag");
    });

    it("enables the submit button after a change", () => {
      render(<TestEditModal {...editProps} />, editMockStore);

      const submitButton = screen.getByText("Done");
      expect(submitButton).toBeDisabled();

      const descInput = screen.getByText(singleTest.attributes.description);
      userEvent.type(descInput, "a change");

      expect(submitButton).toBeEnabled();
    });

    it("shows an error when a required field is empty", () => {
      render(<TestEditModal {...editProps} />, editMockStore);

      const descInput = screen.getByText(singleTest.attributes.description);
      userEvent.clear(descInput);

      userEvent.click(screen.getByText("Done"));

      screen.getByText("Description Cannot be blank");
    });

    it("dispatches an update on submit", () => {
      render(<TestEditModal {...editProps} />, editMockStore);

      const levelElement = screen.getByPlaceholderText("Select the level");
      const versionElement = screen.getByPlaceholderText("Select rulebook version");
      userEvent.selectOptions(levelElement, [certifications[0].attributes.level]);
      userEvent.selectOptions(versionElement, [certifications[0].attributes.version]);
      userEvent.click(screen.getByText("Done"));

      expect(editMockStore.getActions()).toEqual([
        {
          payload: undefined,
          type: "test/updateTestStart",
        },
      ]);
    });

    describe("with an empty test", () => {
      const emptyProps = {
        ...defaultProps,
        testId: "1",
      };

      it("dispatches to get the test", () => {
        render(<TestEditModal {...emptyProps} />, mockStore);

        expect(mockStore.getActions()).toEqual([
          {
            payload: undefined,
            type: "test/getTestStart",
          },
        ]);
      });
    });
  });
});
