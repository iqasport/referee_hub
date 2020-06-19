import { uniqueId } from 'lodash'
import React, { useState } from 'react'
import Input from './Input'

type InputConfig = {
  value: string;
  id: string;
}

const emptyInput = (): InputConfig => ({
  id: uniqueId('input'),
  value: '',
})

const generateInputConfig = (values?: string[]): InputConfig[] => {
  if (!values || !values.length) {
    return [emptyInput()]
  }

  return values.map((value) => {
    return {
      id: uniqueId('input'),
      value
    }
  })
}

interface MultiInputProps {
  values?: string[];
  onChange: (newValues: string[]) => void;
}

const MultiInput = (props: MultiInputProps) => {
  const { values, onChange } = props
  const [inputConfigs, setInputConfigs] = useState(generateInputConfig(values))

  const handleChange = (value: string, id: string) => {
    const newValues: string[] = []
    const newInputs = inputConfigs.map((input) => {
      newValues.push(input.value)
      if (input.id !== id) return input;

      return { ...input, value }
    })

    onChange(newValues)
    setInputConfigs(newInputs)
  }

  const handleRemove = (id: string) => {
    const newInputs = inputConfigs.filter((input) => input.id !== id)

    setInputConfigs(newInputs)
  }

  const handleAdd = () => {
    setInputConfigs([...inputConfigs, emptyInput()])
  }

  const renderInput = (input: InputConfig) => {
    return <Input key={input.id} value={input.value} id={input.id} onChange={handleChange} onRemove={handleRemove} />
  }

  return (
    <div>
      {inputConfigs.map(renderInput)}
      <button type="button" onClick={handleAdd}>Add</button>
    </div>
  )
}

export default MultiInput
