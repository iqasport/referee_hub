import { capitalize } from 'lodash'
import React, { useEffect } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { useHistory } from 'react-router-dom'

import { getTests } from 'MainApp/modules/test/tests'
import { RootState } from 'MainApp/rootReducer'
import { Datum } from 'MainApp/schemas/getTestsSchema'
import { toDateTime } from 'MainApp/utils/dateUtils'
import Table, { CellConfig } from '../Table/Table'
import Toggle from '../Toggle'

const HEADER_CELLS = ['title', 'level', 'language', 'active', 'last updated']

const TestsTable = () => {
  const history = useHistory()
  const dispatch = useDispatch()
  const { tests, isLoading } = useSelector((state: RootState) => state.tests, shallowEqual)

  useEffect(() => {
    dispatch(getTests())
  }, [])

  const handleRowClick = (id: string) => {
    history.push(`/tests/${id}`)
  }

  const handleToggle = (value: boolean, id: string) => {
    // dispatch()
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
    <Table
      items={tests}
      isLoading={isLoading}
      headerCells={HEADER_CELLS}
      onRowClick={handleRowClick}
      emptyRenderer={renderEmpty}
      rowConfig={rowConfig}
    />
  )
}

export default TestsTable
