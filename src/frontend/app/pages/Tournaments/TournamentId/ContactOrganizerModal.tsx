import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle } from "react";
import React from "react";
import { HomeIcon } from "../../../components/icons";
import { useContactTournamentOrganizersMutation } from "../../../store/serviceApi";
import { useAlert } from "../../../hooks/useAlert";

interface OrganizerInfo {
  name: string;
  tournamentName: string;
  tournamentId: string;
}

export interface ContactOrganizerModalRef {
  open: (organizer: OrganizerInfo) => void;
}

const ContactOrganizerModal = forwardRef<ContactOrganizerModalRef>((_props, ref) => {
  const [isOpen, setIsOpen] = useState(false);
  const [organizer, setOrganizer] = useState<OrganizerInfo | null>(null);
  const [message, setMessage] = useState("");
  const [contactOrganizers, { isLoading }] = useContactTournamentOrganizersMutation();
  const { showAlert } = useAlert();

  useImperativeHandle(ref, () => ({
    open: (organizerData: OrganizerInfo) => {
      setOrganizer(organizerData);
      setMessage("");
      setIsOpen(true);
    },
  }));

  function close() {
    setIsOpen(false);
    setMessage("");
  }

  async function handleSend() {
    if (!organizer || !message.trim()) {
      showAlert("Please enter a message", "error");
      return;
    }

    try {
      await contactOrganizers({
        tournamentId: organizer.tournamentId,
        contactTournamentRequest: {
          message: message.trim(),
        },
      }).unwrap();

      showAlert("Message sent successfully to tournament organizers", "success");
      close();
    } catch (error) {
      console.error("Failed to send message:", error);
      showAlert("Failed to send message. Please try again.", "error");
    }
  }

  return (
    <Dialog open={isOpen && !!organizer} as="div" className="relative z-50" onClose={close}>
      <div
        className="fixed inset-0"
        style={{ backgroundColor: "rgba(0, 0, 0, 0.3)" }}
        aria-hidden="true"
      />
      <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
        <DialogPanel className="relative w-full max-w-md rounded-lg bg-white p-6 shadow-xl">
          <DialogTitle as="h3" className="text-xl font-semibold text-gray-900 mb-4">
            Contact Organizer
          </DialogTitle>

          {/* Tournament Name */}
          <p className="text-sm text-gray-600 mb-6">
            For questions about{" "}
            <span className="font-medium text-gray-800">{organizer?.tournamentName}</span>
          </p>

          {/* Organizer Info Card */}
          <div className="bg-gray-50 rounded-lg border border-gray-200 p-4 mb-6">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center">
                <HomeIcon className="w-6 h-6 text-green-600" />
              </div>
              <div>
                <p className="text-xs text-gray-500 uppercase tracking-wide font-semibold mb-1">
                  Organizer
                </p>
                <p className="text-lg font-semibold text-gray-900">
                  {organizer?.name || "Not specified"}
                </p>
              </div>
            </div>
          </div>

          {/* Contact Instructions */}
          <div className="text-sm text-gray-600 space-y-2 mb-6">
            <p>
              Send a message to the tournament organizers. They will receive your message via email
              and can respond to you directly.
            </p>
          </div>

          {/* Message Input */}
          <div className="mb-6">
            <label htmlFor="message" className="block text-sm font-medium text-gray-700 mb-2">
              Your Message <span className="text-red-500">*</span>
            </label>
            <textarea
              id="message"
              rows={4}
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent resize-none"
              placeholder="Type your message here..."
              maxLength={1000}
            />
            <p className="text-xs text-gray-500 mt-1">
              {message.length}/1000 characters
            </p>
          </div>

          {/* Action Buttons */}
          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
            <button
              type="button"
              onClick={close}
              className="btn btn-secondary"
              disabled={isLoading}
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={handleSend}
              className="btn btn-primary"
              disabled={isLoading || !message.trim()}
            >
              {isLoading ? "Sending..." : "Send Message"}
            </button>
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
});

ContactOrganizerModal.displayName = "ContactOrganizerModal";

export default ContactOrganizerModal;
