import classnames from 'classnames'
import { capitalize, words } from 'lodash';
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux';

import { UpdateNgbRequest } from 'MainApp/apis/nationalGoverningBody';
import { getNationalGoverningBody, updateNgb } from 'MainApp/modules/nationalGoverningBody/nationalGoverningBody';
import { RootState } from 'MainApp/rootReducer';

import Modal, { ModalProps, ModalSize } from '../Modal/Modal';
import MultiInput from '../MultiInput';

const REQUIRED_FIELDS = ['name', 'region']
const REGION_OPTIONS = ['north_america', 'south_america', 'europe', 'africa', 'asia']
const initialNewNgb: UpdateNgbRequest = {
  acronym: '',
  country: '',
  name: '',
  playerCount: 0,
  region: null,
  urls: [],
  website: '',
}

const validateInput = (ngb: UpdateNgbRequest): string[] => {
  return Object.keys(ngb).filter((dataKey: string) => {
    if (REQUIRED_FIELDS.includes(dataKey) && !ngb[dataKey]) {
      return true
    }

    return false
  })
}

interface NgbEditModalProps extends Omit<ModalProps, 'size'> {
  ngbId?: number;
}

const NgbEditModal = (props: NgbEditModalProps) => {
  const { ngbId, onClose } = props

  const [errors, setErrors] = useState<string[]>()
  const [hasChangedNgb, setHasChangedNgb] = useState(false)
  const [newNgb, setNewNgb] = useState<UpdateNgbRequest>(initialNewNgb)
  const [urls, setUrls] = useState<string[]>()
  const { ngb, socialAccounts } = useSelector((state: RootState) => state.nationalGoverningBody, shallowEqual)
  const { roles } = useSelector((state: RootState) => state.currentUser, shallowEqual)
  const dispatch = useDispatch()

  const formType = ngbId ? 'Edit' : 'New'
  const hasError = (dataKey: string): boolean => errors?.includes(dataKey)
  const isIqaAdmin = roles.includes('iqa_admin')

  useEffect(() => {
    if (ngbId) {
      dispatch(getNationalGoverningBody(ngbId))
    }
  }, [ngbId, dispatch])

  useEffect(() => {
    if (ngb) {
      const existingUrls = socialAccounts.length ? socialAccounts.map((account) => account.url) : []
      setNewNgb({ ...ngb, urls: existingUrls })
    }
  }, [ngb, socialAccounts])

  const handleSubmit = () => {
    const validationErrors = validateInput(newNgb)
    if (validationErrors.length) {
      setErrors(validationErrors)
      return null
    }

    const ngbToSend = { ...newNgb, urls }
    if (ngbId) {
      dispatch(updateNgb(ngbId, ngbToSend))
    }

    setHasChangedNgb(false)
    onClose()
  }

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = event.target
    let newValue: string | number = value

    if (!hasChangedNgb) setHasChangedNgb(true)
    if (name === 'playerCount') newValue = parseInt(value, 10)

    setNewNgb({ ...newNgb, [name]: value })
  }

  const handleUrlChange = (newUrls: string[]) => {
    if (!hasChangedNgb) setHasChangedNgb(true)
    setUrls(newUrls);
  }

  const renderOption = (value: string) => {
    return (
      <option key={value} value={value}>
        {words(value).map(word => capitalize(word)).join(' ')}
      </option>
    )
  }

  return (
    <Modal {...props} size={ModalSize.Large}>
      <h2 className="text-center text-xl font-semibold my-8">{`${formType} National Governing Body`}</h2>
      <form>
        <label className="block">
          <span className="text-gray-700">Name</span>
          <input
            className={classnames("form-input mt-1 block w-full", {'border border-red-500': hasError('name')})}
            placeholder="US Quidditch"
            name="name"
            onChange={handleInputChange}
            value={newNgb.name}
          />
          {hasError('name') && <span className="text-red-500">Name cannot be blank</span>}
        </label>
        <label className="block my-8">
          <span className="text-gray-700">Country</span>
          <input
            className="form-input mt-1 block w-full"
            placeholder="United States"
            name="country"
            onChange={handleInputChange}
            value={newNgb.country}
          />
        </label>
        <div className="flex w-full my-8">
          <label className="w-1/2 mr-4">
            <span className="text-gray-700">Type</span>
            <select
              disabled={!isIqaAdmin}
              className={
                classnames(
                  "form-select mt-1 block w-full",
                  { 'border border-red-500': hasError('region') }
                )
              }
              name="region"
              onChange={handleInputChange}
              value={newNgb.region || ''}
            >
              <option value="" />
              {REGION_OPTIONS.map(renderOption)}
            </select>
            {hasError('region') && <span className="text-red-500">Region cannot be blank</span>}
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Acronym</span>
            <input
              className="form-input mt-1 block w-full"
              placeholder="USQ"
              name="acronym"
              onChange={handleInputChange}
              value={newNgb.acronym}
            />
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Player Count</span>
            <input
              type="number"
              min="0"
              className="form-input mt-1 block w-full"
              name="playerCount"
              onChange={handleInputChange}
              value={newNgb.playerCount}
            />
          </label>
        </div>
        <label className="block">
          <span className="text-gray-700">Website</span>
          <input
            type="url"
            className="form-input mt-1 block w-full"
            placeholder="https://www.usquidditch.org"
            name="website"
            onChange={handleInputChange}
            value={newNgb.website}
          />
        </label>
        <div className="w-full my-8">
          <label>
            <span className="text-gray-700">Social Media</span>
            <MultiInput onChange={handleUrlChange} values={newNgb.urls || []} />
          </label>
        </div>
        <div className="w-full text-center">
          <button
            type="button"
            className={classnames("uppercase text-xl py-4 px-8 rounded-lg bg-green text-white", {'opacity-50 cursor-default': !hasChangedNgb})}
            onClick={handleSubmit}
            disabled={!hasChangedNgb}
          >
            Done
          </button>
        </div>
      </form>
    </Modal>
  )
}

export default NgbEditModal;
