import React from "react";

import Toggle from "../../../components/Toggle";
import { UserDataViewModel } from "../../../store/serviceApi";

type HeaderNameProps = {
  name: string;
  onChange: (
    stateKey: "firstName" | "lastName"
  ) => (event: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>) => void;
  onToggleChange: (stateKey: string) => (event: React.ChangeEvent<HTMLInputElement>) => void;
  updatedValues: UserDataViewModel;
  originalValues: UserDataViewModel;
  isEditing: boolean;
};

const HeaderName = (props: HeaderNameProps) => {
  const hasName = props.originalValues && !!`${props.originalValues?.firstName}${props.originalValues?.lastName}`;

  const renderNameInput = (nameType: "firstName" | "lastName", placeholder: string): JSX.Element => (
    <input
      className="form-input mr-4"
      type="text"
      value={props.updatedValues && props.updatedValues[nameType] || ""}
      onChange={props.onChange(nameType)}
      placeholder={placeholder}
    />
  );

  // based in Referee profile for public display
  const nameHeader = <h1 className="text-4xl">{props.name}</h1>;

  // based in User Info for editing
  const firstNameInput = !hasName ? renderNameInput("firstName", "First name") : null;
  const lastNameInput = !hasName ? renderNameInput("lastName", "Last name") : null;
  const toggleExport = (
    <Toggle
      name="exportName"
      label="Export Name?"
      onChange={props.onToggleChange("exportName")}
      checked={props.updatedValues?.exportName ?? true}
    />
  );
  const renderEdit = (
    <div className="flex items-center">
      {firstNameInput}
      {lastNameInput}
      {toggleExport}
    </div>
  );

  return !props.isEditing ? nameHeader : renderEdit;
};

export default HeaderName;
