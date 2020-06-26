/* eslint-disable react/prop-types */
import { render } from '@testing-library/react'
import React from 'react'
import { Provider } from 'react-redux'
import store from '../store.ts'

const Wrapped = ({ children }) => (
  <Provider store={store}>
    {children}
  </Provider>
)

const customRender = (ui, options = {}) => render(ui, { wrapper: Wrapped, ...options })

export * from '@testing-library/react'

export { customRender as render }
