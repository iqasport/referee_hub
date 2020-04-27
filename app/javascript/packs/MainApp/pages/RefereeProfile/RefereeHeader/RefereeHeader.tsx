import { capitalize } from 'lodash'
import React from 'react'

import { UpdateRefereeRequest } from '../../../apis/referee';
import Toggle from '../../../components/Toggle';
import { DataAttributes, IncludedAttributes } from '../../../schemas/getRefereeSchema';
import HeaderButtons from './HeaderButtons';
import HeaderImage from './HeaderImage';
import HeaderName from './HeaderName';

type HeaderProps = {
  referee: DataAttributes;
  certifications: IncludedAttributes[],
  isEditing: boolean;
  onChange: (value: string | boolean, stateKey: string) => void;
  onEditClick: () => void;
  onSubmit: () => void;
  updatedValues: UpdateRefereeRequest;
  isSaveDisabled: boolean;
  id: string,
}

const RefereeHeader = (props: HeaderProps) => {
  const {
    referee,
    certifications,
    isEditing,
    onChange,
    onEditClick,
    onSubmit,
    updatedValues,
    id
  } = props;
  
  const refName = () => {
    if (referee.firstName || referee.lastName) {
      return `${referee.firstName} ${referee.lastName}`
    }

    return 'Anonymous Referee'
  }

  const handleChange = (stateKey: string) => (event: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>) => {
    const value = event.currentTarget.value
    onChange(value, stateKey)
  }

  const handleToggleChange = (stateKey: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.currentTarget.checked
    onChange(value, stateKey)
  }

  const renderCertifications = () => {
    if(isEditing) return null

    return certifications.map(certification => (
      <div key={certification.level} className="bg-green py-2 px-6 rounded mr-5">
        {`${capitalize(certification.level)}`}
      </div>
    ))
  }

  const renderPronouns = () => {
    if(!isEditing && referee.showPronouns) {
      return <h2 className="text-l">{referee.pronouns}</h2>
    } else if (isEditing) {
      return (
        <div className="flex items-center">
          <Toggle
            name="showPronouns"
            label="Show Pronouns?"
            onChange={handleToggleChange("showPronouns")}
            checked={updatedValues.showPronouns}
          />
          <input
            className="form-input"
            type="text"
            value={updatedValues.pronouns}
            onChange={handleChange("pronouns")}
          />
        </div>
      );
    }
  }

  const renderBio = () => {
    if(!isEditing) {
      return referee.bio
    } else {
      return <textarea aria-multiline="true" className="form-textarea block w-full" onChange={handleChange('bio')} value={updatedValues.bio} />
    }
  }

  return (
    <div className="flex">
      <HeaderImage avatarUrl={referee.avatarUrl} id={id} />
      <div className="w-5/6">
        <div className="flex items-center mb-8">
          <div className="flex-shrink mr-20 w-1/3">
            {<HeaderName 
              isEditing={isEditing} 
              onChange={handleChange} 
              onToggleChange={handleToggleChange} 
              updatedValues={updatedValues} 
              name={refName()} />}
          </div>
          <div className="flex items-center justify-around">
            {renderCertifications()}
          </div>
          <div className="flex justify-end w-1/2">
            {referee.isEditable && <HeaderButtons isEditing={isEditing} onEdit={onEditClick} onSubmit={onSubmit} isSaveDisabled={props.isSaveDisabled} />}
          </div>
        </div>
        <div className="flex mb-8">
          {renderPronouns()}
        </div>
        <div className="text-2xl mb-4 h-24">
          {renderBio()}
        </div>
      </div>
    </div>
  )
}

export default RefereeHeader
