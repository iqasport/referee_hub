import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'

import { GetTeamsFilter } from '../../apis/team'
import { deleteTeam } from '../../modules/team/team'
import { getTeams, TeamsState, updateFilters } from '../../modules/team/teams'
import { RootState } from '../../rootReducer'
import { Datum } from '../../schemas/getTeamsSchema'

import Table, { CellConfig } from '../Table/Table'
import TeamEditModal from '../TeamEditModal'
import WarningModal from '../WarningModal'
import ActionDropdown from './ActionDropdown'

enum ModalType {
  Edit = 'edit',
  Delete = 'delete',
}

interface TeamTableProps {
  ngbId: number
}

const TeamTable = (props: TeamTableProps) => {
  const [openModal, setOpenModal] = useState<ModalType>()
  const [activeTeamId, setActiveTeamId] = useState<string>()

  const dispatch = useDispatch()
  const { teams, isLoading } = useSelector((state: RootState): TeamsState => state.teams, shallowEqual)

  useEffect(() => {
    const filter: GetTeamsFilter = { nationalGoverningBodies: [props.ngbId] }
    dispatch(updateFilters(filter))
    dispatch(getTeams(filter))
  }, [])

  const handleCloseModal = () => setOpenModal(null)
  const handleEditClick = (teamId: string) => {
    setActiveTeamId(teamId)
    setOpenModal(ModalType.Edit)
  }
  const handleDeleteClick = (teamId: string) => {
    setActiveTeamId(teamId)
    setOpenModal(ModalType.Delete)
  }
  const handleDeleteConfirm = () => {
    dispatch(deleteTeam(activeTeamId))
    handleCloseModal()
  }

  const renderModals = () => {
    switch(openModal) {
      case ModalType.Edit:
        return <TeamEditModal teamId={activeTeamId} open={true} onClose={handleCloseModal} showClose={true} />
      case ModalType.Delete:
        return <WarningModal open={true} onCancel={handleCloseModal} action="delete" dataType="team" onConfirm={handleDeleteConfirm} />
      default:
        return null
    }
  }

  const renderEmpty = () => <h2>No teams found</h2>

  const HEADER_CELLS = ['name', 'city', 'type', 'status', 'actions']
  const rowConfig: CellConfig<Datum>[] = [
    {
      cellRenderer: (item: Datum) => {
        return item.attributes.name
      },
      dataKey: 'name'
    },
    {
      cellRenderer: (item: Datum) => {
        return `${item.attributes.city}, ${item.attributes.state}`
      },
      dataKey: 'city'
    },
    {
      cellRenderer: (item: Datum) => {
        return item.attributes.groupAffiliation
      },
      dataKey: 'groupAffiliation'
    },
    {
      cellRenderer: (item: Datum) => {
        return item.attributes.status
      },
      dataKey: 'status'
    },
    {
      cellRenderer: (item: Datum) => {
        return <ActionDropdown teamId={item.id} onEditClick={handleEditClick} onDeleteClick={handleDeleteClick} />
      },
      customStyle: 'text-right',
      dataKey: 'actions',
    }
  ]

  return (
    <>
      <Table
        items={teams}
        isLoading={isLoading}
        headerCells={HEADER_CELLS}
        rowConfig={rowConfig}
        emptyRenderer={renderEmpty}
        isHeightRestricted={true}
      />
      {renderModals()}
    </>
  )
}

export default TeamTable
