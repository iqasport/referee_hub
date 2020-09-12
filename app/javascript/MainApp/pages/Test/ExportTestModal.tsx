import React from 'react'

import Modal, { ModalSize } from 'MainApp/components/modals/Modal/Modal'

interface ExportModalProps {
  open: boolean;
  onExport: () => void;
  onClose: () => void;
  testName: string;
}

const ExportTestModal = (props: ExportModalProps) => {
  const { open, onClose, onExport, testName } = props

  return (
    <Modal open={open} onClose={onClose} showClose={true} size={ModalSize.Large}>
      <h1 className="my-2 font-bold text-2xl">Export</h1>
      <div className="flex items-center mt-8">
        <p>{`I want to export ${testName} question data`}</p>
      </div>
      <div className="flex w-full mt-8 items-center">
        <p className="w-3/4 mr-4 text-sm text-gray-600 italic">
          We will send you an email with the attached file once your export is done.
          If you don't get the email within 30 minutes, something's up! Email tech@iqasport.org for help.
        </p>
        <button className="green-button-outline h-12" onClick={onExport}>
          Submit
        </button>
      </div>
    </Modal>
  )
}

export default ExportTestModal
