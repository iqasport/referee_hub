import { baseApi as api } from "./baseApi";
export const addTagTypes = [
  "CertificationPayments",
  "Debug",
  "Export",
  "Identity",
  "Ngb",
  "Referee",
  "User",
  "UserInfo",
  "Team",
  "Tests",
  "UserAvatar",
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
          body: queryArg.stripeEvent,
        }),
        invalidatesTags: ["CertificationPayments"],
      }),
      getDataFromLocalBlob: build.query<
        GetDataFromLocalBlobApiResponse,
        GetDataFromLocalBlobApiArg
      >({
        query: (queryArg) => ({ url: `/api/debug/blob/${queryArg.fileKey}` }),
        providesTags: ["Debug"],
      }),
      runStatsJob: build.mutation<RunStatsJobApiResponse, RunStatsJobApiArg>({
        query: (queryArg) => ({ url: `/api/debug/statsjob/run/${queryArg.ngb}`, method: "POST" }),
        invalidatesTags: ["Debug"],
      }),
      scheduleStatsJob: build.mutation<ScheduleStatsJobApiResponse, ScheduleStatsJobApiArg>({
        query: () => ({ url: `/api/debug/statsjob/schedule`, method: "POST" }),
        invalidatesTags: ["Debug"],
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
      login: build.mutation<LoginApiResponse, LoginApiArg>({
        query: (queryArg) => ({
          url: `/api/auth/login`,
          method: "POST",
          body: queryArg.loginRequest,
        }),
        invalidatesTags: ["Identity"],
      }),
      getNgbs: build.query<GetNgbsApiResponse, GetNgbsApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs`,
          params: {
            Filter: queryArg.filter,
            Page: queryArg.page,
            PageSize: queryArg.pageSize,
            SkipPaging: queryArg.skipPaging,
          },
        }),
        providesTags: ["Ngb"],
      }),
      getNgbInfo: build.query<GetNgbInfoApiResponse, GetNgbInfoApiArg>({
        query: (queryArg) => ({ url: `/api/v2/Ngbs/${queryArg.ngb}` }),
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
        providesTags: ["Referee", "UserInfo"],
      }),
      getReferee: build.query<GetRefereeApiResponse, GetRefereeApiArg>({
        query: (queryArg) => ({ url: `/api/v2/Referees/${queryArg.userId}` }),
        providesTags: ["Referee", "UserInfo"],
      }),
      getReferees: build.query<GetRefereesApiResponse, GetRefereesApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Referees`,
          params: {
            Filter: queryArg.filter,
            Page: queryArg.page,
            PageSize: queryArg.pageSize,
            SkipPaging: queryArg.skipPaging,
          },
        }),
        providesTags: ["Referee"],
      }),
      getNgbReferees: build.query<GetNgbRefereesApiResponse, GetNgbRefereesApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/referees`,
          params: {
            Filter: queryArg.filter,
            Page: queryArg.page,
            PageSize: queryArg.pageSize,
            SkipPaging: queryArg.skipPaging,
          },
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
      getNgbTeams: build.query<GetNgbTeamsApiResponse, GetNgbTeamsApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/teams`,
          params: {
            Filter: queryArg.filter,
            Page: queryArg.page,
            PageSize: queryArg.pageSize,
            SkipPaging: queryArg.skipPaging,
          },
        }),
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
      putRootUserAttribute: build.mutation<
        PutRootUserAttributeApiResponse,
        PutRootUserAttributeApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Users/${queryArg.userId}/attributes/root/${queryArg.key}`,
          method: "PUT",
          body: queryArg.body,
        }),
        invalidatesTags: ["User"],
      }),
      putUserAttribute: build.mutation<PutUserAttributeApiResponse, PutUserAttributeApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Users/${queryArg.userId}/attributes/${queryArg.ngb}/${queryArg.key}`,
          method: "PUT",
          body: queryArg.body,
        }),
        invalidatesTags: ["User"],
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
  stripeEvent: object;
};
export type GetDataFromLocalBlobApiResponse = unknown;
export type GetDataFromLocalBlobApiArg = {
  fileKey: string;
};
export type RunStatsJobApiResponse = unknown;
export type RunStatsJobApiArg = {
  ngb: NgbConstraint;
};
export type ScheduleStatsJobApiResponse = unknown;
export type ScheduleStatsJobApiArg = void;
export type ExportRefereesForNgbApiResponse = /** status 200 Success */ ExportResponse;
export type ExportRefereesForNgbApiArg = {
  ngb: string;
};
export type ExportTeamsForNgbApiResponse = /** status 200 Success */ ExportResponse;
export type ExportTeamsForNgbApiArg = {
  ngb: string;
};
export type LoginApiResponse = /** status 200 Success */ AccessTokenResponseRead;
export type LoginApiArg = {
  loginRequest: LoginRequest;
};
export type GetNgbsApiResponse = /** status 200 Success */ NgbViewModelFiltered;
export type GetNgbsApiArg = {
  filter?: string;
  page?: number;
  pageSize?: number;
  skipPaging?: boolean;
};
export type GetNgbInfoApiResponse = /** status 200 Success */ NgbInfoViewModelRead;
export type GetNgbInfoApiArg = {
  ngb: string;
};
export type GetAvailableTestsApiResponse =
  /** status 200 Success */ RefereeTestAvailableViewModel[];
export type GetAvailableTestsApiArg = void;
export type GetTestAttemptsApiResponse = /** status 200 Success */ TestAttemptViewModelRead[];
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
export type GetRefereesApiResponse = /** status 200 Success */ RefereeViewModelFiltered;
export type GetRefereesApiArg = {
  filter?: string;
  page?: number;
  pageSize?: number;
  skipPaging?: boolean;
};
export type GetNgbRefereesApiResponse = /** status 200 Success */ RefereeViewModelFiltered;
export type GetNgbRefereesApiArg = {
  ngb: string;
  filter?: string;
  page?: number;
  pageSize?: number;
  skipPaging?: boolean;
};
export type GetAvailablePaymentsApiResponse = /** status 200 Success */ CertificationProduct[];
export type GetAvailablePaymentsApiArg = void;
export type GetNgbTeamsApiResponse = /** status 200 Success */ NgbTeamViewModelFiltered;
export type GetNgbTeamsApiArg = {
  ngb: string;
  filter?: string;
  page?: number;
  pageSize?: number;
  skipPaging?: boolean;
};
export type GetTestDetailsApiResponse = /** status 200 Success */ RefereeTestDetailsViewModel;
export type GetTestDetailsApiArg = {
  testId: string;
};
export type GetCurrentUserApiResponse = /** status 200 Success */ CurrentUserViewModel;
export type GetCurrentUserApiArg = void;
export type PutRootUserAttributeApiResponse = unknown;
export type PutRootUserAttributeApiArg = {
  userId: string;
  key: string;
  body: any;
};
export type PutUserAttributeApiResponse = unknown;
export type PutUserAttributeApiArg = {
  userId: string;
  ngb: string;
  key: string;
  body: any;
};
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
  | /** status 204 No Content */ void;
export type GetUserAvatarApiArg = {
  userId: string;
};
export type GetCurrentUserDataApiResponse = /** status 200 Success */ UserDataViewModel;
export type GetCurrentUserDataApiArg = void;
export type UpdateCurrentUserDataApiResponse = unknown;
export type UpdateCurrentUserDataApiArg = {
  /** A partial model of user data. */
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
export type NgbConstraint = {};
export type NgbConstraintRead = {
  appliesToAny?: boolean;
};
export type ExportResponse = {
  jobId?: string | null;
};
export type AccessTokenResponse = {
  accessToken?: string | null;
  expiresIn?: number;
  refreshToken?: string | null;
};
export type AccessTokenResponseRead = {
  tokenType?: string | null;
  accessToken?: string | null;
  expiresIn?: number;
  refreshToken?: string | null;
};
export type LoginRequest = {
  email?: string | null;
  password?: string | null;
  twoFactorCode?: string | null;
  twoFactorRecoveryCode?: string | null;
};
export type FilteringMetadata = {
  totalCount?: number | null;
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
  website?: string | null;
  playerCount?: number;
};
export type NgbViewModelFiltered = {
  metadata?: FilteringMetadata;
  items?: NgbViewModel[] | null;
};
export type INgbStatsContext = {};
export type INgbStatsContextRead = {
  totalRefereesCount?: number;
  headRefereesCount?: number;
  assistantRefereesCount?: number;
  flagRefereesCount?: number;
  scorekeeperRefereesCount?: number;
  uncertifiedRefereesCount?: number;
  competitiveTeamsCount?: number;
  developingTeamsCount?: number;
  inactiveTeamsCount?: number;
  youthTeamsCount?: number;
  universityTeamsCount?: number;
  communityTeamsCount?: number;
  totalTeamsCount?: number;
  collectedAt?: string;
};
export type SocialAccountType = "facebook" | "twitter" | "youtube" | "instagram" | "other";
export type SocialAccount = {
  url?: string | null;
  type?: SocialAccountType;
};
export type NgbInfoViewModel = {
  countryCode?: string | null;
  name?: string | null;
  country?: string | null;
  acronym?: string | null;
  region?: NgbRegion;
  membershipStatus?: NgbMembershipStatus;
  website?: string | null;
  playerCount?: number;
  currentStats?: INgbStatsContext;
  historicalStats?: INgbStatsContext[] | null;
  socialAccounts?: SocialAccount[] | null;
  avatarUri?: string | null;
};
export type NgbInfoViewModelRead = {
  countryCode?: string | null;
  name?: string | null;
  country?: string | null;
  acronym?: string | null;
  region?: NgbRegion;
  membershipStatus?: NgbMembershipStatus;
  website?: string | null;
  playerCount?: number;
  currentStats?: INgbStatsContextRead;
  historicalStats?: INgbStatsContextRead[] | null;
  socialAccounts?: SocialAccount[] | null;
  avatarUri?: string | null;
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
  finishedAt?: string | null;
  finishMethod?: TestAttemptFinishMethod;
  score?: number | null;
  passPercentage?: number | null;
  passed?: boolean | null;
  awardedCertifications?: Certification[] | null;
};
export type TestAttemptViewModelRead = {
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
export type RefereeTeamUpdater = {
  id?: string;
};
export type RefereeUpdateViewModel = {
  primaryNgb?: string | null;
  secondaryNgb?: string | null;
  playingTeam?: RefereeTeamUpdater;
  coachingTeam?: RefereeTeamUpdater;
};
export type TeamIndicator = {
  id?: string;
  name?: string | null;
};
export type RefereeViewModel = {
  userId?: string;
  name?: string | null;
  primaryNgb?: string | null;
  secondaryNgb?: string | null;
  playingTeam?: TeamIndicator;
  coachingTeam?: TeamIndicator;
  acquiredCertifications?: Certification[] | null;
  attributes?: {
    [key: string]: any;
  } | null;
};
export type RefereeViewModelFiltered = {
  metadata?: FilteringMetadata;
  items?: RefereeViewModel[] | null;
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
  teamId?: string;
  name?: string | null;
  city?: string | null;
  state?: string | null;
  country?: string | null;
  status?: TeamStatus;
  groupAffiliation?: TeamGroupAffiliation;
  joinedAt?: string;
  socialAccounts?: SocialAccount[] | null;
};
export type NgbTeamViewModelFiltered = {
  metadata?: FilteringMetadata;
  items?: NgbTeamViewModel[] | null;
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
  attributes?: {
    [key: string]: any;
  } | null;
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
  useGetDataFromLocalBlobQuery,
  useRunStatsJobMutation,
  useScheduleStatsJobMutation,
  useExportRefereesForNgbMutation,
  useExportTeamsForNgbMutation,
  useLoginMutation,
  useGetNgbsQuery,
  useGetNgbInfoQuery,
  useGetAvailableTestsQuery,
  useGetTestAttemptsQuery,
  useStartTestMutation,
  useSubmitTestMutation,
  useUpdateCurrentRefereeMutation,
  useGetCurrentRefereeQuery,
  useGetRefereeQuery,
  useGetRefereesQuery,
  useGetNgbRefereesQuery,
  useGetAvailablePaymentsQuery,
  useGetNgbTeamsQuery,
  useGetTestDetailsQuery,
  useGetCurrentUserQuery,
  usePutRootUserAttributeMutation,
  usePutUserAttributeMutation,
  useGetCurrentUserAvatarQuery,
  useUpdateCurrentUserAvatarMutation,
  useGetUserAvatarQuery,
  useGetCurrentUserDataQuery,
  useUpdateCurrentUserDataMutation,
  useGetUserDataQuery,
} = injectedRtkApi;
