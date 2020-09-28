import { capitalize } from 'lodash'
import React from 'react'

import Modal, { ModalSize } from '../Modal/Modal'

interface WarningModalProps {
  action: string;
  dataType: string;
  onCancel: () => void;
  onConfirm: () => void;
  open: boolean;
}

const WarningModal = (props: WarningModalProps) => {
  const { action, dataType, onCancel, onConfirm, open } = props

  return (
    <Modal size={ModalSize.Small} showClose={false} open={open}>
      <h3 className="text-2xl font-bold">{`${capitalize(action)}?`}</h3>
      <p className="my-8">
        Are you sure you want to {` ${action} this ${dataType}? `} This action cannot be undone.
      </p>
      <div className="flex justify-end">
        <button className="border border-navy-blue text-navy-blue py-2 px-4 uppercase mx-4 rounded" onClick={onCancel}>Cancel</button>
        <button className="bg-red-500 text-white uppercase py-2 px-4 rounded" onClick={onConfirm}>Confirm</button>
      </div>
    </Modal>
  )
}

export default WarningModal
