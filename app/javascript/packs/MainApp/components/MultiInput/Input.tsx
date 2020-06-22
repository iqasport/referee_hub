import { faMinusCircle } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import React from 'react'

interface InputProps {
  value: string;
  onChange: (value: string, id: string) => void;
  id: string;
  onRemove: (id: string) => void;
  placeholder?: string;
}

const Input = (props: InputProps) => {
  const { value, onChange, id, onRemove, placeholder } = props

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    onChange(event.target.value, id)
  }

  const handleRemove = () => {
    onRemove(id)
  }

  return (
    <div className="flex items-center">
      <input 
        className="form-input mt-1 mr-4 block w-full"
        placeholder={placeholder}
        value={value} 
        onChange={handleChange} 
      />
      <button type="button" onClick={handleRemove} className="text-red-600">
        <FontAwesomeIcon icon={faMinusCircle} size="2x" />
      </button>
    </div>
  )
}

export default Input
