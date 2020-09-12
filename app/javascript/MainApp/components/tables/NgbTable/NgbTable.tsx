import { capitalize } from 'lodash'
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { useHistory } from 'react-router-dom'

import { AnnotatedNgb } from 'MainApp/apis/nationalGoverningBody'
import { getNationalGoverningBodies } from 'MainApp/modules/nationalGoverningBody/nationalGoverningBodies'
import { RootState } from 'MainApp/rootReducer'

import NgbEditModal from '../../modals/NgbEditModal'
import Table, { CellConfig } from '../Table/Table'
import ActionDropdown from './ActionDropdown'

const HEADER_CELLS = ['name', 'region', 'membership status', 'player count', 'team count', 'referee count', 'actions']

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

  const rowConfig: CellConfig<AnnotatedNgb>[] = [
    {
      cellRenderer: (item: AnnotatedNgb) => {
        return item?.attributes.name
      },
      dataKey: 'name'
    },
    {
      cellRenderer: (item: AnnotatedNgb) => {
        return item?.attributes?.region?.split('_').map((word) => capitalize(word)).join(' ')
      },
      dataKey: 'region'
    },
    {
      cellRenderer: (item: AnnotatedNgb) => {
        return item?.attributes?.membershipStatus?.split('_').map((word) => capitalize(word)).join(' ')
      },
      dataKey: 'membershipStatus'
    },
    {
      cellRenderer: (item: AnnotatedNgb) => {
        return item?.attributes.playerCount.toString()
      },
      dataKey: 'playerCount'
    },
    {
      cellRenderer: (item: AnnotatedNgb) => {
        return item?.teamCount.toString()
      },
      dataKey: 'teamCount'
    },
    {
      cellRenderer: (item: AnnotatedNgb) => {
        return item?.refereeCount.toString()
      },
      dataKey: 'refereeCount'
    },
    {
      cellRenderer: (item: AnnotatedNgb) => {
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
