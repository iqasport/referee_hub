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
      return (
        <textarea 
          aria-multiline="true" 
          className="bg-gray-200 rounded p-4 text-lg block w-full"
          style={{resize: 'none'}} 
          onChange={handleChange('bio')} 
          value={updatedValues.bio} 
        />
      )
    }
  }

  return (
    <div className="flex flex-col lg:flex-row xl:flex-row">
      <HeaderImage avatarUrl={referee.avatarUrl} id={id} />
      <div className="w-5/6">
        <div className="flex flex-col items-center mb-8 md:flex-row lg:flex-row xl:flex-row">
          <div className="flex-shrink w-full lg:mr-5 xl:mr-5 md:w-1/2 lg:w-1/2 xl:w-1/2">
            {
              <HeaderName
                isEditing={isEditing}
                onChange={handleChange}
                onToggleChange={handleToggleChange}
                updatedValues={updatedValues}
                name={refName()}
              />
            }
          </div>
          <div className="flex items-center w-full">
            {renderCertifications()}
          </div>
          <div className="justify-end w-1/2 hidden md:flex lg:flex xl:flex">
            {referee.isEditable && (
              <HeaderButtons
                isEditing={isEditing}
                onEdit={onEditClick}
                onSubmit={onSubmit}
              />
            )}
          </div>
        </div>
        <div className="flex mb-8">{renderPronouns()}</div>
        <div className="text-2xl mb-4 h-24">{renderBio()}</div>
      </div>
    </div>
  );
}

export default RefereeHeader
