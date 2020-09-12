import { faUser } from '@fortawesome/free-regular-svg-icons';
import { faMapPin } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { capitalize } from 'lodash'
import React from 'react'

import { UpdateRefereeRequest } from 'MainApp/apis/referee';
import Toggle from 'MainApp/components/Toggle';
import { DataAttributes, IncludedAttributes } from 'MainApp/schemas/getRefereeSchema';
import { getRefereeCertVersion } from 'MainApp/utils/certUtils';
import { toDateTime } from 'MainApp/utils/dateUtils';
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
  onCancel: () => void;
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
    id,
    onCancel,
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
    if (isEditing) return null

    return certifications.map(certification => (
      <div key={`${certification.level}-${certification.version}`} className="bg-white text-green border border-green py-2 px-6 rounded mr-5 mt-4">
        {`${capitalize(certification.level)} (${getRefereeCertVersion(certification)})`}
      </div>
    ))
  }

  const renderPronouns = (): JSX.Element | null => {
    if(!isEditing && referee.showPronouns) {
      return (
        <h2 className="text-l">
          <FontAwesomeIcon icon={faUser} className="mr-2" />          
          {referee.pronouns}
        </h2>
      )
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
    } else {
      return null
    }
  }

  const renderJoined = (): JSX.Element | null => {
    if (isEditing) return null

    const joinDate = toDateTime(referee.createdAt).year
    return (
      <h2 className="text-l">
        <FontAwesomeIcon className="mr-2" icon={faMapPin} />
        {`Joined ${joinDate}`}
      </h2>
    )
  }

  const renderBio = () => {
    if(!isEditing) {
      return referee.bio
    } else {
      return (
        <textarea 
          aria-multiline="true" 
          className="bg-gray-200 rounded p-4 text-lg block w-full mb-4"
          style={{resize: 'none'}} 
          onChange={handleChange('bio')} 
          value={updatedValues.bio} 
        />
      )
    }
  }

  return (
    <div className="flex flex-col lg:flex-row xl:flex-row">
      <HeaderImage avatarUrl={referee.avatarUrl} id={id} isEditable={referee.isEditable} />
      <div className="w-5/6 ml-8">
        <div className="flex flex-col items-center my-8 md:flex-row lg:flex-row xl:flex-row">
          <div className="flex-shrink w-full lg:mr-5 xl:mr-5 md:w-2/3 lg:w-2/3 xl:w-2/3">
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
          <div className="flex items-center flex-wrap">
            {renderCertifications()}
          </div>
          <div className="justify-end hidden md:flex lg:flex xl:flex">
            {referee.isEditable && (
              <HeaderButtons
                isEditing={isEditing}
                onEdit={onEditClick}
                onSubmit={onSubmit}
                onCancel={onCancel}
              />
            )}
          </div>
        </div>
        <div className="flex mb-8 justify-between w-56">
          {renderJoined()}
          {renderPronouns()}
        </div>
        <div className="text-2xl mb-4">{renderBio()}</div>
      </div>
    </div>
  );
}

export default RefereeHeader
