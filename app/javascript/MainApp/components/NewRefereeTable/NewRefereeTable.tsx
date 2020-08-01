import { capitalize } from 'lodash'
import React, { useEffect } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { useHistory } from 'react-router-dom'

import { GetRefereesFilter } from '../../apis/referee'
import { getReferees, Referee, updateFilters } from '../../modules/referee/referees'
import { RootState } from '../../rootReducer'
import { AssociationType } from '../../schemas/getRefereesSchema'
import FilterToolbar from '../FilterToolbar'
import Table, { CellConfig } from '../Table/Table'

const HEADER_CELLS = ['name', 'highest certification', 'associated teams', 'secondary NGB']
const ADMIN_HEADER_CELLS = ['name', 'highest certification', 'associated teams', 'associated NGBs']

const findHighestCert = (referee: Referee) => {
  return referee?.certifications.find((cert) => {
    if (cert.level === 'head') {
      return true
    } else if (cert.level === 'assistant') {
      return true
    } else if (cert.level === 'snitch') {
      return true
    }

    return false
  })
}

type NewRefereeTableProps = {
  ngbId?: number;
  isHeightRestricted?: boolean;
}

const NewRefereeTable = (props: NewRefereeTableProps) => {
  const { ngbId, isHeightRestricted } = props
  const history = useHistory()
  const dispatch = useDispatch()
  const { referees, isLoading, meta, filters } = useSelector((state: RootState) => state.referees, shallowEqual)

  const headerCells = ngbId ? HEADER_CELLS : ADMIN_HEADER_CELLS

  useEffect(() => {
    const filter: GetRefereesFilter = {}
    if (props.ngbId) filter.nationalGoverningBodies = [props.ngbId]

    dispatch(updateFilters(filter))
    dispatch(getReferees(filter))
  }, [])

  const handleRowClick = (id: string) => {
    history.push(`/referees/${id}`)
  }

  const handleClearSearch = () => handleSearch('')

  const handleSearch = (newValue: string) => {
    const newFilters: GetRefereesFilter = { ...filters, q: newValue }
    dispatch(updateFilters(newFilters))
    dispatch(getReferees(newFilters))
  }

  const handlePageSelect = (newPage: number) => {
    const newFilters: GetRefereesFilter = { ...filters, page: newPage }
    dispatch(updateFilters(newFilters))
    dispatch(getReferees(newFilters))
  }

  const renderEmpty = () => {
    return (
      <h2>No referees found.</h2>
    )
  }

  const rowConfig: CellConfig<Referee>[] = [
    {
      cellRenderer: (item: Referee) => {
        if (!item?.referee.firstName) return 'Anonymous Referee'
        return `${item?.referee.firstName} ${item?.referee.lastName}`
      },
      dataKey: 'name',
    },
    {
      cellRenderer: (item: Referee) => {
        const highestCert = findHighestCert(item)
        return highestCert ? capitalize(highestCert?.level) : 'Uncertified'
      },
      dataKey: 'certifications'
    },
    {
      cellRenderer: (item: Referee) => {
        return item?.teams.map((team) => team.name).join(', ')
      },
      dataKey: 'teams'
    },
  ]

  if (ngbId) {
    rowConfig.push({
      cellRenderer: (item: Referee) => {
        const secondary = item?.locations.filter((location) => location.associationType === AssociationType.Secondary)
        const secondaryName = secondary.length && item?.ngbs.find((ngb) => {
          return ngb.id === secondary[0].nationalGoverningBodyId.toString()
        })?.name
        return secondaryName || 'N/A'
      },
      dataKey: 'locations'
    })
  } else {
    rowConfig.push({
      cellRenderer: (item: Referee) => {
        return item?.ngbs.map((location) => location.name).join(', ')
      },
      dataKey: 'locations'
    })
  }

  return (
    <div className="w-full">
      <FilterToolbar
        currentPage={parseInt(meta?.page, 10)}
        onClearSearch={handleClearSearch}
        total={meta?.total}
        onSearchInput={handleSearch}
        onPageSelect={handlePageSelect}
      />
      <Table
        items={referees}
        isLoading={isLoading}
        headerCells={headerCells}
        rowConfig={rowConfig}
        onRowClick={handleRowClick}
        emptyRenderer={renderEmpty}
        isHeightRestricted={props.isHeightRestricted}
      />
    </div>
  )
}

export default NewRefereeTable
