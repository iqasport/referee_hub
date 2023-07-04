import { baseApi as api } from "./baseApi";
export const addTagTypes = [
  "CertificationPayments",
  "Export",
  "Ngb",
  "Referee",
  "User",
  "Team",
  "Tests",
  "UserAvatar",
  "UserInfo",
] as const;
const injectedRtkApi = api
  .enhanceEndpoints({
    addTagTypes,
  })
  .injectEndpoints({
    endpoints: (build) => ({
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
          body: queryArg.body,
        }),
        invalidatesTags: ["CertificationPayments"],
      }),
      exportRefereesForNgb: build.mutation<
        ExportRefereesForNgbApiResponse,
        ExportRefereesForNgbApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/referees/export`,
          method: "POST",
        }),
        invalidatesTags: ["Export"],
      }),
      exportTeamsForNgb: build.mutation<ExportTeamsForNgbApiResponse, ExportTeamsForNgbApiArg>({
        query: (queryArg) => ({ url: `/api/v2/Ngbs/${queryArg.ngb}/teams/export`, method: "POST" }),
        invalidatesTags: ["Export"],
      }),
      getNgbs: build.query<GetNgbsApiResponse, GetNgbsApiArg>({
        query: () => ({ url: `/api/v2/Ngbs` }),
        providesTags: ["Ngb"],
      }),
      getAvailableTests: build.query<GetAvailableTestsApiResponse, GetAvailableTestsApiArg>({
        query: () => ({ url: `/api/v2/referees/me/tests/available` }),
        providesTags: ["Referee"],
      }),
      getTestAttempts: build.query<GetTestAttemptsApiResponse, GetTestAttemptsApiArg>({
        query: () => ({ url: `/api/v2/referees/me/tests/attempts` }),
        providesTags: ["Referee"],
      }),
      startTest: build.mutation<StartTestApiResponse, StartTestApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/referees/me/tests/${queryArg.testId}/start`,
          method: "POST",
        }),
        invalidatesTags: ["Referee"],
      }),
      submitTest: build.mutation<SubmitTestApiResponse, SubmitTestApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/referees/me/tests/${queryArg.testId}/submit`,
          method: "POST",
          body: queryArg.refereeTestSubmitModel,
        }),
        invalidatesTags: ["Referee"],
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
        invalidatesTags: ["Referee", "User"],
      }),
      getCurrentReferee: build.query<GetCurrentRefereeApiResponse, GetCurrentRefereeApiArg>({
        query: () => ({ url: `/api/v2/Referees/me` }),
        providesTags: ["Referee"],
      }),
      getReferee: build.query<GetRefereeApiResponse, GetRefereeApiArg>({
        query: (queryArg) => ({ url: `/api/v2/Referees/${queryArg.userId}` }),
        providesTags: ["Referee"],
      }),
      getReferees: build.query<GetRefereesApiResponse, GetRefereesApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Referees`,
          params: { Page: queryArg.page, PageSize: queryArg.pageSize },
        }),
        providesTags: ["Referee"],
      }),
      getAvailablePayments: build.query<
        GetAvailablePaymentsApiResponse,
        GetAvailablePaymentsApiArg
      >({
        query: () => ({ url: `/api/v2/certifications/payments` }),
        providesTags: ["Referee"],
      }),
      getNgbTeam: build.query<GetNgbTeamApiResponse, GetNgbTeamApiArg>({
        query: (queryArg) => ({ url: `/api/v2/Ngbs/${queryArg.ngb}/teams` }),
        providesTags: ["Team"],
      }),
      getTestDetails: build.query<GetTestDetailsApiResponse, GetTestDetailsApiArg>({
        query: (queryArg) => ({ url: `/api/v2/referees/me/tests/${queryArg.testId}/details` }),
        providesTags: ["Tests"],
      }),
      getCurrentUser: build.query<GetCurrentUserApiResponse, GetCurrentUserApiArg>({
        query: () => ({ url: `/api/v2/Users/me` }),
        providesTags: ["User"],
      }),
      getCurrentUserAvatar: build.query<
        GetCurrentUserAvatarApiResponse,
        GetCurrentUserAvatarApiArg
      >({
        query: () => ({ url: `/api/v2/Users/me/avatar` }),
        providesTags: ["UserAvatar"],
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
        invalidatesTags: ["UserAvatar"],
      }),
      getUserAvatar: build.query<GetUserAvatarApiResponse, GetUserAvatarApiArg>({
        query: (queryArg) => ({ url: `/api/v2/Users/${queryArg.userId}/avatar` }),
        providesTags: ["UserAvatar"],
      }),
      getCurrentUserData: build.query<GetCurrentUserDataApiResponse, GetCurrentUserDataApiArg>({
        query: () => ({ url: `/api/v2/Users/me/info` }),
        providesTags: ["UserInfo"],
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
        invalidatesTags: ["UserInfo"],
      }),
      getUserData: build.query<GetUserDataApiResponse, GetUserDataApiArg>({
        query: (queryArg) => ({ url: `/api/v2/Users/${queryArg.userId}/info` }),
        providesTags: ["UserInfo"],
      }),
    }),
    overrideExisting: false,
  });
export { injectedRtkApi as serviceApi };
export type CreatePaymentSessionApiResponse = /** status 200 Success */ CheckoutSession;
export type CreatePaymentSessionApiArg = {
  level?: CertificationLevel;
  version?: CertificationVersion;
};
export type SubmitPaymentSessionApiResponse = unknown;
export type SubmitPaymentSessionApiArg = {
  body: object;
};
export type ExportRefereesForNgbApiResponse = /** status 200 Success */ ExportResponse;
export type ExportRefereesForNgbApiArg = {
  ngb: string;
};
export type ExportTeamsForNgbApiResponse = /** status 200 Success */ ExportResponse;
export type ExportTeamsForNgbApiArg = {
  ngb: string;
};
export type GetNgbsApiResponse = /** status 200 Success */ NgbViewModel[];
export type GetNgbsApiArg = void;
export type GetAvailableTestsApiResponse =
  /** status 200 Success */ RefereeTestAvailableViewModel[];
export type GetAvailableTestsApiArg = void;
export type GetTestAttemptsApiResponse = /** status 200 Success */ TestAttemptViewModel[];
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
export type GetAvailablePaymentsApiResponse = /** status 200 Success */ CertificationProduct[];
export type GetAvailablePaymentsApiArg = void;
export type GetNgbTeamApiResponse = /** status 200 Success */ NgbTeamViewModel[];
export type GetNgbTeamApiArg = {
  ngb: string;
};
export type GetTestDetailsApiResponse = /** status 200 Success */ RefereeTestDetailsViewModel;
export type GetTestDetailsApiArg = {
  testId: string;
};
export type GetCurrentUserApiResponse = /** status 200 Success */ CurrentUserViewModel;
export type GetCurrentUserApiArg = void;
export type GetCurrentUserAvatarApiResponse = unknown;
export type GetCurrentUserAvatarApiArg = void;
export type UpdateCurrentUserAvatarApiResponse = /** status 200 Success */ string;
export type UpdateCurrentUserAvatarApiArg = {
  body: {
    avatarBlob?: Blob;
  };
};
export type GetUserAvatarApiResponse = /** status 200 Success */
  | string
  | /** status 204 No Content */ undefined;
export type GetUserAvatarApiArg = {
  userId: string;
};
export type GetCurrentUserDataApiResponse = /** status 200 Success */ UserDataViewModel;
export type GetCurrentUserDataApiArg = void;
export type UpdateCurrentUserDataApiResponse = unknown;
export type UpdateCurrentUserDataApiArg = {
  userDataViewModel: UserDataViewModel;
};
export type GetUserDataApiResponse = /** status 200 Success */ UserDataViewModel;
export type GetUserDataApiArg = {
  userId: string;
};
export type CheckoutSession = {
  sessionId?: string | null;
};
export type CertificationLevel = "snitch" | "assistant" | "head" | "field" | "scorekeeper";
export type CertificationVersion = "eighteen" | "twenty" | "twentytwo";
export type ExportResponse = {
  jobId?: string | null;
};
export type NgbRegion = "north_america" | "south_america" | "europe" | "africa" | "asia";
export type NgbMembershipStatus = "area_of_interest" | "emerging" | "developing" | "full";
export type NgbViewModel = {
  countryCode?: string | null;
  name?: string | null;
  country?: string | null;
  acronym?: string | null;
  region?: NgbRegion;
  membershipStatus?: NgbMembershipStatus;
};
export type Certification = {
  level?: CertificationLevel;
  version?: CertificationVersion;
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
  level?: CertificationLevel;
};
export type TestAttemptFinishMethod = "Timeout" | "Submission";
export type TestAttemptViewModel = {
  attemptId?: string;
  testId?: string;
  level?: CertificationLevel;
  startedAt?: string;
  isInProgress?: boolean;
  finishedAt?: string | null;
  finishMethod?: TestAttemptFinishMethod;
  score?: number | null;
  passPercentage?: number | null;
  passed?: boolean | null;
  awardedCertifications?: Certification[] | null;
  duration?: string | null;
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
export type TeamIdentifier = {
  id?: number;
};
export type RefereeUpdateViewModel = {
  primaryNgb?: string | null;
  secondaryNgb?: string | null;
  playingTeam?: TeamIdentifier;
  coachingTeam?: TeamIdentifier;
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
export type Price = {
  priceId?: string | null;
  unitPrice?: number;
  currency?: string | null;
};
export type ProductStatus = "Available" | "AlreadyPurchased";
export type CertificationProduct = {
  displayName?: string | null;
  description?: string | null;
  item?: Certification;
  price?: Price;
  status?: ProductStatus;
};
export type TeamStatus = "competitive" | "developing" | "inactive" | "other";
export type TeamGroupAffiliation = "university" | "community" | "youth" | "not_applicable";
export type NgbTeamViewModel = {
  teamId?: TeamIdentifier;
  name?: string | null;
  city?: string | null;
  state?: string | null;
  status?: TeamStatus;
  groupAffiliation?: TeamGroupAffiliation;
};
export type RefereeTestDetailsViewModel = {
  testId?: string;
  title?: string | null;
  awardedCertifications?: Certification[] | null;
  language?: string;
  isRefereeEligible?: boolean;
  timeLimit?: string;
  description?: string | null;
  maximumAttempts?: number;
  passPercentage?: number;
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
  createdAt?: string;
};
export const {
  useCreatePaymentSessionMutation,
  useSubmitPaymentSessionMutation,
  useExportRefereesForNgbMutation,
  useExportTeamsForNgbMutation,
  useGetNgbsQuery,
  useGetAvailableTestsQuery,
  useGetTestAttemptsQuery,
  useStartTestMutation,
  useSubmitTestMutation,
  useUpdateCurrentRefereeMutation,
  useGetCurrentRefereeQuery,
  useGetRefereeQuery,
  useGetRefereesQuery,
  useGetAvailablePaymentsQuery,
  useGetNgbTeamQuery,
  useGetTestDetailsQuery,
  useGetCurrentUserQuery,
  useGetCurrentUserAvatarQuery,
  useUpdateCurrentUserAvatarMutation,
  useGetUserAvatarQuery,
  useGetCurrentUserDataQuery,
  useUpdateCurrentUserDataMutation,
  useGetUserDataQuery,
} = injectedRtkApi;
