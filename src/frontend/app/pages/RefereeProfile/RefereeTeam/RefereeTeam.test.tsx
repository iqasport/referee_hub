import React from "react";
import { render, screen, mockedStore } from "../../../utils/test-utils";

import factories from "../../../factories";
import RefereeTeam from "./RefereeTeam";
import { AssociationData } from "../../../apis/referee";

describe("RefereeTeam", () => {
  const allTeams = factories.team.buildList(10);
  const locations = factories.ngb.buildList(5);
  const associationValue: AssociationData = { teams: null, locations: null };
  const defaultStore = {
    teams: {
      teams: allTeams,
      filters: {},
    },
    locations: {
      locations,
    },
  };
  const mockStore = mockedStore(defaultStore);
  const defaultProps = {
    teams: [],
    locations: [],
    isEditing: false,
    onChange: jest.fn(),
    associationValue,
    isDisabled: false,
  };

  it("renders the component", () => {
    render(<RefereeTeam {...defaultProps} />, mockStore);

    expect(screen.getAllByText("Team not selected")).toHaveLength(2);
  });
});
