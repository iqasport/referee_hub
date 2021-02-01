import React, { useState } from "react";

import Modal, { ModalSize } from "../Modal/Modal";

export enum ExportType {
  Team = "team",
  Referee = "referee",
}

interface ExportModalProps {
  open: boolean;
  onExport: (type: string) => void;
  onClose: () => void;
}

const ExportModal = (props: ExportModalProps) => {
  const { open, onClose, onExport } = props;
  const [selectedExport, setSelectedExport] = useState<string>(ExportType.Team);
  const handleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const exportValue = event.target.value;
    setSelectedExport(exportValue);
  };
  const handleExport = () => {
    onExport(selectedExport);
  };

  return (
    <Modal open={open} onClose={onClose} showClose={true} size={ModalSize.Large}>
      <h1 className="my-2 font-bold text-2xl">Export</h1>
      <div className="flex items-center mt-8">
        <p>I want to export </p>
        <select className="block mx-2 border-b border-gray-400" onChange={handleChange}>
          <option key="team" value={ExportType.Team}>
            team
          </option>
          <option key="referee" value={ExportType.Referee}>
            referee
          </option>
        </select>
        <p> data</p>
      </div>
      <div className="flex w-full mt-8 items-center">
        <p className="w-3/4 mr-4 text-sm text-gray-600 italic">
          We will send you an email with the attached file once your export is done. If you don't
          get the email within 30 minutes, something's up! Email tech@iqasport.org for help.
        </p>
        <button className="green-button-outline h-12" onClick={handleExport}>
          Submit
        </button>
      </div>
    </Modal>
  );
};

export default ExportModal;
