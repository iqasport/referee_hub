import React from 'react'

import { UpdateRefereeRequest } from '../../../apis/referee';
import Toggle from '../../../components/Toggle';

type HeaderNameProps = {
  name: string;
  onChange: (stateKey: string) => (event: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>) => void;
  onToggleChange: (stateKey: string) => (event: React.ChangeEvent<HTMLInputElement>) => void;
  updatedValues: UpdateRefereeRequest;
  isEditing: boolean;
}

const HeaderName = (props: HeaderNameProps) => {
  const hasName = props.name !== "Anonymous Referee";
  
  const renderNameInput = (nameType: string): JSX.Element => (
    <input className="form-input" type="text" value={props.updatedValues[nameType] || ""} onChange={props.onChange(nameType)} />
  )
    
  const nameHeader = <h1 className="text-4xl">{props.name}</h1>;
  const firstNameInput = !hasName ? renderNameInput("firstName") : null
  const lastNameInput = !hasName ? renderNameInput("lastName") : null
  const toggleExport = (
    <Toggle
      name="exportName"
      label="Export Name?"
      onChange={props.onToggleChange("exportName")}
      checked={props.updatedValues.exportName}
    />
  )
  const renderEdit = (
    <div className="flex items-center">
      {firstNameInput}
      {lastNameInput}
      {toggleExport}
    </div>
  )

  return !props.isEditing ? nameHeader : renderEdit
};

export default HeaderName
