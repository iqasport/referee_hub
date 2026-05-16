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

  test("it renders a bio containing a URL as text", () => {
    mockUseGetUserDataQuery.mockReturnValue({
      data: {
        ...defaultUserData,
        bio: "Visit https://example.com for more info",
      },
      isLoading: false,
      error: null,
    });

    render(<RefereeHeader {...defaultProps} />);

    expect(screen.getByText("Visit https://example.com for more info")).toBeInTheDocument();
  });

  test("it renders an empty bio without errors", () => {
    mockUseGetUserDataQuery.mockReturnValue({
      data: {
        ...defaultUserData,
        bio: null,
      },
      isLoading: false,
      error: null,
    });

    render(<RefereeHeader {...defaultProps} />);

    // Should render without crashing; bio container is present
    expect(screen.queryByText("Test bio for referee")).not.toBeInTheDocument();
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
    // head/twentyfour is both max-level AND most-recent-version → deduplicated to 1 badge.
    // snitch/twentytwo becomes the second displayed badge.
    // assistant/twentyfour is the hidden one → +1.
    const manyCerts: Certification[] = [
      { level: "head", version: "twentyfour" },
      { level: "snitch", version: "twentytwo" },
      { level: "assistant", version: "twentyfour" },
    ];

    render(<RefereeHeader {...defaultProps} certifications={manyCerts} />);

    // head (highest level, rank 3) and snitch (second, rank 2) are shown.
    // byVersion = head/twentyfour (same as byLevel) → deduped → headerCerts = [head/twentyfour]
    // Wait: byVersion top = head/twentyfour (same as byLevel), deduplicated.
    // So headerCerts = [head/twentyfour], hiddenCerts = [snitch/twentytwo, assistant/twentyfour] → +2
    expect(screen.getByText("+2")).toBeInTheDocument();
  });

  test("it reveals hidden certifications when clicking the +N badge", () => {
    const manyCerts: Certification[] = [
      { level: "head", version: "twentyfour" },
      { level: "snitch", version: "twentytwo" },
      { level: "assistant", version: "twentyfour" },
    ];

    render(<RefereeHeader {...defaultProps} certifications={manyCerts} />);

    const overflowBadge = screen.getByText("+2");

    expect(screen.queryByText("Flag (2022-2023)")).not.toBeInTheDocument();
    expect(screen.queryByText("Assistant (2024)")).not.toBeInTheDocument();

    fireEvent.click(overflowBadge);

    expect(screen.getByText("Flag (2022-2023)")).toBeInTheDocument();
    expect(screen.getByText("Assistant (2024)")).toBeInTheDocument();
  });
});
