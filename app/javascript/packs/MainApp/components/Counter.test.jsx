import * as React from 'react'
import { render, screen } from '@testing-library/react'
import Counter from './Counter'

describe('Counter', () => {
  const defaultProps = {
    timeLimit: 10,
    onTimeLimitMet: jest.fn()
  }

  it('renders a counter', () => {
    render(<Counter {...defaultProps} />)

    expect(screen.getByTestId('counter')).toBeDefined()
  })
})
