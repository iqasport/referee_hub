import { capitalize } from 'lodash'
import Papa from 'papaparse'
import React, { useEffect, useState } from 'react'
import { HeadersMap } from './ImportWizard'

const REQUIRED_HEADERS = ['name', 'city', 'country', 'state', 'age_group', 'status', 'url']

interface MapStepProps {
  uploadedFile: File;
  mappedData: HeadersMap;
  onMappingUpdate: (mappedHeaders: HeadersMap) => void;
}

const MapStep = (props: MapStepProps) => {
  const { uploadedFile, mappedData, onMappingUpdate } = props
  // tslint:disable-next-line: no-any
  const [originalHeaders, setOriginalHeaders] = useState<any>()
  // tslint:disable-next-line: no-any
  const [originalData, setOriginalData] = useState<any[]>()

  useEffect(() => {
    if (uploadedFile) {
      const reader = new FileReader()

      reader.readAsBinaryString(uploadedFile)
      reader.onloadend = () => {
        const parsed = Papa.parse(reader.result.toString())
        setOriginalHeaders(parsed.data[0])
        setOriginalData(parsed.data.slice(1))
      }
    }
  }, [uploadedFile])

  const handleSelectMap = (columnName: string) => (event: React.ChangeEvent<HTMLSelectElement>) => {
    let mappedColumn = event.target.value
    const urls = Object.values(mappedData).filter(value => value.match(/url/))
    
    if (mappedColumn === 'url') {
      mappedColumn = `url_${urls.length + 1}` 
    }

    onMappingUpdate({ ...mappedData, [columnName]: mappedColumn })
  }

  const renderColumn = (columnName: string, index: number) => {
    const value = originalData && originalData[0][index]
    const mappedValue = mappedData[columnName] || ''
    const selectedValue = mappedValue?.match(/url/) ? 'url' : mappedValue

    return (
      <div className="flex justify-between items-center">
        <div key={columnName} className="w-1/4">
          <h3 className="font-bold">{columnName}</h3>
          <p className="italic">{value}</p>
        </div>
        <div className="w-1/4">
          <select 
            className="form-select block mt-1" 
            onChange={handleSelectMap(columnName)}
            value={selectedValue}
          >
            <option value="">Select</option>
            {REQUIRED_HEADERS.map((header) => {
              return (
                <option key={header} value={header} disabled={header === selectedValue}>
                  {header.split('_').map(word => capitalize(word)).join(' ')}
                </option>
              )
            })}
          </select>
        </div>
      </div>
    )
  }

  return (
    <div className="w-1/2 mx-auto my-4 py-12">
      <div className="flex justify-between items-center mb-8">
        <h4>Your Columns</h4>
        <h4>IQA Fields</h4>
      </div>
      {originalHeaders && originalHeaders.map(renderColumn)}
    </div>
  )
}

export default MapStep
