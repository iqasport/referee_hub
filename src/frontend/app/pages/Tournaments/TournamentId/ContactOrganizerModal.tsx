import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { useState, forwardRef, useImperativeHandle } from "react";
import React from "react";
import { HomeIcon } from "../../../components/icons";

interface OrganizerInfo {
  name: string;
  tournamentName: string;
}

export interface ContactOrganizerModalRef {
  open: (organizer: OrganizerInfo) => void;
}

const ContactOrganizerModal = forwardRef<ContactOrganizerModalRef>((_props, ref) => {
  const [isOpen, setIsOpen] = useState(false);
  const [organizer, setOrganizer] = useState<OrganizerInfo | null>(null);

  useImperativeHandle(ref, () => ({
    open: (organizerData: OrganizerInfo) => {
      setOrganizer(organizerData);
      setIsOpen(true);
    },
  }));

  function close() {
    setIsOpen(false);
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
              To get in touch with the tournament organizer, please reach out through your National
              Governing Body (NGB) or check the official tournament communications.
            </p>
          </div>

          {/* Message Input */}
          <div className="mb-6">
            <label htmlFor="message" className="block text-sm font-medium text-gray-700 mb-2">
              Your Message
            </label>
            <textarea
              id="message"
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent resize-none"
              placeholder="Type your message here..."
            />
          </div>

          {/* Close Button */}
          <div className="flex justify-end pt-4 border-t border-gray-200">
            <button type="button" onClick={close} className="btn btn-primary">
              Close
            </button>
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
});

ContactOrganizerModal.displayName = "ContactOrganizerModal";

export default ContactOrganizerModal;
