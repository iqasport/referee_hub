import React, { useState } from "react";

import Modal, { ModalSize } from "../Modal/Modal";
import { useAddTeamManagerMutation, useDeleteTeamManagerMutation } from "../../../store/serviceApi";
import { getErrorString } from "../../../utils/errorUtils";

interface TeamManagersModalProps {
  open: boolean;
  onClose: () => void;
  showClose: boolean;
  ngbId: string;
  teamId: string;
}

const TeamManagersModal = (props: TeamManagersModalProps) => {
  const { ngbId, teamId } = props;

  const [addEmail, setAddEmail] = useState("");
  const [createIfNotExists, setCreateIfNotExists] = useState(false);
  const [deleteEmail, setDeleteEmail] = useState("");

  const [addManager, {error: addError, data: addData }] = useAddTeamManagerMutation();
  const [deleteManager, {error: deleteError, isSuccess: isDeleted }] = useDeleteTeamManagerMutation();

  return (
    <Modal {...props} size={ModalSize.Large}>
      <h1 className="my-2 font-bold text-2xl">Manage team managers</h1>
      <div className="flex flex-col mt-8 w-1/2 mx-auto">
        <h2 className="my-2 font-bold">Add manager</h2>
        <input className="block mx-2 border-b border-gray-400" type="email" placeholder="Email" value={addEmail} onChange={e => setAddEmail(e.target.value)} />
        <label htmlFor="createIfNotExists">
          <input id="createIfNotExists" className="mx-2 mt-2" type="checkbox" checked={createIfNotExists} onChange={e => setCreateIfNotExists(e.target.checked)}/>
          Create user if they don&apos;t have an account already.
        </label>
        <p className="text-red-600">{getErrorString(addError)}</p>
        <p className="text-green">{addData}</p>
        <button className="green-button-outline h-10 mt-4" onClick={() => addManager({ngb: ngbId, teamId: teamId, teamManagerCreationModel: {email: addEmail, createAccountIfNotExists: createIfNotExists}})}>Add manager</button>
      </div>
      <div className="flex flex-col mt-8 w-1/2 mx-auto">
        <h2 className="my-2 font-bold">Delete manager</h2>
        <input className="block mx-2 border-b border-gray-400" type="email" placeholder="Email" value={deleteEmail} onChange={e => setDeleteEmail(e.target.value)} />
        <p className="text-red-600">{getErrorString(deleteError)}</p>
        <p className="text-green">{isDeleted ? "Deleted successfully" : ""}</p>
        <button className="green-button-outline h-10 mt-4" onClick={() => deleteManager({ngb: ngbId, teamId: teamId, email: deleteEmail})}>Delete manager</button>
      </div>
    </Modal>
  );
};

export default TeamManagersModal;
