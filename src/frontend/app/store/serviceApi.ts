import { baseApi as api } from "./baseApi";
export const addTagTypes = [
  "CertificationPayments",
  "RefereeExport",
  "Referees",
  "RefereeTests",
  "Users",
] as const;
const injectedRtkApi = api
  .enhanceEndpoints({
    addTagTypes,
  })
  .injectEndpoints({
    endpoints: (build) => ({
      getAvailablePayments: build.query<
        GetAvailablePaymentsApiResponse,
        GetAvailablePaymentsApiArg
      >({
        query: () => ({ url: `/api/v2/certifications/payments` }),
        providesTags: ["CertificationPayments"],
      }),
      createPaymentSession: build.mutation<
        CreatePaymentSessionApiResponse,
        CreatePaymentSessionApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/certifications/payments/create`,
          method: "POST",
          params: { Level: queryArg.level, Version: queryArg.version },
        }),
        invalidatesTags: ["CertificationPayments"],
      }),
      submitPaymentSession: build.mutation<
        SubmitPaymentSessionApiResponse,
        SubmitPaymentSessionApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/certifications/payments/submit`,
          method: "POST",
          body: queryArg.event,
        }),
        invalidatesTags: ["CertificationPayments"],
      }),
      exportRefereesForNgb: build.mutation<
        ExportRefereesForNgbApiResponse,
        ExportRefereesForNgbApiArg
      >({
        query: (queryArg) => ({ url: `/api/v2/referees/export/${queryArg.ngb}`, method: "POST" }),
        invalidatesTags: ["RefereeExport"],
      }),
      updateCurrentReferee: build.mutation<
        UpdateCurrentRefereeApiResponse,
        UpdateCurrentRefereeApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Referees/me`,
          method: "PATCH",
          body: queryArg.refereeUpdateViewModel,
        }),
        invalidatesTags: ["Referees"],
      }),
      getCurrentReferee: build.query<GetCurrentRefereeApiResponse, GetCurrentRefereeApiArg>({
        query: () => ({ url: `/api/v2/Referees/me` }),
        providesTags: ["Referees"],
      }),
      getReferee: build.query<GetRefereeApiResponse, GetRefereeApiArg>({
        query: (queryArg) => ({ url: `/api/v2/Referees/${queryArg.userId}` }),
        providesTags: ["Referees"],
      }),
      getReferees: build.query<GetRefereesApiResponse, GetRefereesApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Referees`,
          params: { Page: queryArg.page, PageSize: queryArg.pageSize },
        }),
        providesTags: ["Referees"],
      }),
      getAvailableTests: build.query<GetAvailableTestsApiResponse, GetAvailableTestsApiArg>({
        query: () => ({ url: `/api/v2/referees/me/tests/available` }),
        providesTags: ["RefereeTests"],
      }),
      getTestAttempts: build.query<GetTestAttemptsApiResponse, GetTestAttemptsApiArg>({
        query: () => ({ url: `/api/v2/referees/me/tests/attempts` }),
        providesTags: ["RefereeTests"],
      }),
      startTest: build.mutation<StartTestApiResponse, StartTestApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/referees/me/tests/${queryArg.testId}/start`,
          method: "POST",
        }),
        invalidatesTags: ["RefereeTests"],
      }),
      submitTest: build.mutation<SubmitTestApiResponse, SubmitTestApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/referees/me/tests/${queryArg.testId}/submit`,
          method: "POST",
          body: queryArg.refereeTestSubmitModel,
        }),
        invalidatesTags: ["RefereeTests"],
      }),
      getCurrentUser: build.query<GetCurrentUserApiResponse, GetCurrentUserApiArg>({
        query: () => ({ url: `/api/v2/Users/me` }),
        providesTags: ["Users"],
      }),
      getCurrentUserData: build.query<GetCurrentUserDataApiResponse, GetCurrentUserDataApiArg>({
        query: () => ({ url: `/api/v2/Users/me/info` }),
        providesTags: ["Users"],
      }),
      updateCurrentUserData: build.mutation<
        UpdateCurrentUserDataApiResponse,
        UpdateCurrentUserDataApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Users/me/info`,
          method: "PATCH",
          body: queryArg.userDataViewModel,
        }),
        invalidatesTags: ["Users"],
      }),
      getCurrentUserAvatar: build.query<
        GetCurrentUserAvatarApiResponse,
        GetCurrentUserAvatarApiArg
      >({
        query: () => ({ url: `/api/v2/Users/me/avatar` }),
        providesTags: ["Users"],
      }),
      updateCurrentUserAvatar: build.mutation<
        UpdateCurrentUserAvatarApiResponse,
        UpdateCurrentUserAvatarApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Users/me/avatar`,
          method: "PUT",
          body: queryArg.body,
        }),
        invalidatesTags: ["Users"],
      }),
    }),
    overrideExisting: false,
  });
export { injectedRtkApi as serviceApi };
export type GetAvailablePaymentsApiResponse = unknown;
export type GetAvailablePaymentsApiArg = void;
export type CreatePaymentSessionApiResponse = unknown;
export type CreatePaymentSessionApiArg = {
  level?: CertificationLevel;
  version?: CertificationVersion;
};
export type SubmitPaymentSessionApiResponse = unknown;
export type SubmitPaymentSessionApiArg = {
  event: Event;
};
export type ExportRefereesForNgbApiResponse = /** status 200 Success */ RefereeExportResponse;
export type ExportRefereesForNgbApiArg = {
  ngb: string;
};
export type UpdateCurrentRefereeApiResponse = unknown;
export type UpdateCurrentRefereeApiArg = {
  refereeUpdateViewModel: RefereeUpdateViewModel;
};
export type GetCurrentRefereeApiResponse = /** status 200 Success */ RefereeViewModel;
export type GetCurrentRefereeApiArg = void;
export type GetRefereeApiResponse = /** status 200 Success */ RefereeViewModel;
export type GetRefereeApiArg = {
  userId: string;
};
export type GetRefereesApiResponse = /** status 200 Success */ RefereeViewModel[];
export type GetRefereesApiArg = {
  page?: number;
  pageSize?: number;
};
export type GetAvailableTestsApiResponse =
  /** status 200 Success */ RefereeTestAvailableViewModel[];
export type GetAvailableTestsApiArg = void;
export type GetTestAttemptsApiResponse = /** status 200 Success */ TestAttempt[];
export type GetTestAttemptsApiArg = void;
export type StartTestApiResponse = /** status 200 Success */ RefereeTestStartModel;
export type StartTestApiArg = {
  testId: string;
};
export type SubmitTestApiResponse = /** status 200 Success */ RefereeTestSubmitResponse;
export type SubmitTestApiArg = {
  testId: string;
  refereeTestSubmitModel: RefereeTestSubmitModel;
};
export type GetCurrentUserApiResponse = /** status 200 Success */ CurrentUserViewModel;
export type GetCurrentUserApiArg = void;
export type GetCurrentUserDataApiResponse = /** status 200 Success */ UserDataViewModel;
export type GetCurrentUserDataApiArg = void;
export type UpdateCurrentUserDataApiResponse = unknown;
export type UpdateCurrentUserDataApiArg = {
  userDataViewModel: UserDataViewModel;
};
export type GetCurrentUserAvatarApiResponse = unknown;
export type GetCurrentUserAvatarApiArg = void;
export type UpdateCurrentUserAvatarApiResponse = /** status 200 Success */ string;
export type UpdateCurrentUserAvatarApiArg = {
  body: {
    avatarBlob?: Blob;
  };
};
export type CertificationLevel = "snitch" | "assistant" | "head" | "field" | "scorekeeper";
export type CertificationVersion = "eighteen" | "twenty" | "twentytwo";
export type JToken = JToken[];
export type HttpStatusCode =
  | 100
  | 101
  | 102
  | 103
  | 200
  | 201
  | 202
  | 203
  | 204
  | 205
  | 206
  | 207
  | 208
  | 226
  | 300
  | 301
  | 302
  | 303
  | 304
  | 305
  | 306
  | 307
  | 308
  | 400
  | 401
  | 402
  | 403
  | 404
  | 405
  | 406
  | 407
  | 408
  | 409
  | 410
  | 411
  | 412
  | 413
  | 414
  | 415
  | 416
  | 417
  | 421
  | 422
  | 423
  | 424
  | 426
  | 428
  | 429
  | 431
  | 451
  | 500
  | 501
  | 502
  | 503
  | 504
  | 505
  | 506
  | 507
  | 508
  | 510
  | 511;
export type StringStringIEnumerableKeyValuePair = {
  key?: string | null;
  value?: string[] | null;
};
export type StripeResponse = {
  statusCode?: HttpStatusCode;
  headers?: StringStringIEnumerableKeyValuePair[] | null;
  date?: string | null;
  idempotencyKey?: string | null;
  requestId?: string | null;
  content?: string | null;
};
export type IHasObject = {
  object?: string | null;
};
export type EventData = {
  rawJObject?: {
    [key: string]: JToken;
  } | null;
  stripeResponse?: StripeResponse;
  object?: IHasObject;
  previousAttributes?: any | null;
  rawObject?: any | null;
};
export type EventRequest = {
  rawJObject?: {
    [key: string]: JToken;
  } | null;
  stripeResponse?: StripeResponse;
  id?: string | null;
  idempotencyKey?: string | null;
};
export type Event = {
  rawJObject?: {
    [key: string]: JToken;
  } | null;
  stripeResponse?: StripeResponse;
  id?: string | null;
  object?: string | null;
  account?: string | null;
  apiVersion?: string | null;
  created?: string;
  data?: EventData;
  livemode?: boolean;
  pendingWebhooks?: number;
  request?: EventRequest;
  type?: string | null;
};
export type RefereeExportResponse = {
  jobId?: string | null;
};
export type TeamIdentifier = {
  id?: number;
};
export type RefereeUpdateViewModel = {
  primaryNgb?: string | null;
  secondaryNgb?: string | null;
  playingTeam?: TeamIdentifier;
  coachingTeam?: TeamIdentifier;
};
export type Certification = {
  level?: CertificationLevel;
  version?: CertificationVersion;
};
export type RefereeViewModel = {
  userId?: string;
  name?: string | null;
  primaryNgb?: string | null;
  secondaryNgb?: string | null;
  playingTeam?: TeamIdentifier;
  coachingTeam?: TeamIdentifier;
  acquiredCertifications?: Certification[] | null;
};
export type RefereeEligibilityResult =
  | "Unknown"
  | "Eligible"
  | "MissingRequiredCertification"
  | "RecertificationForLowerThanPreviouslyHeld"
  | "RecertificationNotAllowedDueToInitialCertificationStarted"
  | "TestAttemptedMaximumNumberOfTimes"
  | "MissingCertificationPayment"
  | "InCooldownPeriod"
  | "RefereeAlreadyCertified";
export type RefereeTestAvailableViewModel = {
  testId?: string;
  title?: string | null;
  awardedCertifications?: Certification[] | null;
  language?: string;
  isRefereeEligible?: boolean;
  refereeEligibilityResult?: RefereeEligibilityResult;
};
export type TestAttempt = {
  id?: string;
  userId?: string;
  testId?: string;
  level?: CertificationLevel;
  startedAt?: string;
};
export type Answer = {
  answerId?: number;
  htmlText?: string | null;
};
export type Question = {
  questionId?: number;
  htmlText?: string | null;
  answers?: Answer[] | null;
};
export type RefereeTestStartModel = {
  questions?: Question[] | null;
};
export type RefereeTestSubmitResponse = {
  passed?: boolean;
  passPercentage?: number;
  scoredPercentage?: number;
  awardedCertifications?: Certification[] | null;
};
export type SubmittedTestAnswer = {
  questionId?: number;
  answerId?: number;
};
export type RefereeTestSubmitModel = {
  startedAt?: string;
  answers: SubmittedTestAnswer[];
};
export type CurrentUserViewModel = {
  userId?: string;
  firstName?: string | null;
  lastName?: string | null;
  avatarUrl?: string | null;
  languageId?: string | null;
  roles?:
    | {
        roleType?: string;
      }[]
    | null;
};
export type UserDataViewModel = {
  firstName?: string | null;
  lastName?: string | null;
  bio?: string | null;
  pronouns?: string | null;
  showPronouns?: boolean | null;
  exportName?: boolean | null;
  language?: string | null;
};
export const {
  useGetAvailablePaymentsQuery,
  useCreatePaymentSessionMutation,
  useSubmitPaymentSessionMutation,
  useExportRefereesForNgbMutation,
  useUpdateCurrentRefereeMutation,
  useGetCurrentRefereeQuery,
  useGetRefereeQuery,
  useGetRefereesQuery,
  useGetAvailableTestsQuery,
  useGetTestAttemptsQuery,
  useStartTestMutation,
  useSubmitTestMutation,
  useGetCurrentUserQuery,
  useGetCurrentUserDataQuery,
  useUpdateCurrentUserDataMutation,
  useGetCurrentUserAvatarQuery,
  useUpdateCurrentUserAvatarMutation,
} = injectedRtkApi;