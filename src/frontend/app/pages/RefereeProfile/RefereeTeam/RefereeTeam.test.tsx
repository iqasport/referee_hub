import React from "react";
import { render, screen } from "../../../utils/test-utils";
import RefereeTeam, { RefereeTeamOptions } from "./RefereeTeam";
import { RefereeLocationOptions } from "../RefereeLocation/RefereeLocation";

// Mock the RTK Query hooks
const mockUseGetNgbTeamsQuery = jest.fn();
const mockUseGetNationalTeamsQuery = jest.fn();

jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetNgbTeamsQuery: (params: any, options: any) => mockUseGetNgbTeamsQuery(params, options),
  useGetNationalTeamsQuery: (params: any) => mockUseGetNationalTeamsQuery(params),
}));

describe("RefereeTeam", () => {
  const mockOnChange = jest.fn();

  const defaultTeams: RefereeTeamOptions = {
    playingTeam: null,
    coachingTeam: null,
    nationalTeam: null,
  };

  const defaultLocations: RefereeLocationOptions = {
    primaryNgb: "USA",
    secondaryNgb: null,
  };

  const mockPrimaryTeams = {
    items: [
      { teamId: "team-1", name: "Boston Forge", state: "MA", groupAffiliation: "community" },
      { teamId: "team-2", name: "New York Titans", state: "NY", groupAffiliation: "community" },
    ],
    totalCount: 2,
  };

  const mockNationalTeams = {
    items: [
      { teamId: "national-1", name: "USA National Team", groupAffiliation: "national" },
      { teamId: "national-2", name: "Canada National Team", groupAffiliation: "national" },
      { teamId: "national-3", name: "Australia National Team", groupAffiliation: "national" },
    ],
    totalCount: 3,
  };

  const defaultProps = {
    teams: defaultTeams,
    locations: defaultLocations,
    isEditing: false,
    onChange: mockOnChange,
    isOwnProfile: false,
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
    
    // Default: return all national teams
    mockUseGetNationalTeamsQuery.mockReturnValue({
      data: mockNationalTeams,
      isLoading: false,
      error: null,
    });
  });

  test("it renders the component with team selects", () => {
    render(<RefereeTeam {...defaultProps} />);

    // Should have dropdowns for playing, coaching, and national teams
    expect(screen.getByText("Playing Team")).toBeInTheDocument();
    expect(screen.getByText("Coaching Team")).toBeInTheDocument();
    expect(screen.getByText("National Team")).toBeInTheDocument();
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
        { teamId: "team-3", name: "Ottawa Outlaws", state: "ON", groupAffiliation: "community" },
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

  test("it queries national teams from the global endpoint", () => {
    render(<RefereeTeam {...defaultProps} />);

    // Should query the global national teams endpoint
    expect(mockUseGetNationalTeamsQuery).toHaveBeenCalledWith({ skipPaging: true });
  });

  test("it renders selected national team when provided", () => {
    const propsWithNationalTeam = {
      ...defaultProps,
      teams: {
        playingTeam: null,
        coachingTeam: null,
        nationalTeam: { id: "national-2" },
      },
    };

    render(<RefereeTeam {...propsWithNationalTeam} />);

    // Component should display selected national team name
    expect(screen.getByText("Canada National Team")).toBeInTheDocument();
  });

  test("it loads national teams independently of user NGBs", () => {
    const propsWithoutNgb = {
      ...defaultProps,
      locations: {
        primaryNgb: null,
        secondaryNgb: null,
      },
    };

    render(<RefereeTeam {...propsWithoutNgb} />);

    // National teams should still be queried even if no NGBs are selected
    expect(mockUseGetNationalTeamsQuery).toHaveBeenCalledWith({ skipPaging: true });
  });
});

