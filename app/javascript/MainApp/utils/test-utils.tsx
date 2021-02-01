import React, { ReactElement } from "react";
// tslint:disable-next-line: ordered-imports
import { render as rtlRender, RenderResult } from "@testing-library/react";
import { Provider } from "react-redux";
import { MemoryRouter } from "react-router-dom";
import configureMockStore from "redux-mock-store";
import thunk from "redux-thunk";

import store from "../store";

export const mockedStore = configureMockStore([thunk]);

type WrappedProps = {
  children?: ReactElement;
  // tslint:disable-next-line: no-any
  mockStore?: any;
};

const Wrapped: React.ComponentType<WrappedProps> = ({ children, mockStore = store }) => {
  return (
    <Provider store={mockStore}>
      <MemoryRouter>{children}</MemoryRouter>
    </Provider>
  );
};

const customRender = (
  ui: ReactElement,
  // tslint:disable-next-line: no-any
  mockStore?: any
): RenderResult => {
  const wrappedUI = <Wrapped mockStore={mockStore}>{ui}</Wrapped>;
  return rtlRender(wrappedUI);
};

// re-export everything
export * from "@testing-library/react";

// override render method
export { customRender as render };
