import React from "react";
import { fireEvent, render, screen } from "../../../utils/test-utils-rtk";
import { createQuerySuccess, createMutation } from "../../../utils/test-rtk-helpers";

import RefereeHeader from "./RefereeHeader";

// Mock the navigation params
jest.mock("../../../utils/navigationUtils", () => ({
  useNavigationParams: () => ({ refereeId: "test-user-123" }),
}));

// Mock RTK Query hooks
jest.mock("../../../store/serviceApi", () => ({
  ...jest.requireActual("../../../store/serviceApi"),
  useGetUserDataQuery: jest.fn(),
  useGetUserAvatarQuery: jest.fn(),
  useUpdateCurrentUserDataMutation: jest.fn(),
}));

import {
  useGetUserDataQuery,
  useGetUserAvatarQuery,
  useUpdateCurrentUserDataMutation,
} from "../../../store/serviceApi";

describe("RefereeHeader", () => {
  const mockUserData = {
    firstName: "Test",
    lastName: "User",
    bio: "Test bio",
    pronouns: "they/them",
    showPronouns: true,
    exportName: true,
    language: "en-US",
    createdAt: "2023-01-01T00:00:00Z",
  };

  const mockCertifications = [
    { level: "snitch" as const, version: "twentyfour" as const },
    { level: "assistant" as const, version: "twentyfour" as const },
  ];

  const defaultProps = {
    name: "Test User",
    certifications: mockCertifications,
    isEditable: true,
  };

  beforeEach(() => {
    (useGetUserDataQuery as jest.Mock).mockReturnValue(createQuerySuccess(mockUserData));
    (useGetUserAvatarQuery as jest.Mock).mockReturnValue(createQuerySuccess("http://example.com/avatar.jpg"));
    (useUpdateCurrentUserDataMutation as jest.Mock).mockReturnValue(createMutation());
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  test("it renders the referee name", () => {
    render(<RefereeHeader {...defaultProps} />);
    
    expect(screen.getByText("Test User")).toBeInTheDocument();
  });

  test("it renders pronouns when showPronouns is true", () => {
    render(<RefereeHeader {...defaultProps} />);

    expect(screen.getByText(mockUserData.pronouns)).toBeInTheDocument();
  });

  test("it doesn't render pronouns when showPronouns is false", () => {
    const userData = { ...mockUserData, showPronouns: false };
    (useGetUserDataQuery as jest.Mock).mockReturnValue(createQuerySuccess(userData));

    render(<RefereeHeader {...defaultProps} />);

    expect(screen.queryByText(mockUserData.pronouns)).not.toBeInTheDocument();
  });

  test("it renders certifications", () => {
    render(<RefereeHeader {...defaultProps} />);

    // Snitch level is displayed as "Flag"
    expect(screen.getByText(/Flag/i)).toBeInTheDocument();
    expect(screen.getByText(/Assistant/i)).toBeInTheDocument();
  });

  test("it renders the bio", () => {
    render(<RefereeHeader {...defaultProps} />);

    expect(screen.getByText(mockUserData.bio)).toBeInTheDocument();
  });

  test("it calls the edit function on edit button click", () => {
    render(<RefereeHeader {...defaultProps} />);

    const editButton = screen.getByText("Edit");
    fireEvent.click(editButton);

    // Should show save button when editing
    expect(screen.getByText("Save Changes")).toBeInTheDocument();
  });

  describe("while editing", () => {
    test("it renders a save button", () => {
      render(<RefereeHeader {...defaultProps} />);

      fireEvent.click(screen.getByText("Edit"));

      expect(screen.getByText("Save Changes")).toBeInTheDocument();
    });

    test("it renders form fields for editing", () => {
      render(<RefereeHeader {...defaultProps} />);

      fireEvent.click(screen.getByText("Edit"));

      // Check for some form elements
      expect(screen.getByLabelText(/show pronouns/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/export name/i)).toBeInTheDocument();
    });

    test("it can cancel editing", () => {
      render(<RefereeHeader {...defaultProps} />);

      fireEvent.click(screen.getByText("Edit"));
      expect(screen.getByText("Save Changes")).toBeInTheDocument();

      fireEvent.click(screen.getByText("Cancel"));
      expect(screen.queryByText("Save Changes")).not.toBeInTheDocument();
    });
  });

  describe("when data is loading", () => {
    test("it renders empty content when data is loading", () => {
      (useGetUserDataQuery as jest.Mock).mockReturnValue({
        data: undefined,
        isLoading: true,
        error: undefined,
      });

      const { container } = render(<RefereeHeader {...defaultProps} />);

      // Component returns empty fragment when user data is not available (line 134: if (!user) return <></>)
      expect(container.querySelector('div')).toBeNull();
    });
  });
});
