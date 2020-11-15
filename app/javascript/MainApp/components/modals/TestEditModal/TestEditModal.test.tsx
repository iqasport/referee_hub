import React from 'react'

import factories from '../../../factories'
import {
  fireEvent,
  mockedStore,
  render,
  screen,
} from "../../../utils/test-utils";

import TestEditModal from './TestEditModal'

describe('TestEditModal', () => {
  const languages = factories.language.buildList(5)
  const certifications = factories.certification.buildList(4)
  const mockStore = mockedStore({
    certifications: {
      certifications
    },
    languages: {
      languages
    },
    test: {
      certification: {},
      test: {},
    }
  })
  const defaultProps = {
    open: true,
    showClose: true
  }

  it('renders an empty form', async () => {
    render(<TestEditModal {...defaultProps} />, mockStore)

    screen.getByText('New Test')
  });

  it('renders the languages', () => {
    render(<TestEditModal {...defaultProps} />, mockStore)

    const selectElement = screen.getByPlaceholderText('Select the language')
    fireEvent.click(selectElement)
    languages.forEach((lang) => {
      screen.getAllByText(`${lang.attributes.longName} - ${lang.attributes.shortRegion}`)
    })
  })
})
