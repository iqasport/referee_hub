import classnames from "classnames";
import React from "react";

interface LanguageDropdownProps {
  name: string;
  onChange: (event: React.ChangeEvent<HTMLSelectElement>) => void;
  value: string;
  languages: string[];
  hasError: boolean;
}

const LanguageDropdown = (props: LanguageDropdownProps) => {
  const { name, onChange, value, languages, hasError } = props;

  const renderOption = (language: string) => {
    return (
      <option key={language} value={language}>
        {language}
      </option>
    );
  };

  return (
    <select
      className={classnames("form-select mt-1 block w-full", {
        "border border-red-500": hasError,
      })}
      name={name}
      onChange={onChange}
      value={value}
    >
      <option value="">Select the language</option>
      {languages.map(renderOption)}
    </select>
  );
};

export default LanguageDropdown;
