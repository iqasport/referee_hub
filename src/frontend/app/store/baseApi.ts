import { FetchArgs, createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'

/** if the query URL contains impersonate query we forward it with the API calls */
const fetchWithImpersonationQuery = (fetchFn: ReturnType<typeof fetchBaseQuery>) => (args: string | FetchArgs, api, extraOptions) => {
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

// initialize an empty api service that we'll inject endpoints into later as needed
export const baseApi = createApi({
  baseQuery: fetchWithImpersonationQuery(fetchBaseQuery({
    baseUrl: '/',
    prepareHeaders: (headers, api) => {
      if (api.endpoint == "setTestActive") {
        // without this, the boolean value is treated as text/plain and our backend complains
        headers.set("Content-Type", "application/json");
      }
    }
  })),
  endpoints: () => ({}),
})