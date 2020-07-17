import { faPlusCircle } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { uniqueId } from 'lodash'
import React, { useEffect, useState } from 'react'
import Input from './Input'

type InputConfig = {
  value: string;
  id: string;
}

const emptyInput = (): InputConfig => ({
  id: uniqueId('input'),
  value: '',
})

const generateInputConfig = (values: string[]): InputConfig[] => {
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
  const [inputConfigs, setInputConfigs] = useState<InputConfig[]>()

  useEffect(() => {
    let initialInputs: InputConfig[]
    if (!values || !values.length) {
      initialInputs = [emptyInput()]
    } else {
      initialInputs = generateInputConfig(values)
    }

    setInputConfigs(initialInputs)
  }, [values])

  const handleChange = (value: string, id: string) => {
    const newValues: string[] = []
    const newInputs = inputConfigs.map((input) => {
      if (input.id !== id) {
        newValues.push(input.value)
        return input;
      } else {
        newValues.push(value)
        return { ...input, value }
      }
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
    return (
      <Input 
        key={input.id}
        value={input.value}
        id={input.id}
        onChange={handleChange}
        onRemove={handleRemove}
        placeholder="https://iqasport.org"
      />
    )
  }

  return (
    <div>
      {inputConfigs?.map(renderInput)}
      <div className="my-4">
        <button type="button" className="align-center" onClick={handleAdd}>
          <FontAwesomeIcon icon={faPlusCircle} size="2x" className="text-blue-darker ml-2" />
        </button>
      </div>
    </div>
  )
}

export default MultiInput
