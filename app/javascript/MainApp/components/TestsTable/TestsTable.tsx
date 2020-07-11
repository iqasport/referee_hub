import { capitalize } from 'lodash'
import React, { useEffect } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { useHistory } from 'react-router-dom'

import { updateTest } from 'MainApp/modules/test/test'
import { getTests } from 'MainApp/modules/test/tests'
import { RootState } from 'MainApp/rootReducer'
import { Datum } from 'MainApp/schemas/getTestsSchema'
import { toDateTime } from 'MainApp/utils/dateUtils'
import Table, { CellConfig } from '../Table/Table'
import Toggle from '../Toggle'

const HEADER_CELLS = ['title', 'level', 'version', 'language', 'active', 'last updated']

const TestsTable = () => {
  const history = useHistory()
  const dispatch = useDispatch()
  const { tests, isLoading, certifications } = useSelector((state: RootState) => state.tests, shallowEqual)

  useEffect(() => {
    dispatch(getTests())
  }, [])

  const getVersion = (certId: number): string => {
    const certVersion = certifications.find((cert) => cert.id === certId.toString()).version

    switch(certVersion) {
      case 'eighteen':
        return '2018-2020'
      case 'twenty':
        return '2020-2022'
      default:
        return 'Unknown'
    }
  }

  const handleRowClick = (id: string) => {
    history.push(`/admin/tests/${id}`)
  }

  const handleToggle = (value: boolean, id: string) => {
    const newTest = { active: value }
    dispatch(updateTest(id, newTest))
  }

  const renderEmpty = () => {
    return <h2>No tests found</h2>
  }

  const rowConfig: CellConfig<Datum>[] = [
    {
      cellRenderer: (item: Datum) => {
        return item.attributes.name
      },
      dataKey: 'name',
    },
    {
      cellRenderer: (item: Datum) => {
        return capitalize(item.attributes.level)
      },
      dataKey: 'level'
    },
    {
      cellRenderer: (item: Datum) => {
        return getVersion(item.attributes.certificationId)
      },
      dataKey: 'certificationId'
    },
    {
      cellRenderer: (item: Datum) => {
        return item.attributes.language
      },
      dataKey: 'language'
    },
    {
      cellRenderer: (item: Datum) => {
        const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
          const value = event.currentTarget.checked
          handleToggle(value, item.id)
        }
        return <Toggle name="active" checked={item.attributes.active} onChange={handleChange} />
      },
      dataKey: 'active'
    },
    {
      cellRenderer: (item: Datum) => {
        return toDateTime(item.attributes.updatedAt).toFormat('D')
      },
      dataKey: 'updatedAt'
    }
  ]

  return (
    <>
      <h2 className="text-navy-blue text-2xl font-semibold my-4">{`All Tests(${tests.length})`}</h2>
      <Table
        items={tests}
        isLoading={isLoading}
        headerCells={HEADER_CELLS}
        onRowClick={handleRowClick}
        emptyRenderer={renderEmpty}
        rowConfig={rowConfig}
        isHeightRestricted={false}
      />
    </>
  )
}

export default TestsTable
