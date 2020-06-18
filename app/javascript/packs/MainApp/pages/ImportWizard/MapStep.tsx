import { faCaretRight } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { capitalize, difference } from 'lodash'
import Papa from 'papaparse'
import React, { useEffect, useState } from 'react'

export type HeadersMap = {
  [original: string]: string
}

export const REQUIRED_HEADERS = ['name', 'city', 'country', 'state', 'age_group', 'status', 'url']

interface MapStepProps {
  uploadedFile: File;
  mappedData: HeadersMap;
  onMappingUpdate: (mappedHeaders: HeadersMap) => void;
}

const MapStep = (props: MapStepProps) => {
  const { uploadedFile, mappedData, onMappingUpdate } = props
  const [originalHeaders, setOriginalHeaders] = useState<string[]>()
  const [originalData, setOriginalData] = useState<string[]>()

  useEffect(() => {
    if (uploadedFile) {
      const reader = new FileReader()

      reader.readAsBinaryString(uploadedFile)
      reader.onloadend = () => {
        const parsed = Papa.parse<string[]>(reader.result.toString())
        setOriginalHeaders(parsed.data[0])
        setOriginalData(parsed.data[1])
      }
    }
  }, [uploadedFile])

  const handleSelectMap = (columnName: string) => (event: React.ChangeEvent<HTMLSelectElement>) => {
    let mappedColumn = event.target.value
    
    // remove the default data mapping so we don't have duplicates
    const mapClone = Object.assign({}, mappedData)
    delete mapClone[mappedColumn]

    const urls = Object.values(mapClone).filter(value => value.match(/url/))
    
    if (mappedColumn === 'url') {
      mappedColumn = `url_${urls.length + 1}` 
    }

    onMappingUpdate({ ...mapClone, [columnName]: mappedColumn })
  }

  const renderColumn = (columnName: string) => {
    const columnIndex = originalHeaders.indexOf(columnName)
    const value = originalData && originalData[columnIndex]
    const mappedValue = mappedData[columnName] || ''
    const selectedValue = mappedValue?.match(/url/) ? 'url' : mappedValue

    return (
      <div className="flex justify-between items-center mb-4" key={columnName}>
        <div key={columnName} className="w-1/4">
          <h3 className="font-bold text-lg text-navy-blue capitalize">{columnName}</h3>
          <p className="italic text-gray-600">{value}</p>
        </div>
        <div className="w-1/2 flex items-center justify-center">
          <div className="h-1 bg-navy-blue w-1/2" />
          <FontAwesomeIcon icon={faCaretRight} size="3x" className="text-navy-blue" />
        </div>
        <div className="w-1/4">
          <select 
            className="form-select block mt-1 w-full" 
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

  const renderMapping = () => {
    const needsMapping = originalHeaders ? difference(originalHeaders, REQUIRED_HEADERS) : []
    if (!needsMapping.length) return <h1>All headers are correctly mapped, click Next to upload your csv</h1>

    return (
      <>
        <div className="flex justify-between items-center mb-8">
          <h4 className="uppercase text-xl">Your Columns</h4>
          <h4 className="uppercase text-xl">IQA Columns</h4>
        </div>
        {needsMapping.map(renderColumn)}
      </>
    )
  }

  return (
    <div className="w-1/2 mx-auto my-4 py-12">
      {renderMapping()}
    </div>
  )
}

export default MapStep
