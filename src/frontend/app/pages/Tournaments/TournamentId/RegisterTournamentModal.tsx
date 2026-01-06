import { Button, Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle } from "react";
import React from "react";

interface Tournament {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  country: string;
  city: string;
}

interface RegistrationFormData {
  teamName: string;
  contactName: string;
  contactEmail: string;
  contactPhone: string;
  numberOfPlayers: string;
  additionalNotes: string;
}

export interface RegisterTournamentModalRef {
  open: (tournament: Tournament) => void;
}

const RegisterTournamentModal = forwardRef<RegisterTournamentModalRef>(
  (_props, ref) => {
    const [isOpen, setIsOpen] = useState(false);
    const [tournament, setTournament] = useState<Tournament | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    
    const initialFormData: RegistrationFormData = {
      teamName: "",
      contactName: "",
      contactEmail: "",
      contactPhone: "",
      numberOfPlayers: "",
      additionalNotes: "",
    };
    
    const [formData, setFormData] = useState<RegistrationFormData>(initialFormData);

    useImperativeHandle(ref, () => ({
      open: (tournamentData: Tournament) => {
        setTournament(tournamentData);
        setFormData(initialFormData);
        setIsOpen(true);
      },
    }));

    function close() {
      setIsOpen(false);
    }

    async function handleSubmit(e: React.FormEvent) {
      e.preventDefault();
      setIsSubmitting(true);
      
      try {
        // TODO: Implement actual API call when backend endpoint is ready
        // Example: await registerForTournament({ tournamentId: tournament?.id, ...formData }).unwrap();
        
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 1000));
        
        console.log("Registration submitted:", {
          tournamentId: tournament?.id,
          ...formData,
        });
        
        alert(`Successfully registered for ${tournament?.name}!`);
        close();
      } catch (error) {
        console.error("Failed to register for tournament:", error);
        alert("Failed to register for tournament. Please try again.");
      } finally {
        setIsSubmitting(false);
      }
    }

    function handleChange(
      e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
    ) {
      const { name, value } = e.target;
      setFormData((prev) => ({
        ...prev,
        [name]: value,
      }));
    }

    const startDate = tournament ? new Date(tournament.startDate) : null;
    const endDate = tournament ? new Date(tournament.endDate) : null;
    const formattedDateRange = startDate && endDate
      ? `${startDate.toLocaleDateString("en-US", {
          month: "short",
          day: "numeric",
        })} - ${endDate.toLocaleDateString("en-US", {
          month: "short",
          day: "numeric",
          year: "numeric",
        })}`
      : "";

    return (
      <Dialog open={isOpen && !!tournament} as="div" className="relative z-50" onClose={close}>
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
          <DialogPanel className="w-full max-w-2xl rounded-xl bg-white p-6 shadow-xl my-8 max-h-full overflow-y-auto">
            <DialogTitle as="h3" className="text-xl font-semibold text-gray-900 mb-2">
              Register for Tournament
            </DialogTitle>
            
            {/* Tournament Info */}
            <div className="bg-gray-50 rounded-lg p-4 mb-6">
              <h4 className="font-semibold text-gray-900 mb-1">{tournament?.name}</h4>
              <p className="text-sm text-gray-600">
                {formattedDateRange} • {tournament?.city}, {tournament?.country}
              </p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
              {/* Team Information */}
              <div>
                <h5 className="text-sm font-semibold text-gray-900 mb-3">Team Information</h5>
                
                <div className="mb-4">
                  <label htmlFor="teamName" className="block text-sm font-medium text-gray-700 mb-1">
                    Team Name *
                  </label>
                  <input
                    type="text"
                    id="teamName"
                    name="teamName"
                    required
                    value={formData.teamName}
                    onChange={handleChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
                    placeholder="e.g., Dragons Quidditch Club"
                  />
                </div>

                <div>
                  <label htmlFor="numberOfPlayers" className="block text-sm font-medium text-gray-700 mb-1">
                    Number of Players *
                  </label>
                  <input
                    type="number"
                    id="numberOfPlayers"
                    name="numberOfPlayers"
                    required
                    min="1"
                    max="50"
                    value={formData.numberOfPlayers}
                    onChange={handleChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
                    placeholder="e.g., 21"
                  />
                </div>
              </div>

              {/* Contact Information */}
              <div>
                <h5 className="text-sm font-semibold text-gray-900 mb-3">Contact Information</h5>
                
                <div className="mb-4">
                  <label htmlFor="contactName" className="block text-sm font-medium text-gray-700 mb-1">
                    Contact Name *
                  </label>
                  <input
                    type="text"
                    id="contactName"
                    name="contactName"
                    required
                    value={formData.contactName}
                    onChange={handleChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
                    placeholder="Full name"
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                  <div>
                    <label htmlFor="contactEmail" className="block text-sm font-medium text-gray-700 mb-1">
                      Email *
                    </label>
                    <input
                      type="email"
                      id="contactEmail"
                      name="contactEmail"
                      required
                      value={formData.contactEmail}
                      onChange={handleChange}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
                      placeholder="email@example.com"
                    />
                  </div>

                  <div>
                    <label htmlFor="contactPhone" className="block text-sm font-medium text-gray-700 mb-1">
                      Phone Number *
                    </label>
                    <input
                      type="tel"
                      id="contactPhone"
                      name="contactPhone"
                      required
                      value={formData.contactPhone}
                      onChange={handleChange}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
                      placeholder="+1 234 567 8900"
                    />
                  </div>
                </div>
              </div>

              {/* Additional Notes */}
              <div>
                <label
                  htmlFor="additionalNotes"
                  className="block text-sm font-medium text-gray-700 mb-1"
                >
                  Additional Notes (Optional)
                </label>
                <textarea
                  id="additionalNotes"
                  name="additionalNotes"
                  value={formData.additionalNotes}
                  onChange={handleChange}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
                  placeholder="Any special requirements or information..."
                />
              </div>

              {/* Disclaimer */}
              <div className="bg-yellow-50 border border-yellow-200 rounded-md p-3">
                <p className="text-xs text-yellow-800">
                  <strong>Note:</strong> By registering, you confirm that your team meets all tournament requirements. 
                  The organizer will review your registration and contact you for confirmation.
                </p>
              </div>

              <div className="flex justify-end gap-3 mt-6 pt-4 border-t">
                <Button
                  type="button"
                  onClick={close}
                  className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500"
                >
                  Cancel
                </Button>
                <Button
                  type="submit"
                  disabled={isSubmitting}
                  className="px-4 py-2 text-sm font-medium text-white bg-green-600 rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isSubmitting ? "Submitting..." : "Register Team"}
                </Button>
              </div>
            </form>
          </DialogPanel>
        </div>
      </Dialog>
    );
  }
);

RegisterTournamentModal.displayName = "RegisterTournamentModal";

export default RegisterTournamentModal;
