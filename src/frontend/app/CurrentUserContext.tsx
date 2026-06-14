import React, { createContext, useContext } from "react";

import { CurrentUserViewModel, useGetCurrentUserQuery } from "./store/serviceApi";

interface CurrentUserContextValue {
  currentUser?: CurrentUserViewModel;
  error?: unknown;
  isError: boolean;
  isLoading: boolean;
  isAnonymous: boolean;
}

const CurrentUserContext = createContext<CurrentUserContextValue | undefined>(undefined);

const PUBLIC_ROUTE_PATTERNS = [/^\/privacy$/, /^\/tournaments$/, /^\/tournaments\/[^/]+$/];

export const CurrentUserProvider: React.FC<React.PropsWithChildren> = ({ children }) => {
  const isPublicRoute = PUBLIC_ROUTE_PATTERNS.some((pattern) => pattern.test(window.location.pathname));
  const { currentData: currentUser, error, isError, isLoading } = useGetCurrentUserQuery(undefined, {
    skip: isPublicRoute,
  });
  const isAnonymous = isPublicRoute || (!isLoading && (isError || !currentUser));

  return (
    <CurrentUserContext.Provider
      value={{
        currentUser,
        error,
        isError,
        isLoading,
        isAnonymous,
      }}
    >
      {children}
    </CurrentUserContext.Provider>
  );
};

export const useCurrentUser = (): CurrentUserContextValue => {
  const context = useContext(CurrentUserContext);

  if (!context) {
    throw new Error("useCurrentUser must be used within a CurrentUserProvider");
  }

  return context;
};