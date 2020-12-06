import userEvent from '@testing-library/user-event'
import React from 'react'

import factories from 'MainApp/factories'
import { formatLanguage } from 'MainApp/utils/langUtils'
import { mockedStore, render, screen } from 'MainApp/utils/test-utils'

import Settings from './Settings'

describe('Settings', () => {
  const languages = factories.language.buildList(5)
  const currentUser = factories.currentUser.build()
  const defaultStore = {
    currentUser: {
      currentUser: {
        ...currentUser,
        enabledFeatures: ['i18n']
      }
    },
    languages: {
      languages
    },
  }
  const mockStore = mockedStore(defaultStore)

  it('renders the settings page', () => {
    render(<Settings />, mockStore)

    screen.getByText('Settings')
  })

  it('shows cta to change language when user has no language', () => {
    render(<Settings />, mockStore)

    screen.getByText('Set your application language by editing your settings')
  })

  describe('while editing', () => {
    it('shows the language dropdown', () => {
      render(<Settings />, mockStore)

      userEvent.click(screen.getByText('Edit'))

      screen.getByText("Don't see your desired language?", { exact: false })
    })

    it('allows for language selection', () => {
      render(<Settings />, mockStore)

      userEvent.click(screen.getByText('Edit'))

      const dropdown = screen.getByPlaceholderText('Select the language')

      userEvent.selectOptions(dropdown, [languages[2].id])

      screen.getByText(formatLanguage(languages[2]))
    })

    it('is cancelable', () => {
      render(<Settings />, mockStore)

      userEvent.click(screen.getByText('Edit'))
      screen.getByText("Don't see your desired language?", { exact: false })

      userEvent.click(screen.getByText('Cancel'))
      screen.getByText('Set your application language by editing your settings')
    })

    it('is saveable', () => {
      render(<Settings />, mockStore)

      userEvent.click(screen.getByText('Edit'))

      const dropdown = screen.getByPlaceholderText("Select the language")

      userEvent.selectOptions(dropdown, [languages[2].id])

      userEvent.click(screen.getByText('Save'))

      expect(mockStore.getActions()).toEqual([
        { payload: undefined, type: "currentUser/updateUserStart" },
      ]);
    })
  })

  describe('with a language', () => {
    const langStore = {
      ...defaultStore,
      currentUser: {
        currentUser: {
          ...currentUser,
          enabledFeatures: ['i18n']
        },
        language: languages[3]
      }
    }
    const langMockStore = mockedStore(langStore)

    it('shows the current langauge', () => {
      render(<Settings />, langMockStore)

      screen.getByText(formatLanguage(languages[3]))
    })
  })

  describe("when languages aren't fetched", () => {
    const emptyLangStore = {
      ...defaultStore,
      languages: {
        languages: []
      }
    }
    const emptyLangMock = mockedStore(emptyLangStore)

    it('fetches languages', () => {
      render(<Settings />, emptyLangMock)

      expect(emptyLangMock.getActions()).toEqual([{
        "payload": undefined, "type": "languages/getLanguagesStart"
      }])
    })
  })

  describe("when user doesn't have the feature flag", () => {
    const disabledFeatureStore = {
      ...defaultStore,
      currentUser: {
        currentUser
      }
    }
    const disabledFeatureMock = mockedStore(disabledFeatureStore)

    it('does not render the settings page', () => {
      render(<Settings />, disabledFeatureMock)

      expect(screen.queryByText('Settings')).toBeNull()
    })
  })
})
