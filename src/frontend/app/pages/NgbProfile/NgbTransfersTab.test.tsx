import { fireEvent, render, screen } from "@testing-library/react";
import React from "react";

import { useGetNgbTransfersQuery } from "../../store/serviceApi";
import NgbTransfersTab from "./NgbTransfersTab";

jest.mock("../../store/serviceApi", () => ({
  useApproveNgbTransferMutation: () => [jest.fn()],
  useGetNgbTransfersQuery: jest.fn(),
  useRejectNgbTransferMutation: () => [jest.fn()],
  useUpdateNgbTransferSettingsMutation: () => [jest.fn(), { isLoading: false }],
}));

const mockUseGetNgbTransfersQuery = useGetNgbTransfersQuery as jest.Mock;

describe("NgbTransfersTab", () => {
  beforeEach(() => {
    mockUseGetNgbTransfersQuery.mockReturnValue({
      data: {
        items: [{
          invitationId: "TI_1",
          playerName: "Player One",
          originTeamName: "BA Jacks",
          originTeamLogoUri: "/origin-logo.png",
          originNgbCode: "ARG",
          destinationTeamName: "Yankees",
          destinationTeamLogoUri: "/destination-logo.png",
          destinationNgbCode: "USA",
          isInternalTransfer: false,
          status: "pendingNgbApproval",
        }],
        metadata: { totalCount: 26 },
      },
      error: undefined,
      isLoading: false,
    });
  });

  it("requests and navigates through 25-row pages", () => {
    render(<NgbTransfersTab ngbId="USA" />);

    expect(screen.getByText("Pending approval")).toHaveClass("rounded-full", "bg-yellow-100", "text-yellow-800");
    expect(screen.getByText("ARG → USA")).toBeInTheDocument();
    expect(screen.getAllByRole("img")).toHaveLength(2);

    fireEvent.click(screen.getByRole("button", { name: "Actions" }));
    expect(screen.getByRole("button", { name: "Approve transfer" })).toHaveClass("py-1.5");
    expect(screen.getByRole("button", { name: "Reject transfer" })).toHaveClass("py-1.5");

    expect(mockUseGetNgbTransfersQuery).toHaveBeenLastCalledWith({
      ngb: "USA",
      filter: undefined,
      page: 1,
      pageSize: 25,
    });

    fireEvent.click(screen.getByTitle("2"));

    expect(mockUseGetNgbTransfersQuery).toHaveBeenLastCalledWith({
      ngb: "USA",
      filter: undefined,
      page: 2,
      pageSize: 25,
    });
  });

  it("styles pending, accepted, and rejected statuses consistently with type badges", () => {
    mockUseGetNgbTransfersQuery.mockReturnValue({
      data: {
        items: [
          { invitationId: "TI_1", status: "pendingNgbApproval" },
          { invitationId: "TI_2", status: "approved" },
          { invitationId: "TI_3", status: "rejectedByNgb" },
        ],
        metadata: { totalCount: 3 },
      },
      error: undefined,
      isLoading: false,
    });

    render(<NgbTransfersTab ngbId="USA" />);

    expect(screen.getByText("Pending approval")).toHaveClass("rounded-full", "bg-yellow-100", "text-yellow-800");
    expect(screen.getByText("Accepted")).toHaveClass("rounded-full", "bg-green-100", "text-green-800");
    expect(screen.getByText("Rejected")).toHaveClass("rounded-full", "bg-red-100", "text-red-700");
  });
});
