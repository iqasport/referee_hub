import React, { useCallback } from 'react'
import { useDropzone } from 'react-dropzone';

interface DragNDropProps {
  onFileUpload: (uploadedFile: File) => void;
}

const DragNDrop = (props: DragNDropProps) => {
  const onDrop = useCallback((acceptedFiles: File[]) => {
    props.onFileUpload(acceptedFiles[0])
  }, [])

  const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDrop, accept: ['text/csv', 'application/vnd.ms-excel'] })
  
  return (
    <div {...getRootProps({ className: 'w-full p-20 text-center border-dashed rounded border-gray-400 border-4 text-navy-blue mt-8 cursor-pointer' })}>
      <input {...getInputProps()} />
      {
        isDragActive ?
          <p>Drop the csv file here ...</p> :
          <p>Drag 'n' drop a csv file here, or click to select a file</p>
      }
    </div>
  )
}

export default DragNDrop
