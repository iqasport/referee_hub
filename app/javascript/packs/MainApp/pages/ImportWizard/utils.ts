import fs from 'fs'
import Papa from 'papaparse'

export async function parseCsv(file: File) {
  const reader = new FileReader()

  reader.readAsBinaryString(file)
  reader.onloadend = () => {
    return reader.result
  }
}
