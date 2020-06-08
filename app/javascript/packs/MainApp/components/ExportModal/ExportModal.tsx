import React, { useState } from 'react'

import Modal, { ModalSize } from '../Modal/Modal'

export enum ExportType {
  Team = 'team',
  Referee = 'referee',
}

interface ExportModalProps {
  open: boolean;
  onExport: (type: ExportType) => void;
  onClose: () => void;
}

const ExportModal = (props: ExportModalProps) => {
  const { open, onClose, onExport } = props
  const [selectedExport, setSelectedExport] = useState<string>() 
  const handleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const exportValue = event.target.value
    setSelectedExport(exportValue);
  }
  const handleExport = () => {
    onExport(ExportType[selectedExport])
  }

  return (
    <Modal open={open} onClose={onClose} showClose={true} size={ModalSize.Medium}>
      <h1 className="my-2 font-bold text-2xl">Export</h1>
      <select className="form-select block mt-1" onChange={handleChange}>
        <option key="team" value={ExportType.Team}>team</option>
        <option key="referee" value={ExportType.Referee}>referee</option>
      </select>
      <button onClick={handleExport}>
        Submit
      </button>
    </Modal>
  )
}

export default ExportModal;
