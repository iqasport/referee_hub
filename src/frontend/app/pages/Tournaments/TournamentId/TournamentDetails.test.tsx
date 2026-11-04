import { render, screen } from "@testing-library/react";
import React from "react";

import {
  useGetCurrentUserQuery,
  useGetManagedTeamsQuery,
  useGetTournamentInvitesQuery,
} from "../../../store/serviceApi";
import TournamentDetails from "./TournamentDetails";

jest.mock("../../../store/serviceApi", () => ({
  useGetTournamentQuery: () => ({
    data: {
      id: "TR_1",
      name: "Test Cup",
      startDate: "2099-01-01",
      endDate: "2099-01-02",
      isRegistrationOpen: true,
    },
    isLoading: false,
    isError: false,
  }),
  useGetTournamentManagersQuery: () => ({ data: [{ id: "U_1" }], isError: false }),
  useGetCurrentUserQuery: jest.fn(),
  useGetTournamentInvitesQuery: jest.fn(),
  useGetParticipantsQuery: () => ({ data: [], refetch: jest.fn() }),
  useGetManagedTeamsQuery: jest.fn(),
  useRespondToInviteMutation: () => [jest.fn()],
  useDeleteTournamentMutation: () => [jest.fn()],
}));

jest.mock("../../../utils/navigationUtils", () => ({
  useNavigationParams: () => ({ tournamentId: "TR_1" }),
  useNavigate: () => jest.fn(),
}));

jest.mock("./components", () => ({
  TournamentHeader: () => null,
  TournamentInfoCards: () => null,
  TournamentAboutSection: () => null,
  RosterManager: () => null,
}));

function mockModal() {
  const ReactActual = jest.requireActual("react");
  return { __esModule: true, default: ReactActual.forwardRef(() => null) };
}
jest.mock("./RegisterTournamentModal", () => mockModal());
jest.mock("./ContactOrganizerModal", () => mockModal());
jest.mock("../components/AddTournamentModal", () => mockModal());
jest.mock("./TeamRegistrationsModal", () => mockModal());
jest.mock("./VolunteerRegistrationsModal", () => mockModal());
jest.mock("./VolunteerRegistrationModal", () => mockModal());
jest.mock("./InviteTeamsModal", () => mockModal());
jest.mock("./AddTournamentManagerModal", () => mockModal());

const mockUseGetCurrentUserQuery = useGetCurrentUserQuery as jest.Mock;
const mockUseGetManagedTeamsQuery = useGetManagedTeamsQuery as jest.Mock;
const mockUseGetTournamentInvitesQuery = useGetTournamentInvitesQuery as jest.Mock;

describe("TournamentDetails", () => {
  beforeEach(() => {
    mockUseGetCurrentUserQuery.mockReturnValue({
      data: { userId: "U_1", roles: [{ roleType: "TournamentManager", tournament: "TR_1" }] },
    });
    mockUseGetTournamentInvitesQuery.mockReturnValue({ data: [], refetch: jest.fn() });
  });

  it("lets a tournament manager who also manages teams register a team", () => {
    mockUseGetManagedTeamsQuery.mockReturnValue({ data: [{ teamId: "TM_1", teamName: "Yankees", ngb: "USA" }] });

    render(<TournamentDetails />);

    expect(screen.getByText("Manager Tools")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Register for Tournament" })).toBeInTheDocument();
  });

  it("hides team registration for a tournament manager without managed teams", () => {
    mockUseGetManagedTeamsQuery.mockReturnValue({ data: [] });

    render(<TournamentDetails />);

    expect(screen.getByText("Manager Tools")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Register for Tournament" })).not.toBeInTheDocument();
  });
});
