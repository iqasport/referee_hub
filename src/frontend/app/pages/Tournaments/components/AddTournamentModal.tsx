import { Button, Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle } from "react";
import React from "react";

interface Tournament {
  id?: string;
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  type: string;
  country: string;
  city: string;
  place: string;
  isPrivate: boolean;
}

interface AddTournamentModalProps {
  onSubmit: (tournament: Tournament, isEdit: boolean) => void;
}

export interface AddTournamentModalRef {
  openAdd: () => void;
  openEdit: (tournament: Tournament) => void;
}

const AddTournamentModal = forwardRef<AddTournamentModalRef, AddTournamentModalProps>(
  ({ onSubmit }, ref) => {
    const [isOpen, setIsOpen] = useState(false);
    const [mode, setMode] = useState<"add" | "edit">("add");
    const initialFormData = {
      name: "",
      description: "",
      startDate: "",
      endDate: "",
      type: "",
      country: "",
      city: "",
      place: "",
      isPrivate: false,

    };
    const [formData, setFormData] = useState<Tournament>(initialFormData);

    useImperativeHandle(ref, () => ({
      openAdd: () => {
        setMode("add");
        setFormData(initialFormData);
        setIsOpen(true);
      },
      openEdit: (tournament: Tournament) => {
        setMode("edit");
        setFormData(tournament);
        setIsOpen(true);
      },
    }));

    function close() {
      setIsOpen(false);
      setTimeout(() => setFormData(initialFormData), 300);
    }

    function handleSubmit(e: React.FormEvent) {
      e.preventDefault();
      onSubmit(formData, isEditMode);
      close();
    }

    function handleChange(
      e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
    ) {
      const { name, value, type } = e.target;
      setFormData((prev) => ({
        ...prev,
        [name]: type === "checkbox" ? (e.target as HTMLInputElement).checked : value,
      }));
    }

    const isEditMode = mode === "edit";

    return (
      <Dialog open={isOpen} as="div" className="relative z-50" onClose={close}>
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4">
          <DialogPanel className="w-full max-w-2xl rounded-xl bg-white p-6 shadow-xl">
            <DialogTitle as="h3" className="text-xl font-semibold text-gray-900 mb-4">
              {isEditMode ? "Edit Tournament" : "Add New Tournament"}
            </DialogTitle>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
                  Tournament Name *
                </label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  required
                  value={formData.name}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="e.g., Spring Championship 2025"
                />
              </div>

              <div>
                <label
                  htmlFor="description"
                  className="block text-sm font-medium text-gray-700 mb-1"
                >
                  Description
                </label>
                <textarea
                  id="description"
                  name="description"
                  value={formData.description}
                  onChange={handleChange}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Brief description of the tournament"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label
                    htmlFor="startDate"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Start Date *
                  </label>
                  <input
                    type="date"
                    id="startDate"
                    name="startDate"
                    required
                    value={formData.startDate}
                    onChange={handleChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div>
                  <label htmlFor="endDate" className="block text-sm font-medium text-gray-700 mb-1">
                    End Date *
                  </label>
                  <input
                    type="date"
                    id="endDate"
                    name="endDate"
                    required
                    value={formData.endDate}
                    onChange={handleChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>

              <div>
                <label htmlFor="type" className="block text-sm font-medium text-gray-700 mb-1">
                  Tournament Type *
                </label>
                <select
                  id="type"
                  name="type"
                  required
                  value={formData.type}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Select type...</option>
                  <option value="Championship">Championship</option>
                  <option value="Finals">Finals</option>
                  <option value="YouthCup">Youth Cup</option>
                  <option value="Invitational">Invitational</option>
                  <option value="Qualifiers">Qualifiers</option>
                  <option value="Exhibition">Exhibition</option>
                  <option value="Charity">Charity</option>
                  <option value="Classic">Classic</option>
                </select>
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label htmlFor="country" className="block text-sm font-medium text-gray-700 mb-1">
                    Country *
                  </label>
                  <input
                    type="text"
                    id="country"
                    name="country"
                    required
                    value={formData.country}
                    onChange={handleChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="e.g., Belgium"
                  />
                </div>

                <div>
                  <label htmlFor="city" className="block text-sm font-medium text-gray-700 mb-1">
                    City *
                  </label>
                  <input
                    type="text"
                    id="city"
                    name="city"
                    required
                    value={formData.city}
                    onChange={handleChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="e.g., Brussels"
                  />
                </div>

                <div>
                  <label htmlFor="place" className="block text-sm font-medium text-gray-700 mb-1">
                    Venue *
                  </label>
                  <input
                    type="text"
                    id="place"
                    name="place"
                    required
                    value={formData.place}
                    onChange={handleChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="e.g., Central Arena"
                  />
                </div>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="isPrivate"
                  name="isPrivate"
                  checked={formData.isPrivate}
                  onChange={handleChange}
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <label htmlFor="isPrivate" className="ml-2 block text-sm text-gray-700">
                  Private Tournament
                </label>
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
                  
                >
                  {isEditMode ? "Update Tournament" : "Create Tournament"}
                </Button>
              </div>
            </form>
          </DialogPanel>
        </div>
      </Dialog>
    );
  }
);

AddTournamentModal.displayName = "AddTournamentModal";

export default AddTournamentModal;