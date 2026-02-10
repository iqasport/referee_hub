import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle } from "react";
import React from "react";
import { useGetTeamRosterQuery, RosterEntryViewModel } from "../../../store/serviceApi";

export interface RosterViewModalRef {
  open: (tournamentId: string, teamId: string, teamName: string, tournamentName: string) => void;
}

const RosterViewModal = forwardRef<RosterViewModalRef>((_props, ref) => {
  const [isOpen, setIsOpen] = useState(false);
  const [tournamentId, setTournamentId] = useState<string>("");
  const [teamId, setTeamId] = useState<string>("");
  const [teamName, setTeamName] = useState<string>("");
  const [tournamentName, setTournamentName] = useState<string>("");

  const { data: roster, isLoading, isError } = useGetTeamRosterQuery(
    { tournamentId, teamId },
    { skip: !tournamentId || !teamId }
  );

  useImperativeHandle(ref, () => ({
    open: (tournId: string, tId: string, tName: string, trnmtName: string) => {
      setTournamentId(tournId);
      setTeamId(tId);
      setTeamName(tName);
      setTournamentName(trnmtName);
      setIsOpen(true);
    },
  }));

  function close() {
    setIsOpen(false);
  }

  function downloadCSV() {
    if (!roster) return;

    const headers = [
      "Name",
      "Pronouns",
      "Gender",
      "Jersey Number",
      "Role",
      "Max Certification",
      "Certification Date",
    ];

    const csvRows = [headers.join(",")];

    roster.forEach((entry: RosterEntryViewModel) => {
      const row = [
        `"${entry.name || ""}"`,
        `"${entry.pronouns || ""}"`,
        `"${entry.gender || ""}"`,
        `"${entry.jerseyNumber || ""}"`,
        `"${entry.role || ""}"`,
        `"${entry.maxCertification || ""}"`,
        entry.maxCertificationDate
          ? `"${new Date(entry.maxCertificationDate).toLocaleDateString()}"`
          : '""',
      ];
      csvRows.push(row.join(","));
    });

    const csvContent = csvRows.join("\n");
    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");
    const url = URL.createObjectURL(blob);

    const date = new Date().toISOString().split("T")[0].replace(/-/g, "");
    // Sanitize names for filename (remove special characters)
    const sanitizeName = (name: string) => name.replace(/[^a-zA-Z0-9]/g, "_");
    const filename = `${sanitizeName(tournamentName)}_${sanitizeName(teamName)}_roster_${date}.csv`;

    link.setAttribute("href", url);
    link.setAttribute("download", filename);
    link.style.visibility = "hidden";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  return (
    <Dialog open={isOpen} as="div" className="relative z-50" onClose={close}>
      <div
        className="fixed inset-0"
        style={{ backgroundColor: "rgba(0, 0, 0, 0.3)" }}
        aria-hidden="true"
      />
      <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
        <DialogPanel className="relative w-full max-w-5xl rounded-xl bg-white shadow-xl my-8 max-h-screen overflow-hidden flex flex-col">
          {/* Header */}
          <div className="p-6 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <DialogTitle as="h3" className="text-xl font-semibold text-gray-900">
                {teamName} - Roster
              </DialogTitle>
              <button
                onClick={close}
                className="text-gray-400 hover:text-gray-600"
                style={{ fontSize: "24px", lineHeight: 1 }}
              >
                ×
              </button>
            </div>
          </div>

          {/* Content */}
          <div className="p-6 overflow-y-auto" style={{ maxHeight: "70vh" }}>
            {isLoading && (
              <div className="text-center py-8 text-gray-600">Loading roster...</div>
            )}

            {isError && (
              <div className="text-center py-8 text-red-600">
                Failed to load roster. Please try again.
              </div>
            )}

            {!isLoading && !isError && roster && roster.length === 0 && (
              <div className="text-center py-8 text-gray-600">
                No roster entries found for this team.
              </div>
            )}

            {!isLoading && !isError && roster && roster.length > 0 && (
              <>
                <div className="mb-4 flex justify-end">
                  <button
                    onClick={downloadCSV}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    Download CSV
                  </button>
                </div>

                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-gray-200 border border-gray-200 rounded-lg">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Name
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Pronouns
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Gender
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Jersey #
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Role
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Certification
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Cert. Date
                        </th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                      {roster.map((entry: RosterEntryViewModel, index: number) => (
                        <tr key={`${entry.name || 'unknown'}-${entry.role || 'role'}-${index}`} className="hover:bg-gray-50">
                          <td className="px-4 py-3 text-sm text-gray-900">
                            {entry.name || "-"}
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-600">
                            {entry.pronouns || "-"}
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-600">
                            {entry.gender || "-"}
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-600">
                            {entry.jerseyNumber || "-"}
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-600">
                            {entry.role || "-"}
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-600">
                            {entry.maxCertification || "-"}
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-600">
                            {entry.maxCertificationDate
                              ? new Date(entry.maxCertificationDate).toLocaleDateString()
                              : "-"}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>

                <div className="mt-4 text-sm text-gray-600">
                  Total: {roster.length} {roster.length === 1 ? "person" : "people"}
                </div>
              </>
            )}
          </div>

          {/* Footer */}
          <div className="p-6 border-t border-gray-200">
            <button
              onClick={close}
              className="w-full px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors"
            >
              Close
            </button>
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
});

RosterViewModal.displayName = "RosterViewModal";

export default RosterViewModal;
