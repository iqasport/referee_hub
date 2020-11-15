import React from 'react'
import { render, screen } from '../../../utils/test-utils'

import TestEditModal from './TestEditModal'

describe('TestEditModal', () => {
  it('renders', () => {
    render(<TestEditModal open={true} showClose={true} />)
    screen.getByText('New Test')
  })
})
