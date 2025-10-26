import React from "react";
import { render, screen } from "../../../utils/test-utils";
import RefereeTeam, { RefereeTeamOptions } from "./RefereeTeam";
import { RefereeLocationOptions } from "../RefereeLocation/RefereeLocation";

// Mock the RTK Query hooks
const mockUseGetNgbTeamsQuery = jest.fn();

jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetNgbTeamsQuery: (params: any, options: any) => mockUseGetNgbTeamsQuery(params, options),
}));

describe("RefereeTeam", () => {
  const mockOnChange = jest.fn();

  const defaultTeams: RefereeTeamOptions = {
    playingTeam: null,
    coachingTeam: null,
  };

  const defaultLocations: RefereeLocationOptions = {
    primaryNgb: "USA",
    secondaryNgb: null,
  };

  const mockPrimaryTeams = {
    items: [
      { teamId: "team-1", name: "Boston Forge", state: "MA" },
      { teamId: "team-2", name: "New York Titans", state: "NY" },
    ],
    totalCount: 2,
  };

  const defaultProps = {
    teams: defaultTeams,
    locations: defaultLocations,
    isEditing: false,
    onChange: mockOnChange,
  };

  beforeEach(() => {
    jest.clearAllMocks();
    
    // Default: primary NGB has teams, no secondary NGB
    mockUseGetNgbTeamsQuery.mockImplementation((params: any, options: any) => {
      if (options?.skip) {
        return { data: undefined, isLoading: false, error: null };
      }
      if (params.ngb === "USA") {
        return { data: mockPrimaryTeams, isLoading: false, error: null };
      }
      return { data: undefined, isLoading: false, error: null };
    });
  });

  test("it renders the component with team selects", () => {
    render(<RefereeTeam {...defaultProps} />);

    // Should have dropdowns for playing and coaching teams
    expect(screen.getByText("Playing Team")).toBeInTheDocument();
    expect(screen.getByText("Coaching Team")).toBeInTheDocument();
  });

  test("it queries teams from primary NGB", () => {
    render(<RefereeTeam {...defaultProps} />);

    expect(mockUseGetNgbTeamsQuery).toHaveBeenCalledWith(
      { ngb: "USA", skipPaging: true },
      { skip: false }
    );
  });

  test("it renders with both primary and secondary NGB teams", () => {
    const propsWithSecondary = {
      ...defaultProps,
      locations: {
        primaryNgb: "USA",
        secondaryNgb: "CAN",
      },
    };

    const mockSecondaryTeams = {
      items: [
        { teamId: "team-3", name: "Ottawa Outlaws", state: "ON" },
      ],
      totalCount: 1,
    };

    mockUseGetNgbTeamsQuery.mockImplementation((params: any, options: any) => {
      if (options?.skip) {
        return { data: undefined, isLoading: false, error: null };
      }
      if (params.ngb === "USA") {
        return { data: mockPrimaryTeams, isLoading: false, error: null };
      }
      if (params.ngb === "CAN") {
        return { data: mockSecondaryTeams, isLoading: false, error: null };
      }
      return { data: undefined, isLoading: false, error: null };
    });

    render(<RefereeTeam {...propsWithSecondary} />);

    // Should query both NGBs
    expect(mockUseGetNgbTeamsQuery).toHaveBeenCalledWith(
      { ngb: "USA", skipPaging: true },
      { skip: false }
    );
    expect(mockUseGetNgbTeamsQuery).toHaveBeenCalledWith(
      { ngb: "CAN", skipPaging: true },
      { skip: false }
    );
  });

  test("it renders selected teams when provided", () => {
    const propsWithTeams = {
      ...defaultProps,
      teams: {
        playingTeam: { id: "team-1" },
        coachingTeam: { id: "team-2" },
      },
    };

    render(<RefereeTeam {...propsWithTeams} />);

    // Component should display selected team names
    expect(screen.getByText("Boston Forge")).toBeInTheDocument();
    expect(screen.getByText("New York Titans")).toBeInTheDocument();
  });

  test("it skips query when no NGB locations are set", () => {
    const propsWithoutNgb = {
      ...defaultProps,
      locations: {
        primaryNgb: null,
        secondaryNgb: null,
      },
    };

    render(<RefereeTeam {...propsWithoutNgb} />);

    // Both queries should be skipped
    expect(mockUseGetNgbTeamsQuery).toHaveBeenCalledWith(
      { ngb: null, skipPaging: true },
      { skip: true }
    );
  });
});

