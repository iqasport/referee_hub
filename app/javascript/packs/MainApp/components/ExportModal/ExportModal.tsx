import React from 'react'

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
  const { open, onClose } = props

  return (
    <Modal open={open} onClose={onClose} showClose={true} size={ModalSize.Medium}>
      <h1 className="my-2 font-bold text-2xl">Export Data</h1>
    </Modal>
  )
}

export default ExportModal;
