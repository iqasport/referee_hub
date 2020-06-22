import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'

import { RootState } from 'rootReducer'
import TeamEditModal from '../../components/TeamEditModal'
import WarningModal from '../../components/WarningModal'
import { deleteTeam } from '../../modules/team/team'
import { getNgbTeams, TeamsState } from '../../modules/team/teams'
import { Datum } from '../../schemas/getTeamsSchema'
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
    dispatch(getNgbTeams())
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

  const renderRow = (team: Datum) => {
    const teamCity = `${team.attributes.city}, ${team.attributes.state}`
    return (
      <tr key={team?.id} className="border border-gray-300 hover:bg-gray-300">
        <td className="w-1/4 py-4 px-8">{team.attributes.name}</td>
        <td className="w-1/4 py-4 px-8">{teamCity}</td>
        <td className="w-1/4 py-4 px-8">{team.attributes.groupAffiliation}</td>
        <td className="w-1/4 py-4 px-8">{team.attributes.status}</td>
        <td className="w-1/4 py-4 px-8 text-right">
          <ActionDropdown teamId={team.id} onEditClick={handleEditClick} onDeleteClick={handleDeleteClick} />
        </td>
      </tr>
    )
  }

  const renderBody = () => {
    return (
      <tbody>
        {teams.map(renderRow)}
      </tbody>
    )
  }

  const renderEmpty = () => {
    return (
      <tbody>
        <tr>
          <td>
            <h2>{isLoading ? 'Loading...' : 'No teams found'}</h2>
          </td>
        </tr>
      </tbody>
    )
  }

  return (
    <>
      {teams.length && (
        <table className="rounded-table-header">
          <tbody>
            <tr className="text-left">
              <td className="w-1/4 py-4 px-8">name</td>
              <td className="w-1/4 py-4 px-8">city</td>
              <td className="w-1/4 py-4 px-8">type</td>
              <td className="w-1/4 py-4 px-8">status</td>
              <td className="w-1/4 py-4 px-8 text-right">actions</td>
            </tr>
          </tbody>
        </table>
      )}
      <div className="table-container">
        <table className="rounded-table">
          {teams.length ? renderBody() : renderEmpty()}
        </table>
      </div>
      {renderModals()}
    </>
  )
}

export default TeamTable
