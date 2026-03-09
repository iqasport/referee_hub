import React from "react";
import { render, screen } from "../../../utils/test-utils";
import TeamTable from "./TeamTable";

// Mock RTK Query hooks used by TeamTable
const mockUseGetNgbTeamsQuery = jest.fn();
const mockUseDeleteNgbTeamMutation = jest.fn();

jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetNgbTeamsQuery: (params: any) => mockUseGetNgbTeamsQuery(params),
  useDeleteNgbTeamMutation: () => mockUseDeleteNgbTeamMutation(),
}));

const BASE_TEAM = {
  teamId: "TM_1",
  name: "Yankees",
  city: "New York",
  state: "NY",
  country: "USA",
  status: "competitive",
  groupAffiliation: "community",
  joinedAt: "2020-01-01",
  socialAccounts: [],
  description: null,
  contactEmail: null,
};

describe("TeamTable", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockUseDeleteNgbTeamMutation.mockReturnValue([jest.fn(), { error: null }]);
  });

  test("renders team name without a logo when logoUri is not set", () => {
    mockUseGetNgbTeamsQuery.mockReturnValue({
      data: { items: [{ ...BASE_TEAM, logoUri: null }], metadata: { totalCount: 1 } },
      isLoading: false,
    });

    render(<TeamTable ngbId="USA" />);

    expect(screen.getByText("Yankees")).toBeInTheDocument();
    // No logo image should be rendered
    expect(screen.queryByRole("img", { name: /yankees logo/i })).not.toBeInTheDocument();
  });

  test("renders team logo when logoUri is set", () => {
    const logoUri = "/logos/yankees.png";

    mockUseGetNgbTeamsQuery.mockReturnValue({
      data: {
        items: [{ ...BASE_TEAM, logoUri }],
        metadata: { totalCount: 1 },
      },
      isLoading: false,
    });

    render(<TeamTable ngbId="USA" />);

    expect(screen.getByText("Yankees")).toBeInTheDocument();

    // Logo image should be rendered with the correct src
    const logoImg = screen.getByRole("img", { name: /yankees logo/i });
    expect(logoImg).toBeInTheDocument();
    expect(logoImg).toHaveAttribute("src", logoUri);
  });

  test("renders multiple teams, showing logos only for those with logoUri", () => {
    mockUseGetNgbTeamsQuery.mockReturnValue({
      data: {
        items: [
          { ...BASE_TEAM, teamId: "TM_1", name: "Yankees", logoUri: "/yankees.png" },
          { ...BASE_TEAM, teamId: "TM_2", name: "Red Sox", logoUri: null },
        ],
        metadata: { totalCount: 2 },
      },
      isLoading: false,
    });

    render(<TeamTable ngbId="USA" />);

    expect(screen.getByText("Yankees")).toBeInTheDocument();
    expect(screen.getByText("Red Sox")).toBeInTheDocument();

    // Only Yankees should have a logo
    expect(screen.getByRole("img", { name: /yankees logo/i })).toBeInTheDocument();
    expect(screen.queryByRole("img", { name: /red sox logo/i })).not.toBeInTheDocument();
  });
});
