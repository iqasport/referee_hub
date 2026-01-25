import React from "react";
import { render, screen } from "@testing-library/react";
import TournamentAboutSection from "./TournamentAboutSection";

describe("TournamentAboutSection", () => {
  it("renders location information correctly", () => {
    render(
      <TournamentAboutSection
        place="Central Arena"
        city="Madrid"
        country="Spain"
        description="Test description"
      />
    );

    expect(screen.getByText("Central Arena, Madrid, Spain")).toBeInTheDocument();
  });

  it("renders TBD when location is not provided", () => {
    render(<TournamentAboutSection description="Test description" />);

    expect(screen.getByText("TBD")).toBeInTheDocument();
  });

  it("renders description text", () => {
    const description = "This is a test tournament";
    render(<TournamentAboutSection description={description} />);

    expect(screen.getByText(description)).toBeInTheDocument();
  });

  it("renders default message when description is null", () => {
    render(<TournamentAboutSection description={null} />);

    expect(screen.getByText("No description provided.")).toBeInTheDocument();
  });

  it("renders default message when description is empty", () => {
    render(<TournamentAboutSection description="" />);

    expect(screen.getByText("No description provided.")).toBeInTheDocument();
  });

  it("renders the About This Tournament heading", () => {
    render(<TournamentAboutSection description="Test" />);

    expect(screen.getByText("About This Tournament")).toBeInTheDocument();
  });

  it("renders the Description heading", () => {
    render(<TournamentAboutSection description="Test" />);

    expect(screen.getByText("Description")).toBeInTheDocument();
  });
});

