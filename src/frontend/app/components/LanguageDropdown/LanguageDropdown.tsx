import classnames from "classnames";
import React from "react";

import { Datum } from "../../schemas/getLanguagesSchema";

interface LanguageDropdownProps {
  name: string;
  onChange: (event: React.ChangeEvent<HTMLSelectElement>) => void;
  value: string;
  languages: Datum[];
  hasError: boolean;
}

const LanguageDropdown = (props: LanguageDropdownProps) => {
  const { name, onChange, value, languages, hasError } = props;

  const renderOption = (language: Datum) => {
    const {
      attributes: { longName, shortRegion },
      id,
    } = language;
    const regionText = shortRegion ? ` - ${shortRegion}` : "";

    return (
      <option key={id} value={id}>
        {`${longName}${regionText}`}
      </option>
    );
  };

  return (
    <select
      className={classnames("form-select mt-1 block w-full", {
        "border border-red-500": hasError,
      })}
      name={name}
      placeholder="Select the language"
      onChange={onChange}
      value={value}
    >
      <option value="">Select the language</option>
      {languages.map(renderOption)}
    </select>
  );
};

export default LanguageDropdown;
