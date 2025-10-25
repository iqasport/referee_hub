import React, { ReactElement } from "react";
import { render as rtlRender, RenderResult, RenderOptions } from "@testing-library/react";
import { Provider } from "react-redux";
import { MemoryRouter } from "react-router-dom";
import { configureStore, PreloadedState } from "@reduxjs/toolkit";

import rootReducer, { RootState } from "../rootReducer";
import { serviceApi } from "../store/serviceApi";

/**
 * Create a test store with optional preloaded state
 * This supports both old Redux slices and new RTK Query
 */
export const createTestStore = (preloadedState?: PreloadedState<RootState>) => {
  return configureStore({
    reducer: rootReducer,
    preloadedState,
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware({
        // Disable serializability check for tests
        serializableCheck: false,
      }).concat(serviceApi.middleware),
  });
};

type WrapperProps = {
  children?: ReactElement;
  store?: ReturnType<typeof createTestStore>;
};

const Wrapper: React.ComponentType<WrapperProps> = ({ children, store }) => {
  return (
    <Provider store={store}>
      <MemoryRouter>{children}</MemoryRouter>
    </Provider>
  );
};

interface ExtendedRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  preloadedState?: PreloadedState<RootState>;
  store?: ReturnType<typeof createTestStore>;
}

/**
 * Enhanced render function that creates a real Redux store with RTK Query support
 * Use this for components that use RTK Query hooks or need real state management
 */
export const renderWithProviders = (
  ui: ReactElement,
  {
    preloadedState = {},
    store = createTestStore(preloadedState),
    ...renderOptions
  }: ExtendedRenderOptions = {}
): RenderResult & { store: ReturnType<typeof createTestStore> } => {
  const wrappedUI = <Wrapper store={store}>{ui}</Wrapper>;
  return { store, ...rtlRender(wrappedUI, renderOptions) };
};

// Re-export everything from testing library
export * from "@testing-library/react";

// Export the new render as default
export { renderWithProviders as render };
