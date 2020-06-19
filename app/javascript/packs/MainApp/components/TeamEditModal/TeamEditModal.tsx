import { capitalize } from 'lodash';
import React, { useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';

import { UpdateTeamRequest } from '../../apis/team';
import Modal, { ModalProps, ModalSize } from '../../components/Modal/Modal';
import MultiInput from '../../components/MultiInput';
import { createTeam } from '../../modules/team/team';
import { DataAttributes } from '../../schemas/getTeamSchema';

const STATUS_OPTIONS = ['competitive', 'developing', 'inactive']
const TYPE_OPTIONS = ['community', 'university', 'youth']
const initialNewTeam: UpdateTeamRequest = {
  city: '',
  country: '',
  groupAffiliation: null,
  name: '',
  state: '',
  status: null,
  urls: [],
}

interface TeamEditModalProps extends Omit<ModalProps, 'size'> {
  team?: DataAttributes;
}

const TeamEditModal = (props: TeamEditModalProps) => {
  const { team, onClose } = props
  const [newTeam, setNewTeam] = useState<UpdateTeamRequest>(initialNewTeam)
  const dispatch = useDispatch()
  const formType = team ? 'Edit' : 'New'

  const handleSubmit = () => {
    dispatch(createTeam(newTeam))
    onClose()
  }

  const handleDataChange = (dataKey: string, newValue: string) => setNewTeam({ ...newTeam, [dataKey]: newValue })

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = event.target

    handleDataChange(name, value)
  }

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
          <input className="form-input mt-1 block w-full" placeholder="University Quidditch Team" name="name" onChange={handleInputChange} />
        </label>
        <div className="flex w-full my-8">
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">City</span>
            <input 
              className="form-input mt-1 block w-full" 
              placeholder="Los Angeles"
              name="city" 
              onChange={handleInputChange}
            />
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">State/Provence</span>
            <input 
              className="form-input mt-1 block w-full" 
              placeholder="California" 
              name="state" 
              onChange={handleInputChange} 
            />
          </label>
          <label className="w-1/3">
            <span className="text-gray-700">Country</span>
            <input 
              className="form-input mt-1 block w-full" 
              placeholder="United States" 
              name="country" 
              onChange={handleInputChange}
            />
          </label>
        </div>
        <div className="flex w-full">
          <label className="w-1/2 mr-4">
            <span className="text-gray-700">Type</span>
            <select 
              className="form-select block w-full mt-1" 
              placeholder="Select the age group"
              name="groupAffiliation" 
              onChange={handleInputChange}
            >
              <option value="" />
              {TYPE_OPTIONS.map(renderOption)}
            </select>
          </label>
          <label className="w-1/2">
            <span className="text-gray-700">Status</span>
            <select
              className="form-select block w-full mt-1" 
              placeholder="Select the playing status" 
              name="status" 
              onChange={handleInputChange}
            >
              <option value="" />
              {STATUS_OPTIONS.map(renderOption)}
            </select>
          </label>
        </div>
        <div className="w-full my-8">
          <label>
            <span className="text-gray-700">Social Media</span>
            <MultiInput />
          </label>
        </div>
        <button onClick={handleSubmit}>Done</button>
      </form>
    </Modal>
  )
}

export default TeamEditModal
