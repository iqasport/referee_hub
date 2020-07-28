import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { useHistory } from 'react-router-dom'

import { getNationalGoverningBodies } from 'MainApp/modules/nationalGoverningBody/nationalGoverningBodies'
import { RootState } from 'MainApp/rootReducer'
import { Datum } from 'MainApp/schemas/getNationalGoverningBodiesSchema'

import NgbEditModal from '../NgbEditModal'
import Table, { CellConfig } from '../Table/Table'
import ActionDropdown from './ActionDropdown'

const HEADER_CELLS = ['name', 'region', 'acronym', 'player count', 'website', 'actions']

const NgbTable = () => {
  const [activeEdit, setActiveEdit] = useState<string>(null)
  const dispatch = useDispatch()
  const history = useHistory()
  const { isLoading, nationalGoverningBodies } = useSelector(
    (state: RootState) => state.nationalGoverningBodies, shallowEqual
  )

  useEffect(() => {
    if (!nationalGoverningBodies.length) {
      dispatch(getNationalGoverningBodies())
    }
  }, [nationalGoverningBodies])

  const handleRowClick = (id: string) => history.push(`/national_governing_bodies/${id}`)
  const handleEditClick = (id: string) => setActiveEdit(id)
  const handleEditClose = () => setActiveEdit(null)

  const renderEmpty = () => <h2>No National Governing Bodies found</h2>

  const rowConfig: CellConfig<Datum>[] = [
    {
      cellRenderer: (item: Datum) => {
        return item?.attributes.name
      },
      dataKey: 'name'
    },
    {
      cellRenderer: (item: Datum) => {
        return item?.attributes.region
      },
      dataKey: 'region'
    },
    {
      cellRenderer: (item: Datum) => {
        return item?.attributes.acronym
      },
      dataKey: 'acronym'
    },
    {
      cellRenderer: (item: Datum) => {
        return item?.attributes.playerCount.toString()
      },
      dataKey: 'playerCount'
    },
    {
      cellRenderer: (item: Datum) => {
        return item?.attributes.website
      },
      dataKey: 'website'
    },
    {
      cellRenderer: (item: Datum) => {
        return <ActionDropdown ngbId={item.id} onEditClick={handleEditClick} />
      },
      customStyle: 'text-right',
      dataKey: 'actions',
    }
  ]

  return (
    <>
      <Table
        items={nationalGoverningBodies}
        rowConfig={rowConfig}
        headerCells={HEADER_CELLS}
        isHeightRestricted={false}
        isLoading={isLoading}
        emptyRenderer={renderEmpty}
        onRowClick={handleRowClick}
      />
      {
        activeEdit
          ? (
            <NgbEditModal
              open={!!activeEdit}
              showClose={true}
              ngbId={parseInt(activeEdit, 10)}
              onClose={handleEditClose}
            />
          ) : null
      }
    </>
  )
}

export default NgbTable
