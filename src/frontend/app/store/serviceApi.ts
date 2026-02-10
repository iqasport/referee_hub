import { baseApi as api } from "./baseApi";
export const addTagTypes = [
  "CertificationPayments",
  "Debug",
  "Export",
  "Identity",
  "Languages",
  "Ngb",
  "Referee",
  "User",
  "UserInfo",
  "Team",
  "Tests",
  "Tournament",
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
      resendTestFeedbackEmail: build.mutation<
        ResendTestFeedbackEmailApiResponse,
        ResendTestFeedbackEmailApiArg
      >({
        query: (queryArg) => ({
          url: `/api/debug/attempts/${queryArg.attemptId}/resend`,
          method: "POST",
        }),
        invalidatesTags: ["Debug"],
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
      getLanguages: build.query<GetLanguagesApiResponse, GetLanguagesApiArg>({
        query: () => ({ url: `/api/Languages` }),
        providesTags: ["Languages"],
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
      updateNgb: build.mutation<UpdateNgbApiResponse, UpdateNgbApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}`,
          method: "PUT",
          body: queryArg.ngbUpdateModel,
        }),
        invalidatesTags: ["Ngb"],
      }),
      updateNgbAvatar: build.mutation<UpdateNgbAvatarApiResponse, UpdateNgbAvatarApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/avatar`,
          method: "PUT",
          body: queryArg.body,
        }),
        invalidatesTags: ["Ngb"],
      }),
      addNgbAdmin: build.mutation<AddNgbAdminApiResponse, AddNgbAdminApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/admins`,
          method: "POST",
          body: queryArg.ngbAdminCreationModel,
        }),
        invalidatesTags: ["Ngb"],
      }),
      deleteNgbAdmin: build.mutation<DeleteNgbAdminApiResponse, DeleteNgbAdminApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/admins`,
          method: "DELETE",
          params: { email: queryArg.email },
        }),
        invalidatesTags: ["Ngb"],
      }),
      adminUpdateNgb: build.mutation<AdminUpdateNgbApiResponse, AdminUpdateNgbApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/api/v2/admin/Ngbs/${queryArg.ngb}`,
          method: "PUT",
          body: queryArg.adminNgbUpdateModel,
        }),
        invalidatesTags: ["Ngb"],
      }),
      adminCreateNgb: build.mutation<AdminCreateNgbApiResponse, AdminCreateNgbApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/api/v2/admin/Ngbs/${queryArg.ngb}`,
          method: "POST",
          body: queryArg.adminNgbUpdateModel,
        }),
        invalidatesTags: ["Ngb"],
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
          method: "PUT",
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
      createNgbTeam: build.mutation<CreateNgbTeamApiResponse, CreateNgbTeamApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/teams`,
          method: "POST",
          body: queryArg.ngbTeamViewModel,
        }),
        invalidatesTags: ["Team"],
      }),
      updateNgbTeam: build.mutation<UpdateNgbTeamApiResponse, UpdateNgbTeamApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/teams/${queryArg.teamId}`,
          method: "PUT",
          body: queryArg.ngbTeamViewModel,
        }),
        invalidatesTags: ["Team"],
      }),
      deleteNgbTeam: build.mutation<DeleteNgbTeamApiResponse, DeleteNgbTeamApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/teams/${queryArg.teamId}`,
          method: "DELETE",
        }),
        invalidatesTags: ["Team"],
      }),
      addTeamManager: build.mutation<AddTeamManagerApiResponse, AddTeamManagerApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/teams/${queryArg.teamId}/managers`,
          method: "POST",
          body: queryArg.teamManagerCreationModel,
        }),
        invalidatesTags: ["Team"],
      }),
      deleteTeamManager: build.mutation<DeleteTeamManagerApiResponse, DeleteTeamManagerApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/teams/${queryArg.teamId}/managers`,
          method: "DELETE",
          params: { email: queryArg.email },
        }),
        invalidatesTags: ["Team"],
      }),
      getTeamManagers: build.query<GetTeamManagersApiResponse, GetTeamManagersApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/teams/${queryArg.teamId}/managers`,
        }),
        providesTags: ["Team"],
      }),
      getTeamMembers: build.query<GetTeamMembersApiResponse, GetTeamMembersApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/teams/${queryArg.teamId}/members`,
          params: {
            Filter: queryArg.filter,
            Page: queryArg.page,
            PageSize: queryArg.pageSize,
            SkipPaging: queryArg.skipPaging,
          },
        }),
        providesTags: ["Team"],
      }),
      getTeamTournamentInvites: build.query<
        GetTeamTournamentInvitesApiResponse,
        GetTeamTournamentInvitesApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Ngbs/${queryArg.ngb}/teams/${queryArg.teamId}/tournamentInvites`,
        }),
        providesTags: ["Team"],
      }),
      getTestDetails: build.query<GetTestDetailsApiResponse, GetTestDetailsApiArg>({
        query: (queryArg) => ({ url: `/api/v2/referees/me/tests/${queryArg.testId}/details` }),
        providesTags: ["Tests"],
      }),
      createNewTest: build.mutation<CreateNewTestApiResponse, CreateNewTestApiArg>({
        query: (queryArg) => ({
          url: `/api/admin/Tests/create`,
          method: "POST",
          body: queryArg.testViewModel,
        }),
        invalidatesTags: ["Tests"],
      }),
      editTest: build.mutation<EditTestApiResponse, EditTestApiArg>({
        query: (queryArg) => ({
          url: `/api/admin/Tests/${queryArg.testId}`,
          method: "PATCH",
          body: queryArg.testViewModel,
        }),
        invalidatesTags: ["Tests"],
      }),
      setTestActive: build.mutation<SetTestActiveApiResponse, SetTestActiveApiArg>({
        query: (queryArg) => ({
          url: `/api/admin/Tests/${queryArg.testId}/active`,
          method: "POST",
          body: queryArg.body,
        }),
        invalidatesTags: ["Tests"],
      }),
      getAllTests: build.query<GetAllTestsApiResponse, GetAllTestsApiArg>({
        query: () => ({ url: `/api/admin/Tests` }),
        providesTags: ["Tests"],
      }),
      importTestQuestions: build.mutation<
        ImportTestQuestionsApiResponse,
        ImportTestQuestionsApiArg
      >({
        query: (queryArg) => ({
          url: `/api/admin/Tests/${queryArg.testId}/import`,
          method: "POST",
          body: queryArg.testQuestions,
        }),
        invalidatesTags: ["Tests"],
      }),
      getTestQuestions: build.query<GetTestQuestionsApiResponse, GetTestQuestionsApiArg>({
        query: (queryArg) => ({ url: `/api/admin/Tests/${queryArg.testId}/questions` }),
        providesTags: ["Tests"],
      }),
      getTournaments: build.query<GetTournamentsApiResponse, GetTournamentsApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments`,
          params: {
            Filter: queryArg.filter,
            Page: queryArg.page,
            PageSize: queryArg.pageSize,
            SkipPaging: queryArg.skipPaging,
          },
        }),
        providesTags: ["Tournament"],
      }),
      createTournament: build.mutation<CreateTournamentApiResponse, CreateTournamentApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments`,
          method: "POST",
          body: queryArg.tournamentModel,
        }),
        invalidatesTags: ["Tournament"],
      }),
      getTournament: build.query<GetTournamentApiResponse, GetTournamentApiArg>({
        query: (queryArg) => ({ url: `/api/v2/Tournaments/${queryArg.tournamentId}` }),
        providesTags: ["Tournament"],
      }),
      updateTournament: build.mutation<UpdateTournamentApiResponse, UpdateTournamentApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments/${queryArg.tournamentId}`,
          method: "PUT",
          body: queryArg.tournamentModel,
        }),
        invalidatesTags: ["Tournament"],
      }),
      updateTournamentBanner: build.mutation<
        UpdateTournamentBannerApiResponse,
        UpdateTournamentBannerApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments/${queryArg.tournamentId}/banner`,
          method: "PUT",
          body: queryArg.body,
        }),
        invalidatesTags: ["Tournament"],
      }),
      getTournamentManagers: build.query<
        GetTournamentManagersApiResponse,
        GetTournamentManagersApiArg
      >({
        query: (queryArg) => ({ url: `/api/v2/Tournaments/${queryArg.tournamentId}/managers` }),
        providesTags: ["Tournament"],
      }),
      addTournamentManager: build.mutation<
        AddTournamentManagerApiResponse,
        AddTournamentManagerApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments/${queryArg.tournamentId}/managers`,
          method: "POST",
          body: queryArg.addTournamentManagerModel,
        }),
        invalidatesTags: ["Tournament"],
      }),
      removeTournamentManager: build.mutation<
        RemoveTournamentManagerApiResponse,
        RemoveTournamentManagerApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments/${queryArg.tournamentId}/managers/${queryArg.userId}`,
          method: "DELETE",
        }),
        invalidatesTags: ["Tournament"],
      }),
      contactTournamentOrganizers: build.mutation<
        ContactTournamentOrganizersApiResponse,
        ContactTournamentOrganizersApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments/${queryArg.tournamentId}/contact`,
          method: "POST",
          body: queryArg.contactTournamentRequest,
        }),
        invalidatesTags: ["Tournament"],
      }),
      getTournamentInvites: build.query<
        GetTournamentInvitesApiResponse,
        GetTournamentInvitesApiArg
      >({
        query: (queryArg) => ({ url: `/api/v2/Tournaments/${queryArg.tournamentId}/invites` }),
        providesTags: ["Tournament"],
      }),
      createInvite: build.mutation<CreateInviteApiResponse, CreateInviteApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments/${queryArg.tournamentId}/invites`,
          method: "POST",
          body: queryArg.createInviteModel,
        }),
        invalidatesTags: ["Tournament"],
      }),
      respondToInvite: build.mutation<RespondToInviteApiResponse, RespondToInviteApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments/${queryArg.tournamentId}/invites/${queryArg.participantId}`,
          method: "POST",
          body: queryArg.inviteResponseModel,
        }),
        invalidatesTags: ["Tournament"],
      }),
      getParticipants: build.query<GetParticipantsApiResponse, GetParticipantsApiArg>({
        query: (queryArg) => ({ url: `/api/v2/Tournaments/${queryArg.tournamentId}/participants` }),
        providesTags: ["Tournament"],
      }),
      removeParticipant: build.mutation<RemoveParticipantApiResponse, RemoveParticipantApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments/${queryArg.tournamentId}/participants/${queryArg.teamId}`,
          method: "DELETE",
        }),
        invalidatesTags: ["Tournament"],
      }),
      updateParticipantRoster: build.mutation<
        UpdateParticipantRosterApiResponse,
        UpdateParticipantRosterApiArg
      >({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments/${queryArg.tournamentId}/participants/${queryArg.teamId}/roster`,
          method: "PUT",
          body: queryArg.updateRosterModel,
        }),
        invalidatesTags: ["Tournament"],
      }),
      getTeamRoster: build.query<GetTeamRosterApiResponse, GetTeamRosterApiArg>({
        query: (queryArg) => ({
          url: `/api/v2/Tournaments/${queryArg.tournamentId}/teams/${queryArg.teamId}/roster`,
        }),
        providesTags: ["Tournament"],
      }),
      getCurrentUser: build.query<GetCurrentUserApiResponse, GetCurrentUserApiArg>({
        query: () => ({ url: `/api/v2/Users/me` }),
        providesTags: ["User"],
      }),
      getCurrentUserFeatureGates: build.query<
        GetCurrentUserFeatureGatesApiResponse,
        GetCurrentUserFeatureGatesApiArg
      >({
        query: () => ({ url: `/api/v2/Users/me/featuregates` }),
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
      getMyGender: build.query<GetMyGenderApiResponse, GetMyGenderApiArg>({
        query: () => ({ url: `/api/v2/Users/me/gender` }),
        providesTags: ["User"],
      }),
      deleteMyGender: build.mutation<DeleteMyGenderApiResponse, DeleteMyGenderApiArg>({
        query: () => ({ url: `/api/v2/Users/me/gender`, method: "DELETE" }),
        invalidatesTags: ["User"],
      }),
      getManagedTeams: build.query<GetManagedTeamsApiResponse, GetManagedTeamsApiArg>({
        query: () => ({ url: `/api/v2/Users/me/managedTeams` }),
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
  stripeEvent: object;
};
export type GetDataFromLocalBlobApiResponse = unknown;
export type GetDataFromLocalBlobApiArg = {
  fileKey: string;
};
export type ResendTestFeedbackEmailApiResponse = /** status 200 Success */ string;
export type ResendTestFeedbackEmailApiArg = {
  attemptId: string;
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
export type GetLanguagesApiResponse = /** status 200 Success */ string[];
export type GetLanguagesApiArg = void;
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
export type UpdateNgbApiResponse = unknown;
export type UpdateNgbApiArg = {
  ngb: string;
  ngbUpdateModel: NgbUpdateModel;
};
export type UpdateNgbAvatarApiResponse = /** status 200 Success */ string;
export type UpdateNgbAvatarApiArg = {
  ngb: string;
  body: {
    avatarBlob?: Blob;
  };
};
export type AddNgbAdminApiResponse = /** status 200 Success */ NgbAdminCreationStatus;
export type AddNgbAdminApiArg = {
  ngb: string;
  ngbAdminCreationModel: NgbAdminCreationModel;
};
export type DeleteNgbAdminApiResponse = /** status 200 Success */ any;
export type DeleteNgbAdminApiArg = {
  ngb: string;
  email?: string;
};
export type AdminUpdateNgbApiResponse = unknown;
export type AdminUpdateNgbApiArg = {
  ngb: string;
  adminNgbUpdateModel: AdminNgbUpdateModel;
};
export type AdminCreateNgbApiResponse = unknown;
export type AdminCreateNgbApiArg = {
  ngb: string;
  adminNgbUpdateModel: AdminNgbUpdateModel;
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
export type CreateNgbTeamApiResponse = /** status 200 Success */ NgbTeamViewModel;
export type CreateNgbTeamApiArg = {
  ngb: string;
  ngbTeamViewModel: NgbTeamViewModel;
};
export type UpdateNgbTeamApiResponse = /** status 200 Success */ NgbTeamViewModel;
export type UpdateNgbTeamApiArg = {
  ngb: string;
  teamId: string;
  ngbTeamViewModel: NgbTeamViewModel;
};
export type DeleteNgbTeamApiResponse = unknown;
export type DeleteNgbTeamApiArg = {
  ngb: string;
  teamId: string;
};
export type AddTeamManagerApiResponse = /** status 200 Success */ TeamManagerCreationStatus;
export type AddTeamManagerApiArg = {
  ngb: string;
  teamId: string;
  teamManagerCreationModel: TeamManagerCreationModel;
};
export type DeleteTeamManagerApiResponse = /** status 200 Success */ void;
export type DeleteTeamManagerApiArg = {
  ngb: string;
  teamId: string;
  email?: string;
};
export type GetTeamManagersApiResponse = /** status 200 Success */ TeamManagerViewModel[];
export type GetTeamManagersApiArg = {
  ngb: string;
  teamId: string;
};
export type GetTeamMembersApiResponse = /** status 200 Success */ TeamMemberViewModelFiltered;
export type GetTeamMembersApiArg = {
  ngb: string;
  teamId: string;
  filter?: string;
  page?: number;
  pageSize?: number;
  skipPaging?: boolean;
};
export type GetTeamTournamentInvitesApiResponse =
  /** status 200 Success */ TournamentInviteViewModel[];
export type GetTeamTournamentInvitesApiArg = {
  ngb: string;
  teamId: string;
};
export type GetTestDetailsApiResponse = /** status 200 Success */ RefereeTestDetailsViewModel;
export type GetTestDetailsApiArg = {
  testId: string;
};
export type CreateNewTestApiResponse = /** status 200 Success */ string;
export type CreateNewTestApiArg = {
  testViewModel: TestViewModel;
};
export type EditTestApiResponse = /** status 200 Success */ string;
export type EditTestApiArg = {
  testId: string;
  testViewModel: TestViewModel;
};
export type SetTestActiveApiResponse = unknown;
export type SetTestActiveApiArg = {
  testId: string;
  body: boolean;
};
export type GetAllTestsApiResponse = /** status 200 Success */ TestViewModel[];
export type GetAllTestsApiArg = void;
export type ImportTestQuestionsApiResponse = unknown;
export type ImportTestQuestionsApiArg = {
  testId: string;
  testQuestions: object;
};
export type GetTestQuestionsApiResponse = /** status 200 Success */ TestQuestionRecord[];
export type GetTestQuestionsApiArg = {
  testId: string;
};
export type GetTournamentsApiResponse = /** status 200 Success */ TournamentViewModelFiltered;
export type GetTournamentsApiArg = {
  filter?: string;
  page?: number;
  pageSize?: number;
  skipPaging?: boolean;
};
export type CreateTournamentApiResponse = /** status 200 Success */ TournamentIdResponse;
export type CreateTournamentApiArg = {
  tournamentModel: TournamentModel;
};
export type GetTournamentApiResponse = /** status 200 Success */ TournamentViewModel;
export type GetTournamentApiArg = {
  tournamentId: string;
};
export type UpdateTournamentApiResponse = /** status 200 Success */ TournamentIdResponse;
export type UpdateTournamentApiArg = {
  tournamentId: string;
  tournamentModel: TournamentModel;
};
export type UpdateTournamentBannerApiResponse = /** status 200 Success */ string;
export type UpdateTournamentBannerApiArg = {
  tournamentId: string;
  body: {
    bannerBlob?: Blob;
  };
};
export type GetTournamentManagersApiResponse =
  /** status 200 Success */ TournamentManagerViewModel[];
export type GetTournamentManagersApiArg = {
  tournamentId: string;
};
export type AddTournamentManagerApiResponse = /** status 200 Success */ void;
export type AddTournamentManagerApiArg = {
  tournamentId: string;
  addTournamentManagerModel: AddTournamentManagerModel;
};
export type RemoveTournamentManagerApiResponse = /** status 200 Success */ void;
export type RemoveTournamentManagerApiArg = {
  tournamentId: string;
  userId: string;
};
export type ContactTournamentOrganizersApiResponse = /** status 200 Success */ void;
export type ContactTournamentOrganizersApiArg = {
  tournamentId: string;
  contactTournamentRequest: ContactTournamentRequest;
};
export type GetTournamentInvitesApiResponse = /** status 200 Success */ TournamentInviteViewModel[];
export type GetTournamentInvitesApiArg = {
  tournamentId: string;
};
export type CreateInviteApiResponse = /** status 201 Created */ TournamentInviteViewModel;
export type CreateInviteApiArg = {
  tournamentId: string;
  createInviteModel: CreateInviteModel;
};
export type RespondToInviteApiResponse = /** status 200 Success */ void;
export type RespondToInviteApiArg = {
  tournamentId: string;
  participantId: string;
  inviteResponseModel: InviteResponseModel;
};
export type GetParticipantsApiResponse = /** status 200 Success */ TournamentParticipantViewModel[];
export type GetParticipantsApiArg = {
  tournamentId: string;
};
export type RemoveParticipantApiResponse = /** status 200 Success */ void;
export type RemoveParticipantApiArg = {
  tournamentId: string;
  teamId: string;
};
export type UpdateParticipantRosterApiResponse = /** status 200 Success */ void;
export type UpdateParticipantRosterApiArg = {
  tournamentId: string;
  teamId: string;
  updateRosterModel: UpdateRosterModel;
};
export type GetTeamRosterApiResponse = /** status 200 Success */ RosterEntryViewModel[];
export type GetTeamRosterApiArg = {
  tournamentId: string;
  teamId: string;
};
export type GetCurrentUserApiResponse = /** status 200 Success */ CurrentUserViewModel;
export type GetCurrentUserApiArg = void;
export type GetCurrentUserFeatureGatesApiResponse = /** status 200 Success */ FeatureGates;
export type GetCurrentUserFeatureGatesApiArg = void;
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
export type GetMyGenderApiResponse = /** status 200 Success */ UserGenderViewModel;
export type GetMyGenderApiArg = void;
export type DeleteMyGenderApiResponse = unknown;
export type DeleteMyGenderApiArg = void;
export type GetManagedTeamsApiResponse = /** status 200 Success */ ManagedTeamViewModel[];
export type GetManagedTeamsApiArg = void;
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
export type CertificationVersion = "eighteen" | "twenty" | "twentytwo" | "twentyfour";
export type NgbConstraint = {};
export type NgbConstraintRead = {
  appliesToAny?: boolean;
};
export type ExportResponse = {
  /** Id of the background job scheduled to process this task. */
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
  /** The identifier of the NGB. */
  countryCode?: string | null;
  /** Official name of the NGB. */
  name?: string | null;
  /** Country name where NGB is located. */
  country?: string | null;
  /** 3 letter country acronym. */
  acronym?: string | null;
  region?: NgbRegion;
  membershipStatus?: NgbMembershipStatus;
  /** Website of the NGB. */
  website?: string | null;
  /** Number of players as declared by the NGB. */
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
  /** The identifier of the NGB. */
  countryCode?: string | null;
  /** Official name of the NGB. */
  name?: string | null;
  /** Country name where NGB is located. */
  country?: string | null;
  /** 3 letter country acronym. */
  acronym?: string | null;
  region?: NgbRegion;
  membershipStatus?: NgbMembershipStatus;
  /** Website of the NGB. */
  website?: string | null;
  /** Number of players as declared by the NGB. */
  playerCount?: number;
  currentStats?: INgbStatsContext;
  historicalStats?: INgbStatsContext[] | null;
  socialAccounts?: SocialAccount[] | null;
  avatarUri?: string | null;
  adminEmails?: string[] | null;
};
export type NgbInfoViewModelRead = {
  /** The identifier of the NGB. */
  countryCode?: string | null;
  /** Official name of the NGB. */
  name?: string | null;
  /** Country name where NGB is located. */
  country?: string | null;
  /** 3 letter country acronym. */
  acronym?: string | null;
  region?: NgbRegion;
  membershipStatus?: NgbMembershipStatus;
  /** Website of the NGB. */
  website?: string | null;
  /** Number of players as declared by the NGB. */
  playerCount?: number;
  currentStats?: INgbStatsContextRead;
  historicalStats?: INgbStatsContextRead[] | null;
  socialAccounts?: SocialAccount[] | null;
  avatarUri?: string | null;
  adminEmails?: string[] | null;
};
export type NgbUpdateModel = {
  /** Official name of the NGB. */
  name?: string | null;
  /** Country name where NGB is located. */
  country?: string | null;
  /** 3 letter country acronym. */
  acronym?: string | null;
  /** Website of the NGB. */
  website?: string | null;
  /** Number of players as declared by the NGB. */
  playerCount?: number;
  /** Social account URLs. */
  socialAccounts?: SocialAccount[] | null;
};
export type NgbAdminCreationStatus =
  | "InvalidEmail"
  | "UserDoesNotExist"
  | "AdminRoleAdded"
  | "AdminUserCreated";
export type NgbAdminCreationModel = {
  email?: string | null;
  createAccountIfNotExists?: boolean;
};
export type AdminNgbUpdateModel = {
  /** Official name of the NGB. */
  name?: string | null;
  /** Country name where NGB is located. */
  country?: string | null;
  /** 3 letter country acronym. */
  acronym?: string | null;
  /** Website of the NGB. */
  website?: string | null;
  /** Number of players as declared by the NGB. */
  playerCount?: number;
  /** Social account URLs. */
  socialAccounts?: SocialAccount[] | null;
  membershipStatus?: NgbMembershipStatus;
  region?: NgbRegion;
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
  /** Id of this attempt. */
  attemptId?: string;
  /** Identifier of the attempted test. */
  testId?: string;
  level?: CertificationLevel;
  version?: CertificationVersion;
  /** When the attempt was started. */
  startedAt?: string;
  /** When the attempt was finished (either through submission or timeout). */
  finishedAt?: string | null;
  finishMethod?: TestAttemptFinishMethod;
  /** Score of the finished attempt. */
  score?: number | null;
  /** Minimum score required to pass. */
  passPercentage?: number | null;
  /** Whether the score was enough to pass the test. (saved in case the test was later modified) */
  passed?: boolean | null;
  /** New certifications the referee was awarded with this attempt if passed. */
  awardedCertifications?: Certification[] | null;
};
export type TestAttemptViewModelRead = {
  /** Id of this attempt. */
  attemptId?: string;
  /** Identifier of the attempted test. */
  testId?: string;
  level?: CertificationLevel;
  version?: CertificationVersion;
  /** When the attempt was started. */
  startedAt?: string;
  /** Whether the test attempt is still in progress. */
  isInProgress?: boolean;
  /** When the attempt was finished (either through submission or timeout). */
  finishedAt?: string | null;
  finishMethod?: TestAttemptFinishMethod;
  /** Score of the finished attempt. */
  score?: number | null;
  /** Minimum score required to pass. */
  passPercentage?: number | null;
  /** Whether the score was enough to pass the test. (saved in case the test was later modified) */
  passed?: boolean | null;
  /** New certifications the referee was awarded with this attempt if passed. */
  awardedCertifications?: Certification[] | null;
  /** Duration of the test. */
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
  /** Primary NGB this referee is located in. */
  primaryNgb?: string | null;
  /** Secondary NGB this referee is located in. */
  secondaryNgb?: string | null;
  playingTeam?: RefereeTeamUpdater;
  coachingTeam?: RefereeTeamUpdater;
  nationalTeam?: RefereeTeamUpdater;
};
export type TeamIndicator = {
  id?: string;
  name?: string | null;
};
export type RefereeViewModel = {
  /** User ID of the referee. */
  userId?: string;
  /** Name of the referee (or "Anonymous" if they disallow exporting). */
  name?: string | null;
  /** Primary NGB this referee is located in. */
  primaryNgb?: string | null;
  /** Secondary NGB this referee is located in. */
  secondaryNgb?: string | null;
  playingTeam?: TeamIndicator;
  coachingTeam?: TeamIndicator;
  nationalTeam?: TeamIndicator;
  /** Certifications acquired by this referee. */
  acquiredCertifications?: Certification[] | null;
  /** User attributes of this referee. */
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
export type TeamGroupAffiliation =
  | "university"
  | "community"
  | "youth"
  | "not_applicable"
  | "national";
export type NgbTeamViewModel = {
  /** Team identifier. */
  teamId?: string;
  /** Team name. */
  name?: string | null;
  /** The city the team is based in. */
  city?: string | null;
  /** The state the team is based in. */
  state?: string | null;
  /** The country the team is based in (for multi country Ngbs). */
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
export type TeamManagerCreationStatus =
  | "InvalidEmail"
  | "UserDoesNotExist"
  | "ManagerRoleAdded"
  | "ManagerUserCreated";
export type TeamManagerCreationModel = {
  email?: string | null;
  createAccountIfNotExists?: boolean;
};
export type ProblemDetails = {
  type?: string | null;
  title?: string | null;
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
  [key: string]: any;
};
export type TeamManagerViewModel = {
  id?: string;
  name?: string | null;
  email?: string | null;
};
export type TeamMemberViewModel = {
  userId?: string;
  name?: string | null;
};
export type TeamMemberViewModelFiltered = {
  metadata?: FilteringMetadata;
  items?: TeamMemberViewModel[] | null;
};
export type ParticipantType = "team";
export type InviteStatus = "pending" | "approved" | "rejected";
export type ApprovalStatus = "pending" | "approved" | "rejected";
export type ApprovalStatusViewModel = {
  status?: ApprovalStatus;
  date?: string | null;
};
export type TournamentInviteViewModel = {
  participantType?: ParticipantType;
  participantId?: string | null;
  participantName?: string | null;
  status?: InviteStatus;
  initiatorUserId?: string;
  createdAt?: string;
  tournamentManagerApproval?: ApprovalStatusViewModel;
  participantApproval?: ApprovalStatusViewModel;
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
  questionsCount?: number;
};
export type TestViewModel = {
  /** Title of the test (how it's displayed to users). */
  title?: string | null;
  /** Block of text displayed to the refere before taking the test. */
  description?: string | null;
  /** Language of the test. */
  language?: string;
  awardedCertification?: Certification;
  /** Time limit in minutes. */
  timeLimit?: number;
  /** Pass percentage. */
  passPercentage?: number;
  /** How many questions to given to the referee during the test. */
  questionsCount?: number;
  /** If it's a recertification test for the previous rulebook. */
  recertification?: boolean;
  /** Feedback to be displayed to the referee after the test if the pass. */
  positiveFeedback?: string | null;
  /** Feedback to be displayed to the referee after the test if they fail. */
  negativeFeedback?: string | null;
  /** Whether the test is active for all users. */
  active?: boolean;
  /** Identifier of the test. */
  testId?: string;
};
export type TestQuestionRecord = {
  sequenceNum?: number;
  question?: string | null;
  feedback?: string | null;
  answer1?: string | null;
  answer2?: string | null;
  answer3?: string | null;
  answer4?: string | null;
  correct?: number;
  correctAnswer?: string | null;
};
export type TournamentType = "Club" | "National" | "Youth" | "Fantasy";
export type TournamentViewModel = {
  name?: string | null;
  description?: string | null;
  startDate?: string;
  endDate?: string;
  type?: TournamentType;
  country?: string | null;
  city?: string | null;
  place?: string | null;
  organizer?: string | null;
  isPrivate?: boolean;
  id?: string;
  bannerImageUrl?: string | null;
  isCurrentUserInvolved?: boolean;
};
export type TournamentViewModelFiltered = {
  metadata?: FilteringMetadata;
  items?: TournamentViewModel[] | null;
};
export type TournamentIdResponse = {
  id?: string | null;
};
export type TournamentModel = {
  name?: string | null;
  description?: string | null;
  startDate?: string;
  endDate?: string;
  type?: TournamentType;
  country?: string | null;
  city?: string | null;
  place?: string | null;
  organizer?: string | null;
  isPrivate?: boolean;
};
export type TournamentManagerViewModel = {
  id?: string;
  name?: string | null;
  email?: string | null;
};
export type AddTournamentManagerModel = {
  email?: string | null;
};
export type ContactTournamentRequest = {
  message: string;
};
export type CreateInviteModel = {
  participantType?: ParticipantType;
  participantId?: string | null;
};
export type InviteResponseModel = {
  approved?: boolean;
};
export type PlayerViewModel = {
  userId?: string;
  userName?: string | null;
  number?: string | null;
  gender?: string | null;
};
export type StaffViewModel = {
  userId?: string;
  userName?: string | null;
};
export type TournamentParticipantViewModel = {
  teamId?: string;
  teamName?: string | null;
  players?: PlayerViewModel[] | null;
  coaches?: StaffViewModel[] | null;
  staff?: StaffViewModel[] | null;
};
export type RosterPlayerModel = {
  userId?: string;
  number?: string | null;
  gender?: string | null;
};
export type RosterStaffModel = {
  userId?: string;
};
export type UpdateRosterModel = {
  players?: RosterPlayerModel[] | null;
  coaches?: RosterStaffModel[] | null;
  staff?: RosterStaffModel[] | null;
};
export type RosterEntryViewModel = {
  name?: string | null;
  pronouns?: string | null;
  gender?: string | null;
  jerseyNumber?: string | null;
  role?: string | null;
  maxCertification?: string | null;
  maxCertificationDate?: string | null;
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
export type FeatureGates = {
  isTestFlag?: boolean;
  showTestResultsOnFinish?: boolean;
};
export type TournamentReferenceViewModel = {
  id?: string | null;
  name?: string | null;
  startDate?: string;
  endDate?: string;
};
export type UserGenderViewModel = {
  gender?: string | null;
  referencedInTournaments?: TournamentReferenceViewModel[] | null;
};
export type ManagedTeamViewModel = {
  teamId?: string;
  teamName?: string | null;
  ngb?: string;
  groupAffiliation?: TeamGroupAffiliation;
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
  useResendTestFeedbackEmailMutation,
  useRunStatsJobMutation,
  useScheduleStatsJobMutation,
  useExportRefereesForNgbMutation,
  useExportTeamsForNgbMutation,
  useLoginMutation,
  useGetLanguagesQuery,
  useGetNgbsQuery,
  useGetNgbInfoQuery,
  useUpdateNgbMutation,
  useUpdateNgbAvatarMutation,
  useAddNgbAdminMutation,
  useDeleteNgbAdminMutation,
  useAdminUpdateNgbMutation,
  useAdminCreateNgbMutation,
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
  useCreateNgbTeamMutation,
  useUpdateNgbTeamMutation,
  useDeleteNgbTeamMutation,
  useAddTeamManagerMutation,
  useDeleteTeamManagerMutation,
  useGetTeamManagersQuery,
  useGetTeamMembersQuery,
  useGetTeamTournamentInvitesQuery,
  useGetTestDetailsQuery,
  useCreateNewTestMutation,
  useEditTestMutation,
  useSetTestActiveMutation,
  useGetAllTestsQuery,
  useImportTestQuestionsMutation,
  useGetTestQuestionsQuery,
  useGetTournamentsQuery,
  useCreateTournamentMutation,
  useGetTournamentQuery,
  useUpdateTournamentMutation,
  useUpdateTournamentBannerMutation,
  useGetTournamentManagersQuery,
  useAddTournamentManagerMutation,
  useRemoveTournamentManagerMutation,
  useContactTournamentOrganizersMutation,
  useGetTournamentInvitesQuery,
  useCreateInviteMutation,
  useRespondToInviteMutation,
  useGetParticipantsQuery,
  useRemoveParticipantMutation,
  useUpdateParticipantRosterMutation,
  useGetTeamRosterQuery,
  useGetCurrentUserQuery,
  useGetCurrentUserFeatureGatesQuery,
  usePutRootUserAttributeMutation,
  usePutUserAttributeMutation,
  useGetMyGenderQuery,
  useDeleteMyGenderMutation,
  useGetManagedTeamsQuery,
  useGetCurrentUserAvatarQuery,
  useUpdateCurrentUserAvatarMutation,
  useGetUserAvatarQuery,
  useGetCurrentUserDataQuery,
  useUpdateCurrentUserDataMutation,
  useGetUserDataQuery,
} = injectedRtkApi;
