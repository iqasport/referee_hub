import React from 'react'

type ToggleProps = {
  name: string;
  label?: string;
  onChange: (event: React.ChangeEvent<HTMLInputElement>) => void;
  checked: boolean;
}

const Toggle = (props: ToggleProps) => {
  return (
    <div className="mb-2 mr-6">
      <div className="toggle inline-block align-middle">
        <input
          type="checkbox"
          className="toggle-checkbox"
          name={props.name}
          id={props.name}
          checked={props.checked}
          onChange={props.onChange}
        />
        <label htmlFor={props.name} className="toggle-label" />
      </div>
      {props.label && <label htmlFor={props.name} className="text-s text-gray-600">{props.label}</label>}
    </div>
  )
}

export default Toggle
