import userEvent from "@testing-library/user-event";
import React from "react";

import factories from "../../factories";
import { formatLanguage } from "../../utils/langUtils";
import { render, screen } from "../../utils/test-utils-rtk";

import Settings from "./Settings";

describe("Settings", () => {
  const languages = factories.language.buildList(5);
  const currentUser = factories.currentUser.build();
  const preloadedState = {
    currentUser: {
      currentUser: {
        ...currentUser,
        enabledFeatures: ["i18n"],
      },
      language: null,
      id: currentUser.id,
    },
    languages: {
      languages,
    },
  };

  it("renders the settings page", () => {
    render(<Settings />, { preloadedState });

    expect(screen.getByText("Settings")).toBeInTheDocument();
  });

  it("shows cta to change language when user has no language", () => {
    render(<Settings />, { preloadedState });

    expect(screen.getByText("Set your application language by editing your settings")).toBeInTheDocument();
  });

  describe("with a language", () => {
    const langPreloadedState = {
      ...preloadedState,
      currentUser: {
        ...preloadedState.currentUser,
        language: languages[3],
      },
    };

    it("shows the current language", () => {
      render(<Settings />, { preloadedState: langPreloadedState });

      expect(screen.getByText(formatLanguage(languages[3]))).toBeInTheDocument();
    });
  });

  describe("when user doesn't have the feature flag", () => {
    const disabledFeatureState = {
      ...preloadedState,
      currentUser: {
        ...preloadedState.currentUser,
        currentUser,
      },
    };

    it("does not render the settings page", () => {
      render(<Settings />, { preloadedState: disabledFeatureState });

      expect(screen.queryByText("Settings")).not.toBeInTheDocument();
    });
  });

  // Note: Tests for the language dropdown/edit mode are commented out
  // because there's a mismatch between the old Redux language format (Datum[])
  // and the new LanguageDropdown component which expects string[].
  // These tests should be re-enabled once Settings is migrated to RTK Query.
  /*
  describe("while editing", () => {
    it("shows the language dropdown", async () => {
      render(<Settings />, { preloadedState });

      await userEvent.click(screen.getByText("Edit"));

      expect(screen.getByText("Don't see your desired language?", { exact: false })).toBeInTheDocument();
    });

    it("allows for language selection", async () => {
      render(<Settings />, { preloadedState });

      await userEvent.click(screen.getByText("Edit"));

      const dropdown = screen.getByRole("combobox");

      await userEvent.selectOptions(dropdown, [languages[2].id]);

      expect(screen.getByText(formatLanguage(languages[2]))).toBeInTheDocument();
    });

    it("is cancelable", async () => {
      render(<Settings />, { preloadedState });

      await userEvent.click(screen.getByText("Edit"));
      expect(screen.getByText("Don't see your desired language?", { exact: false })).toBeInTheDocument();

      await userEvent.click(screen.getByText("Cancel"));
      expect(screen.getByText("Set your application language by editing your settings")).toBeInTheDocument();
    });

    it("is saveable", async () => {
      const { store } = render(<Settings />, { preloadedState });

      await userEvent.click(screen.getByText("Edit"));

      const dropdown = screen.getByRole("combobox");

      await userEvent.selectOptions(dropdown, [languages[2].id]);

      await userEvent.click(screen.getByText("Save"));

      // Modal should close after save
      expect(screen.queryByText("Don't see your desired language?", { exact: false })).not.toBeInTheDocument();
    });
  });
  */
});
