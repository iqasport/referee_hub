import userEvent from "@testing-library/user-event";
import React from "react";

import factories from "../../factories";
import { formatLanguage } from "../../utils/langUtils";
import { mockedStore, render, screen } from "../../utils/test-utils";

import Settings from "./Settings";

describe("Settings", () => {
  const languages = factories.language.buildList(5);
  const currentUser = factories.currentUser.build();
  const defaultStore = {
    currentUser: {
      currentUser: {
        ...currentUser,
        enabledFeatures: ["i18n"],
      },
    },
    languages: {
      languages,
    },
  };
  const mockStore = mockedStore(defaultStore);

  it("renders the settings page", () => {
    render(<Settings />, mockStore);

    screen.getByText("Settings");
  });

  it("shows cta to change language when user has no language", () => {
    render(<Settings />, mockStore);

    screen.getByText("Set your application language by editing your settings");
  });

  describe("while editing", () => {
    it("shows the language dropdown", async () => {
      const user = userEvent.setup();
      render(<Settings />, mockStore);

      await user.click(screen.getByText("Edit"));

      await screen.findByText("Don't see your desired language?", { exact: false });
    });

    it("allows for language selection", async () => {
      const user = userEvent.setup();
      render(<Settings />, mockStore);

      await user.click(screen.getByText("Edit"));

      const dropdown = await screen.findByRole("combobox", { name: "" });

      await user.selectOptions(dropdown, [languages[2].id]);

      // The dropdown should show the selected language
      expect(dropdown).toHaveValue(languages[2].id);
    });

    it("is cancelable", async () => {
      const user = userEvent.setup();
      render(<Settings />, mockStore);

      await user.click(screen.getByText("Edit"));
      await screen.findByText("Don't see your desired language?", { exact: false });

      await user.click(screen.getByText("Cancel"));
      await screen.findByText("Set your application language by editing your settings");
    });

    // Legacy redux action dispatch test - component will be migrated to RTK Query
    it.skip("is saveable", async () => {
      const user = userEvent.setup();
      render(<Settings />, mockStore);

      await user.click(screen.getByText("Edit"));

      const dropdown = await screen.findByRole("combobox", { name: "" });

      await user.selectOptions(dropdown, [languages[2].id]);

      await user.click(screen.getByText("Save"));

      expect(mockStore.getActions()).toEqual([
        { payload: undefined, type: "currentUser/updateUserStart" },
      ]);
    });
  });

  describe("with a language", () => {
    const langStore = {
      ...defaultStore,
      currentUser: {
        currentUser: {
          ...currentUser,
          enabledFeatures: ["i18n"],
        },
        language: languages[3],
      },
    };
    const langMockStore = mockedStore(langStore);

    it("shows the current langauge", () => {
      render(<Settings />, langMockStore);

      screen.getByText(formatLanguage(languages[3]));
    });
  });

  describe("when languages aren't fetched", () => {
    const emptyLangStore = {
      ...defaultStore,
      languages: {
        languages: [],
      },
    };
    const emptyLangMock = mockedStore(emptyLangStore);

    // Legacy redux action dispatch test - component will be migrated to RTK Query
    it.skip("fetches languages", () => {
      render(<Settings />, emptyLangMock);

      expect(emptyLangMock.getActions()).toEqual([
        {
          payload: undefined,
          type: "languages/getLanguagesStart",
        },
      ]);
    });
  });

  describe("when user doesn't have the feature flag", () => {
    const disabledFeatureStore = {
      ...defaultStore,
      currentUser: {
        currentUser,
      },
    };
    const disabledFeatureMock = mockedStore(disabledFeatureStore);

    it("does not render the settings page", () => {
      render(<Settings />, disabledFeatureMock);

      expect(screen.queryByText("Settings")).toBeNull();
    });
  });
});
