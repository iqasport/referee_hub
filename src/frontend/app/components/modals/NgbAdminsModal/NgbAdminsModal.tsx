import React, { useState } from "react";

import Modal, { ModalSize } from "../Modal/Modal";
import { useAddNgbAdminMutation, useDeleteNgbAdminMutation } from "../../../store/serviceApi";
import { getErrorString } from "../../../utils/errorUtils";

interface NgbAdminsModalProps {
  open: boolean;
  onClose: () => void;
  showClose: boolean;
  ngbId: string;
}

const NgbAdminsModal = (props: NgbAdminsModalProps) => {
  const { ngbId } = props;

  const [addEmail, setAddEmail] = useState("");
  const [createIfNotExists, setCreateIfNotExists] = useState(false);
  const [deleteEmail, setDeleteEmail] = useState("");

  const [addAdmin, {error: addError, data: addData }] = useAddNgbAdminMutation();
  const [deleteAdmin, {error: deleteError, isSuccess: isDeleted }] = useDeleteNgbAdminMutation();

  return (
    <Modal {...props} size={ModalSize.Large}>
      <h1 className="my-2 font-bold text-2xl">Manage admins</h1>
      <div className="flex flex-col mt-8 w-1/2 mx-auto">
        <h2 className="my-2 font-bold">Add admin</h2>
        <input className="block mx-2 border-b border-gray-400" type="email" placeholder="Email" value={addEmail} onChange={e => setAddEmail(e.target.value)} />
        <label htmlFor="createIfNotExists">
          <input className="mx-2 mt-2" type="checkbox" checked={createIfNotExists} onChange={e => setCreateIfNotExists(e.target.checked)}/>
          Create user if they don&apos;t have an account already.
        </label>
        <p className="text-red-600">{getErrorString(addError)}</p>
        <p className="text-green">{addData}</p>
        <button className="green-button-outline h-10 mt-4" onClick={() => addAdmin({ngb: ngbId, ngbAdminCreationModel: {email: addEmail, createAccountIfNotExists: createIfNotExists}})}>Add admin</button>
      </div>
      <div className="flex flex-col mt-8 w-1/2 mx-auto">
        <h2 className="my-2 font-bold">Delete admin</h2>
        <input className="block mx-2 border-b border-gray-400" type="email" placeholder="Email" value={deleteEmail} onChange={e => setDeleteEmail(e.target.value)} />
        <p className="text-red-600">{getErrorString(deleteError)}</p>
        <p className="text-green">{isDeleted ? "Deleted successfully": ""}</p>
        <button className="green-button-outline h-10 mt-4" onClick={() => deleteAdmin({ngb: ngbId, email: deleteEmail})}>Delete admin</button>
      </div>
    </Modal>
  );
};

export default NgbAdminsModal;
