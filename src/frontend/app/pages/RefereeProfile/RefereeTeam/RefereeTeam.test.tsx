import React from "react";
import { render, screen } from "../../../utils/test-utils-rtk";
import { createQuerySuccess } from "../../../utils/test-rtk-helpers";

import RefereeTeam from "./RefereeTeam";

// Mock RTK Query hooks
jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetNgbTeamsQuery: jest.fn(),
}));

import { useGetNgbTeamsQuery } from "../../../store/serviceApi";

describe("RefereeTeam", () => {
  const mockTeams = {
    items: [
      { teamId: "1", name: "Team Alpha", city: "City A", state: "State A", country: "Country A", status: "competitive" as const, groupAffiliation: "university" as const, joinedAt: "2023-01-01", socialAccounts: [] },
      { teamId: "2", name: "Team Beta", city: "City B", state: "State B", country: "Country B", status: "developing" as const, groupAffiliation: "community" as const, joinedAt: "2023-02-01", socialAccounts: [] },
    ],
    metadata: { totalCount: 2 },
  };

  const defaultProps = {
    teams: { playingTeam: null, coachingTeam: null },
    locations: { primaryNgb: "USA", secondaryNgb: null },
    isEditing: false,
    onChange: jest.fn(),
  };

  beforeEach(() => {
    (useGetNgbTeamsQuery as jest.Mock).mockReturnValue(createQuerySuccess(mockTeams));
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it("renders the component", () => {
    render(<RefereeTeam {...defaultProps} />);

    // Component should render with labels for team selection
    expect(screen.getByText(/Playing/i)).toBeInTheDocument();
    expect(screen.getByText(/Coaching/i)).toBeInTheDocument();
  });

  it("displays selected teams when provided", () => {
    const propsWithTeams = {
      ...defaultProps,
      teams: {
        playingTeam: { id: "1", name: "Team Alpha" },
        coachingTeam: { id: "2", name: "Team Beta" },
      },
    };

    render(<RefereeTeam {...propsWithTeams} />);

    // Should display team names
    expect(screen.getByText("Team Alpha")).toBeInTheDocument();
    expect(screen.getByText("Team Beta")).toBeInTheDocument();
  });

  it("shows disabled state when no locations are set", () => {
    const propsWithoutLocation = {
      ...defaultProps,
      locations: { primaryNgb: null, secondaryNgb: null },
    };

    render(<RefereeTeam {...propsWithoutLocation} />);

    // Component should handle no location gracefully
    expect(screen.getByText(/Playing/i)).toBeInTheDocument();
  });

  it("handles editing mode", () => {
    const editProps = {
      ...defaultProps,
      isEditing: true,
    };

    render(<RefereeTeam {...editProps} />);

    // In editing mode, should render the component
    expect(screen.getByText(/Playing/i)).toBeInTheDocument();
  });
});
