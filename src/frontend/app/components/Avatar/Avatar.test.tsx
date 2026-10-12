import React from "react";

import { fireEvent, render, screen } from "../../utils/test-utils";

import Avatar from "./";

// Mock the RTK Query hooks
const mockUseGetCurrentUserAvatarQuery = jest.fn();
const mockUseGetManagedTeamsQuery = jest.fn();

jest.mock("../../store/serviceApi", () => ({
  ...jest.requireActual("../../store/serviceApi"),
  useGetCurrentUserAvatarQuery: () => mockUseGetCurrentUserAvatarQuery(),
  useGetManagedTeamsQuery: () => mockUseGetManagedTeamsQuery(),
}));

describe("Avatar", () => {
  const defaultProps = {
    enabledFeatures: [],
    firstName: "Quidditch",
    lastName: "Rocks",
    ownedNgbId: 1234,
    roles: ["Referee"],
    unreadNotifications: 0,
    userId: "123",
  };

  beforeEach(() => {
    jest.clearAllMocks();
    mockUseGetManagedTeamsQuery.mockReturnValue({ data: [] });
    mockUseGetCurrentUserAvatarQuery.mockReturnValue({ data: undefined });
  });

  test("it renders initials when no profile picture is set", () => {
    render(<Avatar {...defaultProps} />);

    expect(screen.getByText("QR")).toBeInTheDocument();
  });

  test("it renders the profile picture when one is available", () => {
    mockUseGetCurrentUserAvatarQuery.mockReturnValue({ data: "https://example.com/avatar.jpg" });

    render(<Avatar {...defaultProps} />);

    const img = screen.getByRole("img", { name: "Profile picture of Quidditch Rocks" });
    expect(img).toBeInTheDocument();
    expect(img).toHaveAttribute("src", "https://example.com/avatar.jpg");
    expect(screen.queryByText("QR")).not.toBeInTheDocument();
  });

  test("it shows My Profile for referees", () => {
    render(<Avatar {...defaultProps} />);

    fireEvent.click(screen.getByRole("button"));

    expect(screen.getByRole("link", { name: "My Profile" })).toBeInTheDocument();
  });

  test("it hides My Profile for users without the referee role", () => {
    render(<Avatar {...defaultProps} roles={["NgbAdmin"]} />);

    fireEvent.click(screen.getByRole("button"));

    expect(screen.queryByRole("link", { name: "My Profile" })).not.toBeInTheDocument();
  });
});
