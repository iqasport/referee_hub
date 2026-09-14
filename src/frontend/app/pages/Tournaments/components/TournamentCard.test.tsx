import React from "react";
import { render, screen } from "@testing-library/react";
import TournamentCard from "./TournamentCard";

describe("TournamentCard", () => {
  const baseProps = {
    title: "Spring Cup",
    description: "Competitive weekend tournament",
    startDate: "2026-05-10",
    endDate: "2026-05-11",
    type: "Club" as const,
    country: "Spain",
    location: "Madrid",
  };

  it("renders volunteer registration badge when enabled", () => {
    render(<TournamentCard {...baseProps} showVolunteerRegistrationBadge={true} />);

    expect(screen.getByText("Searching for volunteers")).toBeInTheDocument();
  });

  it("does not render volunteer registration badge when disabled", () => {
    render(<TournamentCard {...baseProps} showVolunteerRegistrationBadge={false} />);

    expect(screen.queryByText("Searching for volunteers")).not.toBeInTheDocument();
  });
});
