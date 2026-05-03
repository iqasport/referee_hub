import React, { forwardRef, useImperativeHandle, useMemo, useState } from "react";
import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useCreateInviteMutation, TournamentParticipantViewModel } from "../../../store/serviceApi";
import CustomAlert from "../../../components/CustomAlert";
import { useAlert } from "../../../hooks/useAlert";

export interface VolunteerRegistrationModalRef {
  open: (tournamentId: string, userId: string, existingObservations?: string | null) => void;
}

interface VolunteerRegistrationModalProps {
  teams: TournamentParticipantViewModel[];
  onSaved?: () => void;
}

interface VolunteerRegistrationForm {
  positions: string[];
  excitement: number | "";
  experience: number | "";
  affiliatedTeamId: string;
  associatedTeamIds: string[];
}

const AVAILABLE_POSITIONS = [
  "Head Referee",
  "Flag Referee",
  "Flag Runner",
  "Assistant Referee",
];

function parseExistingForm(existingObservations?: string | null): VolunteerRegistrationForm {
  if (!existingObservations) {
    return {
      positions: [],
      excitement: "",
      experience: "",
      affiliatedTeamId: "",
      associatedTeamIds: [],
    };
  }

  try {
    const parsed = JSON.parse(existingObservations) as Partial<VolunteerRegistrationForm>;
    return {
      positions: Array.isArray(parsed.positions) ? parsed.positions : [],
      excitement: typeof parsed.excitement === "number" ? parsed.excitement : "",
      experience: typeof parsed.experience === "number" ? parsed.experience : "",
      affiliatedTeamId: parsed.affiliatedTeamId || "",
      associatedTeamIds: Array.isArray(parsed.associatedTeamIds) ? parsed.associatedTeamIds : [],
    };
  } catch {
    return {
      positions: [],
      excitement: "",
      experience: "",
      affiliatedTeamId: "",
      associatedTeamIds: [],
    };
  }
}

const VolunteerRegistrationModal = forwardRef<VolunteerRegistrationModalRef, VolunteerRegistrationModalProps>(
  ({ teams, onSaved }, ref) => {
    const { alertState, showAlert, hideAlert } = useAlert();
    const [isOpen, setIsOpen] = useState(false);
    const [tournamentId, setTournamentId] = useState("");
    const [userId, setUserId] = useState("");
    const [form, setForm] = useState<VolunteerRegistrationForm>(parseExistingForm(null));
    const [createInvite, { isLoading }] = useCreateInviteMutation();

    useImperativeHandle(ref, () => ({
      open: (nextTournamentId: string, nextUserId: string, existingObservations?: string | null) => {
        setTournamentId(nextTournamentId);
        setUserId(nextUserId);
        setForm(parseExistingForm(existingObservations));
        setIsOpen(true);
      },
    }));

    const teamsById = useMemo(() => {
      const map = new Map<string, TournamentParticipantViewModel>();
      teams.forEach((team) => {
        if (team.teamId) {
          map.set(team.teamId, team);
        }
      });
      return map;
    }, [teams]);

    const isFormValid = form.positions.length > 0;

    const togglePosition = (position: string) => {
      setForm((prev) => ({
        ...prev,
        positions: prev.positions.includes(position)
          ? prev.positions.filter((p) => p !== position)
          : [...prev.positions, position],
      }));
    };

    const submit = async (event: React.FormEvent) => {
      event.preventDefault();
      if (!tournamentId || !userId) return;
      if (!isFormValid) {
        showAlert("Select at least one volunteer position.", "error");
        return;
      }

      try {
        await createInvite({
          tournamentId,
          createInviteModel: {
            participantType: "referee",
            participantId: userId,
            observations: JSON.stringify(form),
          } as any,
        }).unwrap();

        showAlert("Volunteer registration saved.", "success");
        onSaved?.();
        setIsOpen(false);
      } catch (error) {
        console.error("Failed to save volunteer registration", error);
        showAlert("Failed to save volunteer registration. Please try again.", "error");
      }
    };

    return (
      <>
        {alertState.isVisible && (
          <CustomAlert message={alertState.message} type={alertState.type} onClose={hideAlert} />
        )}
        <Dialog open={isOpen} as="div" className="relative z-50" onClose={() => setIsOpen(false)}>
          <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
          <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
            <DialogPanel className="w-full max-w-2xl rounded-xl bg-white p-6 shadow-xl max-h-full overflow-y-auto">
              <DialogTitle as="h3" className="text-xl font-semibold text-gray-900 mb-4">
                Volunteer Registration
              </DialogTitle>

              <form onSubmit={submit} className="space-y-5">
                <div>
                  <p className="block text-sm font-medium text-gray-700 mb-2">Positions</p>
                  <div className="grid grid-cols-2 gap-2">
                    {AVAILABLE_POSITIONS.map((position) => (
                      <label key={position} className="flex items-center gap-2 text-sm text-gray-800">
                        <input
                          type="checkbox"
                          checked={form.positions.includes(position)}
                          onChange={() => togglePosition(position)}
                        />
                        {position}
                      </label>
                    ))}
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Excitement (0-10)</label>
                    <input
                      type="number"
                      min={0}
                      max={10}
                      value={form.excitement}
                      onChange={(e) =>
                        setForm((prev) => ({
                          ...prev,
                          excitement: e.target.value === "" ? "" : Number(e.target.value),
                        }))
                      }
                      className="w-full px-3 py-2 border border-gray-300 rounded-md"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Experience (0-10)</label>
                    <input
                      type="number"
                      min={0}
                      max={10}
                      value={form.experience}
                      onChange={(e) =>
                        setForm((prev) => ({
                          ...prev,
                          experience: e.target.value === "" ? "" : Number(e.target.value),
                        }))
                      }
                      className="w-full px-3 py-2 border border-gray-300 rounded-md"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Affiliated Team</label>
                  <select
                    value={form.affiliatedTeamId}
                    onChange={(e) => setForm((prev) => ({ ...prev, affiliatedTeamId: e.target.value }))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  >
                    <option value="">None</option>
                    {teams.map((team) => (
                      <option key={team.teamId} value={team.teamId}>
                        {team.teamName}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <p className="block text-sm font-medium text-gray-700 mb-2">Associated Teams</p>
                  <div className="max-h-36 overflow-y-auto border border-gray-200 rounded-md p-2">
                    {teams.length === 0 && <p className="text-sm text-gray-500">No registered teams yet.</p>}
                    {teams.map((team) => {
                      const teamId = team.teamId || "";
                      const checked = form.associatedTeamIds.includes(teamId);
                      return (
                        <label key={teamId} className="flex items-center gap-2 text-sm text-gray-800 py-1">
                          <input
                            type="checkbox"
                            checked={checked}
                            onChange={() => {
                              setForm((prev) => ({
                                ...prev,
                                associatedTeamIds: checked
                                  ? prev.associatedTeamIds.filter((id) => id !== teamId)
                                  : [...prev.associatedTeamIds, teamId],
                              }));
                            }}
                          />
                          {teamsById.get(teamId)?.teamName || "Unknown team"}
                        </label>
                      );
                    })}
                  </div>
                </div>

                <div className="flex justify-end gap-3 pt-2">
                  <button
                    type="button"
                    onClick={() => setIsOpen(false)}
                    className="px-4 py-2 rounded-md border border-gray-300 text-gray-700"
                  >
                    Cancel
                  </button>
                  <button type="submit" disabled={isLoading} className="btn btn-primary">
                    {isLoading ? "Saving..." : "Save Volunteer Registration"}
                  </button>
                </div>
              </form>
            </DialogPanel>
          </div>
        </Dialog>
      </>
    );
  }
);

VolunteerRegistrationModal.displayName = "VolunteerRegistrationModal";

export default VolunteerRegistrationModal;
