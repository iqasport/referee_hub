import React from "react";
import { fireEvent, render, screen } from "../../utils/test-utils";
import RefereeProfile from "./RefereeProfile";

const mockNavigate = jest.fn();
const mockUpdateReferee = jest.fn();
const mockUpdateUserData = jest.fn();
const mockRespondToTeamInvite = jest.fn();

jest.mock("../../utils/navigationUtils", () => ({
  useNavigate: () => mockNavigate,
  useNavigationParams: () => ({ refereeId: "me" }),
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
  useGetRefereeQuery: () => ({ currentData: undefined, error: undefined }),
  useGetCurrentRefereeQuery: () => ({
    currentData: {
      userId: "U_jg7fm2z5ykxevmyrntpdo56g74",
      name: "Jimmy Referee",
      primaryNgb: "USA",
      secondaryNgb: null,
      playingTeam: null,
      coachingTeam: null,
      nationalTeam: null,
      acquiredCertifications: [],
      attributes: {},
    },
    error: undefined,
  }),
  useGetUserDataQuery: () => ({ data: undefined }),
  useGetCurrentUserDataQuery: () => ({ data: {} }),
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