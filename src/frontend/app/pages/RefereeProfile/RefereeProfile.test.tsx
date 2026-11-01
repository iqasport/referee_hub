import React from "react";
import { fireEvent, render, screen } from "../../utils/test-utils";
import RefereeProfile from "./RefereeProfile";

const mockNavigate = jest.fn();
const mockUpdateReferee = jest.fn();
const mockUpdateUserData = jest.fn();
const mockRespondToTeamInvite = jest.fn();
const mockGetRefereeQuery = jest.fn();
const mockGetUserByUsernameQuery = jest.fn();
const mockCheckUsernameAvailabilityQuery = jest.fn();
let mockRefereeId = "me";
const mockCurrentUserData = { username: "jimmy.referee" };
const mockCurrentReferee = {
  userId: "U_jg7fm2z5ykxevmyrntpdo56g74",
  name: "Jimmy Referee",
  primaryNgb: "USA",
  secondaryNgb: null,
  playingTeam: null,
  coachingTeam: null,
  nationalTeam: null,
  acquiredCertifications: [],
  attributes: {},
};

jest.mock("../../utils/navigationUtils", () => ({
  useNavigate: () => mockNavigate,
  useNavigationParams: () => ({ refereeId: mockRefereeId }),
}));

jest.mock("./RefereeHeader", () => {
  const MockComponent = () => <div>Referee Header</div>;
  MockComponent.displayName = "MockRefereeHeader";
  return MockComponent;
});

jest.mock("./RefereeLocation", () => {
  const MockComponent = () => <div>Referee Location</div>;
  MockComponent.displayName = "MockRefereeLocation";
  return MockComponent;
});

jest.mock("./RefereeTeam", () => {
  const MockComponent = (props: { onChange: (value: unknown) => void }) => (
  <button
    type="button"
    onClick={() => props.onChange({
      playingTeam: { id: "TM_2" },
      coachingTeam: null,
      nationalTeam: null,
    })}
  >
    Set LA Bisons
  </button>
);
  MockComponent.displayName = "MockRefereeTeam";
  return MockComponent;
});

jest.mock("../../store/serviceApi", () => ({
  serviceApi: {
    reducerPath: "serviceApi",
    reducer: () => ({}),
    middleware: () => (next: (value: unknown) => unknown) => (value: unknown) => next(value),
  },
  useGetRefereeQuery: (...args: unknown[]) => mockGetRefereeQuery(...args),
  useGetUserByUsernameQuery: (...args: unknown[]) => mockGetUserByUsernameQuery(...args),
  useCheckUsernameAvailabilityQuery: (...args: unknown[]) => mockCheckUsernameAvailabilityQuery(...args),
  useGetCurrentRefereeQuery: () => ({ currentData: mockCurrentReferee, error: undefined }),
  useGetUserDataQuery: () => ({ data: undefined }),
  useGetCurrentUserDataQuery: () => ({ data: mockCurrentUserData }),
  useUpdateCurrentRefereeMutation: () => [mockUpdateReferee, { error: undefined }],
  useUpdateCurrentUserDataMutation: () => [mockUpdateUserData, { error: undefined }],
  useGetMyTeamInvitesQuery: () => ({ data: [], isLoading: false }),
  useRespondToTeamInviteMutation: () => [mockRespondToTeamInvite],
  useCancelMyTeamInviteMutation: () => [jest.fn()],
  useGetMyTeamHistoryQuery: () => ({ data: [], isLoading: false }),
  useGetManagedTeamsQuery: () => ({ data: [], isLoading: false }),
  useGetMyUpcomingTournamentsQuery: () => ({ data: [], isLoading: false }),
  useGetTestAttemptsQuery: () => ({ data: [], isLoading: false }),
}));

describe("RefereeProfile", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockRefereeId = "me";
    mockGetRefereeQuery.mockReturnValue({ currentData: undefined, error: undefined });
    mockGetUserByUsernameQuery.mockReturnValue({ currentData: undefined, error: undefined });
    mockCheckUsernameAvailabilityQuery.mockReturnValue({ currentData: undefined, isFetching: false });
    mockUpdateReferee.mockReturnValue({ unwrap: jest.fn().mockResolvedValue(undefined) });
    mockUpdateUserData.mockReturnValue({ unwrap: jest.fn().mockResolvedValue(undefined) });
    mockRespondToTeamInvite.mockReturnValue({ unwrap: jest.fn().mockResolvedValue(undefined) });
  });

  it("sends the selected playing team in the update payload", async () => {
    render(<RefereeProfile />);

    fireEvent.click(screen.getAllByRole("button", { name: "Edit" })[0]);
    fireEvent.click(screen.getByRole("button", { name: "Set LA Bisons" }));
    fireEvent.click(screen.getByRole("button", { name: "Save" }));

    expect(mockUpdateReferee).toHaveBeenCalledWith({
      refereeUpdateViewModel: expect.objectContaining({
        primaryNgb: "USA",
        secondaryNgb: null,
        playingTeam: { id: "TM_2" },
        coachingTeam: null,
        nationalTeam: null,
      }),
    });
  });

  it("shows the username with a shareable link on own profile", () => {
    render(<RefereeProfile />);

    expect(screen.getByText("@jimmy.referee")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Copy profile link" })).toBeInTheDocument();
  });

  it("disables saving basic details while the username is invalid", () => {
    render(<RefereeProfile />);

    fireEvent.click(screen.getAllByRole("button", { name: "Edit" })[1]);
    fireEvent.change(screen.getByPlaceholderText("username"), { target: { value: "a!" } });

    expect(screen.getByRole("button", { name: "Save" })).toBeDisabled();
    expect(mockCheckUsernameAvailabilityQuery).toHaveBeenLastCalledWith({ username: "a!" }, { skip: true });
  });

  it("resolves a username route parameter to the user id", () => {
    mockRefereeId = "jimmy.referee";
    mockGetUserByUsernameQuery.mockReturnValue({
      currentData: { userId: "U_jg7fm2z5ykxevmyrntpdo56g74", username: "jimmy.referee" },
      error: undefined,
    });

    render(<RefereeProfile />);

    expect(mockGetUserByUsernameQuery).toHaveBeenCalledWith({ username: "jimmy.referee" }, { skip: false });
    expect(mockGetRefereeQuery).toHaveBeenCalledWith(
      { userId: "U_jg7fm2z5ykxevmyrntpdo56g74" },
      { skip: false },
    );
  });

  it("shows an error when the username route parameter does not exist", () => {
    mockRefereeId = "no.such.user";
    mockGetUserByUsernameQuery.mockReturnValue({ currentData: undefined, error: { status: 404 } });

    render(<RefereeProfile />);

    expect(screen.getByText("No referee found with that username.")).toBeInTheDocument();
  });

  it("renders Upcoming Events above Team Transfer History on own profile", () => {
    render(<RefereeProfile />);

    const upcomingEventsHeading = screen.getByRole("heading", { name: "Upcoming Events" });
    const teamTransferHistoryHeading = screen.getByRole("heading", { name: "Team Transfer History" });

    expect(
      upcomingEventsHeading.compareDocumentPosition(teamTransferHistoryHeading)
      & Node.DOCUMENT_POSITION_FOLLOWING,
    ).toBeTruthy();
  });
});