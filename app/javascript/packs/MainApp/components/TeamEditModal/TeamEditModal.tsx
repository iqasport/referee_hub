import { capitalize } from 'lodash';
import React, { useEffect, useState } from 'react';
import { shallowEqual, useDispatch, useSelector } from 'react-redux';

import { RootState } from 'rootReducer';
import { UpdateTeamRequest } from '../../apis/team';
import Modal, { ModalProps, ModalSize } from '../../components/Modal/Modal';
import MultiInput from '../../components/MultiInput';
import { createTeam, getTeam, updateTeam } from '../../modules/team/team';

const STATUS_OPTIONS = ['competitive', 'developing', 'inactive']
const TYPE_OPTIONS = ['community', 'university', 'youth']
const initialNewTeam: UpdateTeamRequest = {
  city: '',
  country: '',
  groupAffiliation: null,
  name: '',
  state: '',
  status: null,
  urls: null,
}

interface TeamEditModalProps extends Omit<ModalProps, 'size'> {
  teamId?: string;
}

const TeamEditModal = (props: TeamEditModalProps) => {
  const { teamId, onClose } = props

  const [newTeam, setNewTeam] = useState<UpdateTeamRequest>(initialNewTeam)
  const [urls, setNewUrls] = useState<string[]>()
  const { team, socialAccounts } = useSelector((state: RootState) => state.team, shallowEqual)
  const dispatch = useDispatch()

  const formType = teamId ? 'Edit' : 'New'
  
  useEffect(() => {
    if (!team && teamId) {
      dispatch(getTeam(teamId))
    }
  }, [teamId, dispatch])
  
  useEffect(() => {
    if (team) {
      const existingUrls = socialAccounts.length ? socialAccounts.map((account) => account.url) : []
      setNewTeam({ ...team, urls: existingUrls })
    }
  }, [team, socialAccounts])

  const handleSubmit = () => {
    const teamToSend = { ...newTeam, urls }
    if (teamId) {
      dispatch(updateTeam(teamId, teamToSend))
    } else {
      dispatch(createTeam(teamToSend))
    }
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
          <input 
            className="form-input mt-1 block w-full" 
            placeholder="University Quidditch Team" 
            name="name" 
            onChange={handleInputChange}
            value={newTeam.name}
          />
        </label>
        <div className="flex w-full my-8">
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">City</span>
            <input 
              className="form-input mt-1 block w-full" 
              placeholder="Los Angeles"
              name="city" 
              onChange={handleInputChange}
              value={newTeam.city}
            />
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">State/Provence</span>
            <input 
              className="form-input mt-1 block w-full" 
              placeholder="California" 
              name="state" 
              onChange={handleInputChange}
              value={newTeam.state}
            />
          </label>
          <label className="w-1/3">
            <span className="text-gray-700">Country</span>
            <input 
              className="form-input mt-1 block w-full" 
              placeholder="United States" 
              name="country" 
              onChange={handleInputChange}
              value={newTeam.country}
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
              value={newTeam.groupAffiliation || ''}
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
              value={newTeam.status || ''}
            >
              <option value="" />
              {STATUS_OPTIONS.map(renderOption)}
            </select>
          </label>
        </div>
        <div className="w-full my-8">
          <label>
            <span className="text-gray-700">Social Media</span>
            {newTeam.urls !== null && <MultiInput onChange={setNewUrls} values={newTeam.urls} />}
          </label>
        </div>
        <button onClick={handleSubmit}>Done</button>
      </form>
    </Modal>
  )
}

export default TeamEditModal
