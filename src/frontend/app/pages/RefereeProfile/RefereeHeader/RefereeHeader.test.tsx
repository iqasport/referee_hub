import React from "react";
import { render, screen, fireEvent } from "../../../utils/test-utils";
import RefereeHeader from "./RefereeHeader";
import { Certification } from "../../../store/serviceApi";

// Mock the RTK Query hooks
const mockUseGetUserDataQuery = jest.fn();
const mockUseGetUserAvatarQuery = jest.fn();
const mockUseUpdateCurrentUserDataMutation = jest.fn();

jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetUserDataQuery: (params: any) => mockUseGetUserDataQuery(params),
  useGetUserAvatarQuery: (params: any) => mockUseGetUserAvatarQuery(params),
  useUpdateCurrentUserDataMutation: () => mockUseUpdateCurrentUserDataMutation(),
}));

// Mock navigation params
jest.mock("../../../utils/navigationUtils", () => ({
  useNavigationParams: () => ({ refereeId: "123" }),
}));

describe("RefereeHeader", () => {
  const mockUpdateUser = jest.fn();

  const certifications: Certification[] = [
    {
      level: "snitch",
      version: "eighteen",
    },
    {
      level: "assistant",
      version: "twentyfour",
    },
  ];

  const defaultUserData = {
    firstName: "John",
    lastName: "Doe",
    bio: "Test bio for referee",
    pronouns: "he/him",
    showPronouns: true,
    exportName: false,
  };

  const defaultProps = {
    name: "John Doe",
    certifications,
    isEditable: true,
  };

  beforeEach(() => {
    jest.clearAllMocks();
    
    mockUseGetUserDataQuery.mockReturnValue({
      data: defaultUserData,
      isLoading: false,
      error: null,
    });

    mockUseGetUserAvatarQuery.mockReturnValue({
      data: null,
      isLoading: false,
      error: null,
    });

    mockUseUpdateCurrentUserDataMutation.mockReturnValue([
      mockUpdateUser,
      { error: null },
    ]);
  });

  test("it renders the referee name", () => {
    render(<RefereeHeader {...defaultProps} />);
    
    expect(screen.getByText("John Doe")).toBeInTheDocument();
  });

  test("it renders pronouns when showPronouns is true", () => {
    render(<RefereeHeader {...defaultProps} />);

    expect(screen.getByText("he/him")).toBeInTheDocument();
  });

  test("it doesn't render pronouns when showPronouns is false", () => {
    mockUseGetUserDataQuery.mockReturnValue({
      data: {
        ...defaultUserData,
        showPronouns: false,
      },
      isLoading: false,
      error: null,
    });

    render(<RefereeHeader {...defaultProps} />);

    expect(screen.queryByText("he/him")).not.toBeInTheDocument();
  });

  test("it renders certifications", () => {
    render(<RefereeHeader {...defaultProps} />);

    // Snitch level is displayed as "Flag"
    expect(screen.getByText(/Flag/)).toBeInTheDocument();
    expect(screen.getByText(/Assistant/)).toBeInTheDocument();
  });

  test("it renders the bio", () => {
    render(<RefereeHeader {...defaultProps} />);

    expect(screen.getByText("Test bio for referee")).toBeInTheDocument();
  });

  test("it renders user data from RTK Query", () => {
    render(<RefereeHeader {...defaultProps} />);

    // Verify the hook was called with correct params
    expect(mockUseGetUserDataQuery).toHaveBeenCalledWith({ userId: "123" });
    expect(mockUseGetUserAvatarQuery).toHaveBeenCalledWith({ userId: "123" });
  });

  test("it does not show +N badge when all certs fit in the header", () => {
    // Two certs → both displayed, no overflow
    render(<RefereeHeader {...defaultProps} />);

    expect(screen.queryByText(/^\+\d+$/)).not.toBeInTheDocument();
  });

  test("it shows +N badge when there are more than 2 certifications", () => {
    // head/twentyfour is both max-level AND most-recent-version → deduplicated to 1 explicit badge.
    // hiddenCerts = [snitch/twentytwo, assistant/twentyfour] → +2
    const manyCerts: Certification[] = [
      { level: "head", version: "twentyfour" },
      { level: "snitch", version: "twentytwo" },
      { level: "assistant", version: "twentyfour" },
    ];

    render(<RefereeHeader {...defaultProps} certifications={manyCerts} />);

    expect(screen.getByText("+2")).toBeInTheDocument();
  });

  test("it picks the most recent version when multiple certs share the highest level", () => {
    // Regression: without a version tiebreaker, byLevel could return Head (2020-2021) instead
    // of Head (2024) when the older entry happened to appear first in the array.
    const certs: Certification[] = [
      { level: "head", version: "twenty" },      // older head cert — should NOT be shown
      { level: "head", version: "twentyfour" },  // newer head cert — should be shown
      { level: "assistant", version: "twenty" },
    ];

    render(<RefereeHeader {...defaultProps} certifications={certs} />);

    expect(screen.getByText(/Head \(2024\)/i)).toBeInTheDocument();
    expect(screen.queryByText(/Head \(2020-2021\)/i)).not.toBeInTheDocument();
  });

  test("it reveals hidden certifications on hover of the +N badge", () => {
    const manyCerts: Certification[] = [
      { level: "head", version: "twentyfour" },
      { level: "snitch", version: "twentytwo" },
      { level: "assistant", version: "twentyfour" },
    ];

    render(<RefereeHeader {...defaultProps} certifications={manyCerts} />);

    const overflowBadge = screen.getByText("+2");

    // Tooltip should not be visible before hover
    expect(screen.queryByRole("tooltip")).not.toBeInTheDocument();

    // Hover over the outer wrapper span (parentElement has the onMouseEnter handler)
    fireEvent.mouseEnter(overflowBadge.parentElement!);
    expect(screen.getByRole("tooltip")).toBeInTheDocument();

    // Mouse leave hides the tooltip
    fireEvent.mouseLeave(overflowBadge.parentElement!);
    expect(screen.queryByRole("tooltip")).not.toBeInTheDocument();
  });
});
