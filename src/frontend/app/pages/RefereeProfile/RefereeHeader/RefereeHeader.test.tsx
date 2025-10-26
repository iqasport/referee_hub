import React from "react";
import { render, screen } from "../../../utils/test-utils";
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
});
