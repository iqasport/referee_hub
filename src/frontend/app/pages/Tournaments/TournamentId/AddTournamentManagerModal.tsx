import React, { useState } from "react";

import { useAddTournamentManagerMutation } from "../../../store/serviceApi";

interface AddTournamentManagerModalProps {
  tournamentId: string;
  onClose: () => void;
}

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const SUCCESS_MESSAGE_DURATION = 2000;

const AddTournamentManagerModal = ({ tournamentId, onClose }: AddTournamentManagerModalProps) => {
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [addTournamentManager, { isLoading }] = useAddTournamentManagerMutation();

  const handleAdd = async () => {
    setError("");
    setSuccess("");

    if (!emailRegex.test(email)) {
      setError("Please enter a valid email address");
      return;
    }

    try {
      await addTournamentManager({
        tournamentId,
        addTournamentManagerModel: { email },
      }).unwrap();
      setSuccess("Manager added successfully!");
      setEmail("");
      setTimeout(() => {
        onClose();
      }, SUCCESS_MESSAGE_DURATION);
    } catch (err: any) {
      const errorMessage =
        err?.data?.error || err?.data || "Failed to add manager. Please try again.";
      setError(errorMessage);
    }
  };

  const handleCancel = () => {
    setEmail("");
    setError("");
    setSuccess("");
    onClose();
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <h3 className="text-xl font-bold mb-4">Add Tournament Manager</h3>
        <div className="mb-4">
          <label className="block text-sm font-medium mb-2">Email Address</label>
          <input
            type="email"
            className="w-full px-3 py-2 border border-gray-300 rounded"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="manager@example.com"
          />
        </div>
        {error && <div className="mb-4 text-red-600 text-sm">{error}</div>}
        {success && <div className="mb-4 text-green-600 text-sm">{success}</div>}
        <div className="flex justify-end space-x-3">
          <button
            className="px-4 py-2 bg-gray-300 rounded hover:bg-gray-400"
            onClick={handleCancel}
            disabled={isLoading}
          >
            Cancel
          </button>
          <button
            className="px-4 py-2 bg-green text-white rounded hover:bg-green-700 disabled:opacity-50"
            onClick={handleAdd}
            disabled={isLoading || !email}
          >
            {isLoading ? "Adding..." : "Add Manager"}
          </button>
        </div>
      </div>
    </div>
  );
};

export default AddTournamentManagerModal;
