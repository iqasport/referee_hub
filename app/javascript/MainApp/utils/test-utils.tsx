import React, { ReactElement } from "react";
// tslint:disable-next-line: ordered-imports
import { render as rtlRender, RenderResult } from "@testing-library/react";
import { Provider } from 'react-redux'
import { MemoryRouter } from "react-router-dom";
import store from '../store'

type WrappedProps = {
  children?: ReactElement;
};

const Wrapped: React.ComponentType<WrappedProps> = ({
  children,
}) => {
  return (
    <Provider store={store}>
      <MemoryRouter>
        {children}
      </MemoryRouter>
    </Provider>
  );
};

const customRender = (
  ui: ReactElement
): RenderResult => {
  const wrappedUI = (
    <Wrapped>
      {ui}
    </Wrapped>
  )
  return rtlRender(wrappedUI)
}

// re-export everything
export * from "@testing-library/react";

// override render method
export { customRender as render };
