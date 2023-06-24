import React from "react";
import { fireEvent, render, screen } from "../../../utils/test-utils";

import { UpdateRefereeRequest } from "../../../apis/referee";
import { DataAttributes, IncludedAttributes } from "../../../schemas/getRefereeSchema";
import RefereeHeader from "./RefereeHeader";

/** TODO: rewrite tests
describe("RefereeHeader", () => {
  const referee: DataAttributes = {
    avatarUrl: null,
    bio: "words",
    createdAt: new Date().toString(),
    exportName: false,
    firstName: "Build",
    hasPendingPolicies: false,
    isEditable: true,
    lastName: "Stuff",
    pronouns: "she/her",
    showPronouns: true,
    submittedPaymentAt: new Date(),
  };
  const certifications: IncludedAttributes[] = [
    {
      level: "snitch",
    },
    {
      level: "assistant",
    },
  ];
  const updatedValues: UpdateRefereeRequest = {
    bio: referee.bio,
    exportName: referee.exportName,
    firstName: referee.firstName,
    lastName: referee.lastName,
    ngbData: {},
    pronouns: referee.pronouns,
    showPronouns: referee.showPronouns,
    submittedPaymentAt: referee.submittedPaymentAt,
    teamsData: {},
  };
  const defaultProps = {
    certifications,
    id: "123",
    isEditing: false,
    onCancel: jest.fn(),
    onChange: jest.fn(),
    onEditClick: jest.fn(),
    onSubmit: jest.fn(),
    referee,
    updatedValues,
  };

  test("it renders the referee name", () => {
    render(<RefereeHeader {...defaultProps} />);
    const fullName = `${referee.firstName} ${referee.lastName}`;
    screen.getByText(fullName);
  });

  test("it renders a default name if names not present", () => {
    const noName: DataAttributes = {
      ...referee,
      firstName: null,
      lastName: null,
    };
    const noNameProps = {
      ...defaultProps,
      referee: noName,
    };
    render(<RefereeHeader {...noNameProps} />);

    screen.getByText("Anonymous Referee");
  });

  test("it renders pronouns", () => {
    render(<RefereeHeader {...defaultProps} />);

    screen.getByText(referee.bio);
  });

  test("it doesn't render pronouns when show pronouns is false", () => {
    const noPronounsProps = {
      ...defaultProps,
      referee: {
        ...referee,
        showPronouns: false,
      },
    };
    render(<RefereeHeader {...noPronounsProps} />);

    expect(screen.queryByText(referee.pronouns)).toBeNull();
  });

  test("it renders certifications", () => {
    render(<RefereeHeader {...defaultProps} />);

    screen.getByText("Snitch (Unknown)");
    screen.getByText("Assistant (Unknown)");
  });

  test("it renders the bio", () => {
    render(<RefereeHeader {...defaultProps} />);

    screen.getByText(referee.bio);
  });

  test("it calls the edit function on edit button click", () => {
    render(<RefereeHeader {...defaultProps} />);

    const editButton = screen.getByText("Edit");
    fireEvent.click(editButton);

    expect(defaultProps.onEditClick).toHaveBeenCalled();
  });

  describe("while editing", () => {
    const editProps = {
      ...defaultProps,
      isEditing: true,
    };

    test("it renders a save button", () => {
      render(<RefereeHeader {...editProps} />);

      screen.getByText("Save Changes");
    });

    test("it does not render certifications", () => {
      render(<RefereeHeader {...editProps} />);

      expect(screen.queryByText("Snitch")).toBeNull();
      expect(screen.queryByText("Assistant")).toBeNull();
    });

    test("it renders an export name toggle", () => {
      render(<RefereeHeader {...editProps} />);

      expect(screen.getByLabelText("Export Name?")).toBeDefined();
    });

    test("it renders pronoun editing", () => {
      render(<RefereeHeader {...editProps} />);

      expect(screen.getByLabelText("Show Pronouns?")).toBeDefined();
      expect(screen.getAllByRole("textbox")[0].getAttribute("value")).toEqual(referee.pronouns);
    });

    test("it renders bio editing", () => {
      render(<RefereeHeader {...editProps} />);

      const bio = screen.getAllByRole("textbox")[1];

      expect(bio.innerHTML).toEqual(referee.bio);
    });

    test("it fires the change event when a value has changed", () => {
      render(<RefereeHeader {...editProps} />);

      fireEvent.click(screen.getByLabelText("Show Pronouns?"));

      expect(defaultProps.onChange).toHaveBeenCalledWith(false, "showPronouns");
    });

    describe("without a name", () => {
      const noNameProps = {
        ...editProps,
        referee: {
          ...referee,
          firstName: null,
          lastName: null,
        },
        updatedValues: {
          ...updatedValues,
          firstName: null,
          lastName: null,
        },
      };

      test("it renders name inputs", () => {
        render(<RefereeHeader {...noNameProps} />);

        const allInputs = screen.getAllByRole("textbox");
        const firstName = allInputs[0];
        const lastName = allInputs[1];

        expect(firstName.getAttribute("value")).toEqual("");
        expect(lastName.getAttribute("value")).toEqual("");
      });
    });
  });
});
*/
