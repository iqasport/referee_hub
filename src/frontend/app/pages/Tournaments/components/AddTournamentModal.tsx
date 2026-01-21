import { Button, Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle } from "react";
import React from "react";
import {
  useCreateTournamentMutation,
  useUpdateTournamentMutation,
} from "../../../store/serviceApi";
import type { TournamentType } from "../../../store/serviceApi";
import UploadedImage from "../../../components/UploadedImage";
import CustomAlert from "../../../components/CustomAlert";
import { useAlert } from "../../../hooks/useAlert";

interface Tournament {
  id?: string;
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  type: TournamentType | "";
  country: string;
  city: string;
  place: string;
  organizer?: string;
  isPrivate: boolean;
  bannerImageUrl?: string;
}

export interface AddTournamentModalRef {
  openAdd: () => void;
  openEdit: (tournament: Tournament) => void;
}

const AddTournamentModal = forwardRef<AddTournamentModalRef>((_props, ref) => {
  const { alertState, showAlert, hideAlert } = useAlert();
  const [isOpen, setIsOpen] = useState(false);
  const [mode, setMode] = useState<"add" | "edit">("add");
  const [pendingBannerFile, setPendingBannerFile] = useState<File | null>(null);
  const [createTournament, { isLoading: isCreating }] = useCreateTournamentMutation();
  const [updateTournament, { isLoading: isUpdating }] = useUpdateTournamentMutation();
  const initialFormData: Tournament = {
    name: "",
    description: "",
    startDate: "",
    endDate: "",
    type: "",
    country: "",
    city: "",
    place: "",
    organizer: "",
    isPrivate: false,
    bannerImageUrl: "",
  };
  const [formData, setFormData] = useState<Tournament>(initialFormData);

  useImperativeHandle(ref, () => ({
    openAdd: () => {
      setFormData(initialFormData);
      setPendingBannerFile(null);
      setMode("add");
      setIsOpen(true);
    },
    openEdit: (tournament: Tournament) => {
      setFormData(tournament);
      setPendingBannerFile(null);
      setMode("edit");
      setIsOpen(true);
    },
  }));

  function close() {
    setIsOpen(false);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    try {
      if (isEditMode) {
        if (!formData.id) {
          showAlert("Tournament ID is missing", "error");
          return;
        }
        await updateTournament({
          tournamentId: formData.id,
          tournamentModel: {
            name: formData.name,
            description: formData.description,
            startDate: formData.startDate,
            endDate: formData.endDate,
            type: formData.type || undefined,
            country: formData.country,
            city: formData.city,
            place: formData.place,
            organizer: formData.organizer,
            isPrivate: formData.isPrivate,
          },
        }).unwrap();
      } else {
        const result = await createTournament({
          tournamentModel: {
            name: formData.name,
            description: formData.description,
            startDate: formData.startDate,
            endDate: formData.endDate,
            type: formData.type || undefined,
            country: formData.country,
            city: formData.city,
            place: formData.place,
            organizer: formData.organizer,
            isPrivate: formData.isPrivate,
          },
        }).unwrap();

        // Upload banner image if one was selected during creation
        if (pendingBannerFile && result.id) {
          try {
            const payload = new FormData();
            payload.append("bannerBlob", pendingBannerFile);
            await fetch(`/api/v2/Tournaments/${result.id}/banner`, {
              method: "PUT",
              body: payload,
            });
          } catch (bannerError) {
            console.error("Failed to upload banner:", bannerError);
            // Don't fail the whole operation, tournament was created successfully
          }
        }
      }
      close();
    } catch (error) {
      console.error("Failed to save tournament:", error);
      showAlert("Failed to save tournament. Please try again.", "error");
    }
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

  async function handleBannerUpload(file: File) {
    if (!formData.id) {
      // In create mode, store the file to upload after tournament creation
      setPendingBannerFile(file);
      // Update preview with temporary URL
      setFormData((prev) => ({
        ...prev,
        bannerImageUrl: URL.createObjectURL(file),
      }));
      return;
    }
    try {
      // RTK Query code gen doesn't support multipart form requests, so use native fetch
      const payload = new FormData();
      payload.append("bannerBlob", file);
      const response = await fetch(`/api/v2/Tournaments/${formData.id}/banner`, {
        method: "PUT",
        body: payload,
      });
      if (!response.ok) {
        throw new Error(`Upload failed: ${response.statusText}`);
      }
      // Update local state with a temporary URL for preview
      setFormData((prev) => ({
        ...prev,
        bannerImageUrl: URL.createObjectURL(file),
      }));
    } catch (error) {
      console.error("Failed to upload banner:", error);
      showAlert("Failed to upload banner image. Please try again.", "error");
    }
  }

  const isEditMode = mode === "edit";

  return (
    <>
      {alertState.isVisible && (
        <CustomAlert message={alertState.message} type={alertState.type} onClose={hideAlert} />
      )}
      <Dialog open={isOpen} as="div" className="relative z-50" onClose={close}>
        <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
        <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
          <DialogPanel className="w-full max-w-2xl rounded-xl bg-white p-6 shadow-xl my-8 max-h-full overflow-y-auto">
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

              {/* Banner Image Upload */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Banner Image</label>
                <UploadedImage
                  imageUrl={formData.bannerImageUrl || ""}
                  imageAlt="Tournament banner"
                  onSubmit={handleBannerUpload}
                  isEditable={true}
                />
                <p className="mt-1 text-xs text-gray-500">
                  Click the + icon to upload a banner image for the tournament.
                </p>
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
                  <option value="Club">Club</option>
                  <option value="National">National</option>
                  <option value="Youth">Youth</option>
                  <option value="Fantasy">Fantasy</option>
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

              <div>
                <label htmlFor="organizer" className="block text-sm font-medium text-gray-700 mb-1">
                  Organizer
                </label>
                <input
                  type="text"
                  id="organizer"
                  name="organizer"
                  value={formData.organizer || ""}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="e.g., Regional Sports Association"
                />
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
                <Button type="submit" disabled={isCreating || isUpdating}>
                  {isCreating || isUpdating
                    ? "Saving..."
                    : isEditMode
                    ? "Update Tournament"
                    : "Create Tournament"}
                </Button>
              </div>
            </form>
          </DialogPanel>
        </div>
      </Dialog>
    </>
  );
});

AddTournamentModal.displayName = "AddTournamentModal";

export default AddTournamentModal;
