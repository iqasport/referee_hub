import { faCheck, faXmark } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classNames from "classnames";
import React, { useMemo, useState } from "react";
import FilterToolbar from "../../components/FilterToolbar";
import Toggle from "../../components/Toggle";
import {
  NgbTransferViewModel,
  TransferApprovalStatus,
  useApproveNgbTransferMutation,
  useGetNgbTransfersQuery,
  useRejectNgbTransferMutation,
  useUpdateNgbTransferSettingsMutation,
} from "../../store/serviceApi";

// ─── Status badge ─────────────────────────────────────────────────────────────

const STATUS_LABELS: Record<TransferApprovalStatus, string> = {
  notATransfer: "New join",
  pendingNgbApproval: "Pending approval",
  pendingTeamApproval: "Pending approval",
  rejectedByNgb: "Rejected",
  approved: "Accepted",
  declined: "Cancelled",
};

const STATUS_COLORS: Record<TransferApprovalStatus, string> = {
  notATransfer: "bg-gray-100 text-gray-700",
  pendingNgbApproval: "bg-yellow-100 text-yellow-800",
  pendingTeamApproval: "bg-yellow-100 text-yellow-800",
  rejectedByNgb: "bg-red-100 text-red-700",
  approved: "bg-green-100 text-green-800",
  declined: "bg-gray-200 text-gray-700",
};

const StatusBadge: React.FC<{ status?: TransferApprovalStatus }> = ({ status }) => {
  if (!status) return null;
  return (
    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_COLORS[status]}`}>
      {STATUS_LABELS[status]}
    </span>
  );
};

const TeamCell: React.FC<{ name?: string | null; logoUri?: string | null }> = ({ name, logoUri }) => {
  if (!name) return <span className="italic text-gray-400">None (new join)</span>;

  return (
    <div className="flex items-center gap-2">
      {logoUri ? (
        <img src={logoUri} alt="" className="h-8 w-8 shrink-0 rounded object-cover" />
      ) : (
        <div className="h-8 w-8 shrink-0 rounded bg-gray-100" aria-hidden="true" />
      )}
      <span>{name}</span>
    </div>
  );
};

// ─── Per-row action menu ───────────────────────────────────────────────────────

interface TransferActionsProps {
  transfer: NgbTransferViewModel;
  ngbId: string;
}

const normalizeStatus = (status?: string | null): TransferApprovalStatus | undefined => {
  switch (status) {
    case "notATransfer":
    case "NotATransfer":
      return "notATransfer";
    case "pendingNgbApproval":
    case "PendingNgbApproval":
      return "pendingNgbApproval";
    case "pendingTeamApproval":
    case "PendingTeamApproval":
      return "pendingTeamApproval";
    case "rejectedByNgb":
    case "RejectedByNgb":
      return "rejectedByNgb";
    case "approved":
    case "Approved":
      return "approved";
    case "declined":
    case "Declined":
      return "declined";
    default:
      return undefined;
  }
};

const TransferActions: React.FC<TransferActionsProps> = ({ transfer, ngbId }) => {
  const [open, setOpen] = useState(false);
  const [approve] = useApproveNgbTransferMutation();
  const [reject] = useRejectNgbTransferMutation();
  const status = normalizeStatus(transfer.status);
  const invitationId = transfer.invitationId;

  const canAct = status === "pendingNgbApproval" && Boolean(invitationId);

  const handleApprove = async () => {
    if (!canAct || !invitationId) return;
    setOpen(false);
    await approve({ ngb: ngbId, invitationId });
  };

  const handleReject = async () => {
    if (!canAct || !invitationId) return;
    setOpen(false);
    if (confirm(`Reject transfer for ${transfer.playerEmail ?? "this player"}?`)) {
      await reject({ ngb: ngbId, invitationId });
    }
  };

  return (
    <div className={classNames("relative inline-block text-left", { "z-20": open })}>
      <button
        className="rounded p-1 hover:bg-gray-200"
        aria-label="Actions"
        onClick={() => setOpen((v) => !v)}
      >
        <span className="text-xl leading-none">⋯</span>
      </button>
      {open && (
        <div className="absolute right-0 z-30 mt-1 w-40 rounded border border-gray-200 bg-white p-1 shadow-lg">
          <button
            className="flex w-full items-center gap-2 rounded px-2 py-1.5 text-left text-sm hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
            onClick={handleApprove}
            disabled={!canAct}
          >
            <FontAwesomeIcon icon={faCheck} className="w-4 shrink-0 text-green-700" />
            <span>Approve transfer</span>
          </button>
          <button
            className="flex w-full items-center gap-2 rounded px-2 py-1.5 text-left text-sm text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
            onClick={handleReject}
            disabled={!canAct}
          >
            <FontAwesomeIcon icon={faXmark} className="w-4 shrink-0" />
            <span>Reject transfer</span>
          </button>
          {!canAct && (
            <p className="border-t px-4 py-2 text-xs text-gray-500">No actions available for this status</p>
          )}
        </div>
      )}
    </div>
  );
};

// ─── Main tab component ────────────────────────────────────────────────────────

interface NgbTransfersTabProps {
  ngbId: string;
}

const NgbTransfersTab: React.FC<NgbTransfersTabProps> = ({ ngbId }) => {
  const [page, setPage] = useState(1);
  const [filter, setFilter] = useState<string>();
  const { data: transfers, isLoading, error } = useGetNgbTransfersQuery({ ngb: ngbId, filter, page, pageSize: 25 });
  const [updateSettings, { isLoading: isSaving }] = useUpdateNgbTransferSettingsMutation();

  const orderedTransfers = useMemo(() => {
    if (!transfers?.items) return [];

    const isPending = (status?: TransferApprovalStatus) => status === "pendingNgbApproval";
    return [...transfers.items].sort((a, b) => {
      const aPending = isPending(normalizeStatus(a.status));
      const bPending = isPending(normalizeStatus(b.status));
      if (aPending !== bPending) return aPending ? -1 : 1;

      const aTime = a.createdAt ? new Date(a.createdAt).getTime() : 0;
      const bTime = b.createdAt ? new Date(b.createdAt).getTime() : 0;
      return bTime - aTime;
    });
  }, [transfers]);

  // Derive current auto-approve state from first record (all share same NGB setting).
  // We don't have a direct GET for NGB settings yet, so we derive it optimistically.
  const [autoApprove, setAutoApprove] = useState(false);

  const handleSearch = (value: string) => {
    setFilter(value || undefined);
    setPage(1);
  };

  const handleAutoApproveToggle = async (checked: boolean) => {
    setAutoApprove(checked);
    try {
      await updateSettings({ ngb: ngbId, body: { autoApproveInternalTransfers: checked } }).unwrap();
    } catch {
      setAutoApprove(!checked); // revert on error
    }
  };

  if (isLoading) {
    return <p className="text-gray-500 p-4">Loading transfers…</p>;
  }

  if (error) {
    return <p className="text-red-500 p-4">Failed to load transfers.</p>;
  }

  return (
    <div className="space-y-4">
      {/* Auto-approve setting */}
      <div className="flex items-center justify-between rounded border border-gray-200 bg-white p-4">
        <div>
          <p className="font-semibold text-gray-900">Auto-approve internal transfers</p>
          <p className="text-sm text-gray-500">
            Automatically approve transfers between two teams within this NGB.
            International transfers always require manual review.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Toggle
            name="autoApproveInternalTransfers"
            checked={autoApprove}
            onChange={(e) => handleAutoApproveToggle(e.target.checked)}
          />
          {isSaving && <span className="text-xs text-gray-500">Saving…</span>}
        </div>
      </div>

      {/* Transfer table */}
      <FilterToolbar
        currentPage={page}
        onClearSearch={() => handleSearch("")}
        total={transfers?.metadata?.totalCount ?? 0}
        onSearchInput={handleSearch}
        onPageSelect={setPage}
      />
      {orderedTransfers.length === 0 ? (
        <div className="rounded-md border border-gray-200 bg-gray-50 p-4 text-gray-500">
          No transfers to show yet.
        </div>
      ) : (
        <div className="overflow-x-auto rounded border border-gray-200">
          <table className="min-w-full bg-white text-sm">
            <thead className="bg-gray-100">
              <tr>
                <th className="px-4 py-2 text-left font-medium">Player</th>
                <th className="px-4 py-2 text-left font-medium">Origin team</th>
                <th className="px-4 py-2 text-left font-medium">Destination team</th>
                <th className="px-4 py-2 text-left font-medium">Type</th>
                <th className="px-4 py-2 text-left font-medium">Status</th>
                <th className="px-4 py-2 text-left font-medium">Date</th>
                <th className="px-4 py-2 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {orderedTransfers.map((t) => (
                <tr key={t.invitationId} className="border-t hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <p className="font-medium">{t.playerName ?? "—"}</p>
                    <p className="text-xs text-gray-500">{t.playerEmail}</p>
                  </td>
                  <td className="px-4 py-3 text-gray-700">
                    <TeamCell name={t.originTeamName} logoUri={t.originTeamLogoUri} />
                  </td>
                  <td className="px-4 py-3 text-gray-700">
                    <TeamCell name={t.destinationTeamName} logoUri={t.destinationTeamLogoUri} />
                  </td>
                  <td className="px-4 py-3">
                    {t.originTeamName ? (
                      <div>
                        <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${t.isInternalTransfer ? "bg-indigo-100 text-indigo-700" : "bg-orange-100 text-orange-700"}`}>
                          {t.isInternalTransfer ? "Internal" : "International"}
                        </span>
                        {!t.isInternalTransfer && t.originNgbCode && t.destinationNgbCode && (
                          <p className="mt-1 text-xs text-gray-500">{t.originNgbCode} → {t.destinationNgbCode}</p>
                        )}
                      </div>
                    ) : "—"}
                  </td>
                  <td className="px-4 py-3">
                    <StatusBadge status={normalizeStatus(t.status)} />
                  </td>
                  <td className="px-4 py-3 text-gray-500">
                    {t.createdAt ? new Date(t.createdAt).toLocaleDateString() : "—"}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <TransferActions transfer={t} ngbId={ngbId} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default NgbTransfersTab;
