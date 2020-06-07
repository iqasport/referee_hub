import classnames from 'classnames'
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps } from 'react-router-dom'

import { RootState } from 'rootReducer'
import ExportModal, { ExportType } from '../../components/ExportModal/ExportModal'
import NewRefereeTable from '../../components/NewRefereeTable'
import StatsViewer from '../../components/StatsViewer'
import TeamTable from '../../components/TeamTable'
import { getNationalGoverningBody, SingleNationalGoverningBodyState } from '../../modules/nationalGoverningBody/nationalGoverningBody'
import ActionsButton from './ActionsButton'
import Sidebar from './Sidebar'

type IdParams = { id: string }

enum SelectedTable {
  Referees = 'referees',
  Teams = 'teams',
}

enum ModalType {
  Export = 'export',
  Import = 'import',
}

const NgbAdmin = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id } } } = props
  const [selectedTable, setSelectedTable] = useState(SelectedTable.Referees)
  const [openModal, setOpenModal] = useState<ModalType>()
  const [isEditing, setIsEditing] = useState(false)
  const dispatch = useDispatch()
  const { ngb, socialAccounts, refereeCount, teamCount, stats } = useSelector((state: RootState): SingleNationalGoverningBodyState => {
    return {
      error: state.nationalGoverningBody.error,
      id: state.nationalGoverningBody.id,
      isLoading: state.nationalGoverningBody.isLoading,
      ngb: state.nationalGoverningBody.ngb,
      refereeCount: state.nationalGoverningBody.refereeCount,
      socialAccounts: state.nationalGoverningBody.socialAccounts,
      stats: state.nationalGoverningBody.stats,
      teamCount: state.nationalGoverningBody.teamCount,
    }
  }, shallowEqual)

  useEffect(() => {
    if (id) {
      dispatch(getNationalGoverningBody(parseInt(id, 10)))
    }
  }, [id, dispatch])

  if (!ngb) return null

  const handleTableToggle = (table: SelectedTable) => (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    event.preventDefault()
    if (selectedTable === table) return

    setSelectedTable(table)
  }
  const handleEditClick = () => setIsEditing(true)
  const handleOpenModal = (type: ModalType) => () => setOpenModal(type)
  const handleCloseModal = () => setOpenModal(null)
  const handleExport = (type: ExportType) => console.log(type);

  const renderModals = () => {
    switch(openModal) {
      case ModalType.Export:
        return <ExportModal open={true} onClose={handleCloseModal} onExport={handleExport} />
      default:
        return null
    }
  }
  const isRefereesActive = selectedTable === SelectedTable.Referees
  const isTeamsActive = selectedTable === SelectedTable.Teams

  return (
    <div className="w-5/6 mx-auto my-8">
      <div className="flex justify-between w-full mb-8">
        <h1 className="w-full text-4xl text-navy-blue font-extrabold">{ngb.name}</h1>
        <ActionsButton 
          onEditClick={handleEditClick} 
          onImportClick={handleOpenModal(ModalType.Import)} 
          onExportClick={handleOpenModal(ModalType.Export)} 
        />
      </div>
      <div className="flex w-full flex-row">
        <Sidebar 
          ngb={ngb} 
          socialAccounts={socialAccounts} 
          refereeCount={refereeCount} 
          teamCount={teamCount} 
          isEditing={false} 
          ngbId={id} 
        />
        <div className="flex flex-col w-4/5 pl-8">
          <StatsViewer stats={stats} />
          <div className="w-full">
            <div className="flex justify-start w-full mt-8">
              <button 
                className={classnames('button-tab', { ['active-button-tab']: isRefereesActive })} 
                onClick={handleTableToggle(SelectedTable.Referees)}
              >
                Referees
              </button>
              <button 
                className={classnames('button-tab', { ['active-button-tab']: isTeamsActive })}
                onClick={handleTableToggle(SelectedTable.Teams)}
              >
                Teams
              </button>
            </div>
            {isRefereesActive && <NewRefereeTable ngbId={parseInt(id, 10)} />}
            {isTeamsActive && <TeamTable ngbId={parseInt(id, 10)} />}
          </div>
        </div>
      </div>
      {renderModals()}
    </div>
  )
}

export default NgbAdmin
