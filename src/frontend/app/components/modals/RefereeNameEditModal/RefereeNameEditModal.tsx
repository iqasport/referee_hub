import classnames from "classnames";
import React, { useEffect, useState } from "react";

import { useGetUserDataQuery, useUpdateRefereeNameMutation, useUpdateRefereeNameAdminMutation } from "../../../store/serviceApi";
import { getErrorString } from "../../../utils/errorUtils";
import Modal, { ModalProps, ModalSize } from "../Modal/Modal";

interface RefereeNameEditModalProps extends Omit<ModalProps, "size"> {
  ngbId?: string;
  userId?: string;
  isAdmin?: boolean;
}

const RefereeNameEditModal = ({ ngbId, onClose, open, showClose, userId, isAdmin }: RefereeNameEditModalProps) => {
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [validationError, setValidationError] = useState<string | null>(null);

  const { data: userData, isLoading } = useGetUserDataQuery({ userId: userId || "" }, { skip: !open || !userId });
  const [updateRefereeName, { error: ngbError, isLoading: ngbSaving }] = useUpdateRefereeNameMutation();
  const [updateRefereeNameAdmin, { error: adminError, isLoading: adminSaving }] = useUpdateRefereeNameAdminMutation();

  const error = isAdmin ? adminError : ngbError;
  const isSaving = isAdmin ? adminSaving : ngbSaving;

  useEffect(() => {
    if (!open) {
      setValidationError(null);
      return;
    }

    setFirstName(userData?.firstName || "");
    setLastName(userData?.lastName || "");
  }, [open, userData]);

  const handleSubmit = async () => {
    if (!userId) return;

    if (!firstName.trim() && !lastName.trim()) {
      setValidationError("At least one name field is required.");
      return;
    }

    setValidationError(null);

    if (isAdmin) {
      await updateRefereeNameAdmin({
        userId,
        updateRefereeNameRequest: {
          firstName: firstName.trim(),
          lastName: lastName.trim(),
        },
      }).unwrap();
    } else if (ngbId) {
      await updateRefereeName({
        ngb: ngbId,
        userId,
        updateRefereeNameRequest: {
          firstName: firstName.trim(),
          lastName: lastName.trim(),
        },
      }).unwrap();
    }
    onClose();
  };

  return (
    <Modal open={open} onClose={onClose} showClose={showClose} size={ModalSize.Small}>
      <h1 className="my-2 text-2xl font-bold">Rename referee</h1>
      <div className="mt-6 flex flex-col gap-4">
        <label>
          <span className="text-gray-700">First name</span>
          <input
            className={classnames("form-input mt-1 block w-full", {
              "border border-red-500": !!validationError && !firstName.trim() && !lastName.trim(),
            })}
            value={firstName}
            onChange={(event) => setFirstName(event.target.value)}
            disabled={isLoading || isSaving}
          />
        </label>
        <label>
          <span className="text-gray-700">Last name</span>
          <input
            className={classnames("form-input mt-1 block w-full", {
              "border border-red-500": !!validationError && !firstName.trim() && !lastName.trim(),
            })}
            value={lastName}
            onChange={(event) => setLastName(event.target.value)}
            disabled={isLoading || isSaving}
          />
        </label>
        <p className="text-red-600">{validationError || getErrorString(error)}</p>
        <div className="flex justify-end gap-3 pt-2">
          <button className="green-button-outline h-10 px-4" onClick={onClose} type="button" disabled={isSaving}>
            Cancel
          </button>
          <button
            className={classnames("uppercase h-10 px-4 rounded-lg bg-green text-white", {
              "opacity-50 cursor-default": isLoading || isSaving,
            })}
            onClick={handleSubmit}
            type="button"
            disabled={isLoading || isSaving}
          >
            Save
          </button>
        </div>
      </div>
    </Modal>
  );
};

export default RefereeNameEditModal;
