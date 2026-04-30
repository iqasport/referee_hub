import { FetchArgs, createApi, fetchBaseQuery, retry } from '@reduxjs/toolkit/query/react'

const PUBLIC_TOURNAMENT_ROUTE_PATTERN = /^\/tournaments(?:\/[^/]+)?$/;

function isPublicTournamentRoute(): boolean {
  return PUBLIC_TOURNAMENT_ROUTE_PATTERN.test(window.location.pathname);
}

function isCurrentUserEndpoint(args: string | FetchArgs): boolean {
  if (typeof args === "string") {
    return args.startsWith("/api/v2/Users/me");
  }

  return typeof args.url === "string" && args.url.startsWith("/api/v2/Users/me");
}

/** if the query URL contains impersonate query we forward it with the API calls */
const fetchWithImpersonationQuery = (fetchFn: ReturnType<typeof fetchBaseQuery>) => (args: string | FetchArgs, api, extraOptions) => {
  // Public tournament pages are anonymous-capable and should never hit Users/me.
  if (isPublicTournamentRoute() && isCurrentUserEndpoint(args)) {
    return Promise.resolve({
      error: {
        data: { message: "Users/me skipped on public tournament route" },
        status: 401,
      },
    });
  }

  const impersonateKey = "impersonate";
  const impersonate = new URLSearchParams(location.search).get(impersonateKey);
  if (impersonate) {
    if (typeof args === "string") {
      const startingChar = args.includes('?') ? '&' : '?';
      args = `${args}${startingChar}${impersonateKey}=${impersonate}`;
    } else {
      args.params = args.params || {};
      args.params[impersonateKey] = impersonate;
    }
  }

  return fetchFn(args, api, extraOptions);
}

const fetchWithRetries = (fetchFn: ReturnType<typeof fetchBaseQuery>) => {
  return retry(fetchFn, {
    retryCondition: (error, _, extraArgs) => {
      // if we failed to execute the fetch call (e.g. due to network error) let's retry up to 3 times
      if (error.status === "FETCH_ERROR" && extraArgs.attempt <= 3) {
        return true;
      }
      return false;
    }
  })
}

// initialize an empty api service that we'll inject endpoints into later as needed
export const baseApi = createApi({
  baseQuery: fetchWithRetries(fetchWithImpersonationQuery(fetchBaseQuery({
    baseUrl: '/',
    prepareHeaders: (headers, api) => {
      if (api.endpoint == "setTestActive") {
        // without this, the boolean value is treated as text/plain and our backend complains
        headers.set("Content-Type", "application/json");
      }
    }
  }))),
  endpoints: () => ({}),
})