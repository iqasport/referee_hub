import { capitalize } from 'lodash';
import React, { useState } from 'react';

import Modal, { ModalProps, ModalSize } from '../../components/Modal/Modal';
import MultiInput from '../../components/MultiInput';
import { Datum } from '../../schemas/getTeamsSchema';

const statusOptions = ['competitive', 'developing', 'inactive']
const typeOptions = ['community', 'university', 'youth']

interface TeamEditModalProps extends Omit<ModalProps, 'size'> {
  team?: Datum;
  // onChange: (dataKey: string, value: string) => void;
}

const TeamEditModal = (props: TeamEditModalProps) => {
  const { team } = props
  const formType = team ? 'Edit' : 'New'

  const renderOption = (value: string) => {
    return (
      <option key={value} value={value}>
        {capitalize(value)}
      </option>
    )
  }

  return (
    <Modal {...props} size={ModalSize.Large}>
      <h2 className="text-center text-xl font-semibold my-8">{`${formType} Team`}</h2>
      <form>
        <label className="block">
          <span className="text-gray-700">Name</span>
          <input className="form-input mt-1 block w-full" placeholder="University Quidditch Team" />
        </label>
        <div className="flex w-full my-8">
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">City</span>
            <input className="form-input mt-1 block w-full" placeholder="Los Angeles" />
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">State/Provence</span>
            <input className="form-input mt-1 block w-full" placeholder="California" />
          </label>
          <label className="w-1/3">
            <span className="text-gray-700">Country</span>
            <input className="form-input mt-1 block w-full" placeholder="United States" />
          </label>
        </div>
        <div className="flex w-full">
          <label className="w-1/2 mr-4">
            <span className="text-gray-700">Type</span>
            <select className="form-select block w-full mt-1" placeholder="Select the age group">
              {typeOptions.map(renderOption)}
            </select>
          </label>
          <label className="w-1/2">
            <span className="text-gray-700">Status</span>
            <select className="form-select block w-full mt-1" placeholder="Select the playing status">
              {statusOptions.map(renderOption)}
            </select>
          </label>
        </div>
        <div className="w-full my-8">
          <label>
            <span className="text-gray-700">Social Media</span>
            <MultiInput />
          </label>
        </div>
      </form>
    </Modal>
  )
}

export default TeamEditModal
