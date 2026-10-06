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

function getErrorStatus(error: unknown): number | undefined {
  if (!error || typeof error !== "object") {
    return undefined;
  }

  const candidate = error as { status?: unknown };
  return typeof candidate.status === "number" ? candidate.status : undefined;
}

export const CurrentUserProvider: React.FC<React.PropsWithChildren> = ({ children }) => {
  const { currentData: currentUser, error, isError, isLoading } = useGetCurrentUserQuery();
  const errorStatus = getErrorStatus(error);
  const isUnauthorized = errorStatus === 401 || errorStatus === 403;
  const isAnonymous = !isLoading && (isUnauthorized || (!isError && !currentUser));

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