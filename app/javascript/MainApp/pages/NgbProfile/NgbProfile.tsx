import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps, useHistory } from 'react-router-dom'

import ExportModal, { ExportType } from 'MainApp/components/modals/ExportModal/ExportModal'
import NgbEditModal from 'MainApp/components/modals/NgbEditModal'
import TeamEditModal from 'MainApp/components/modals/TeamEditModal'
import StatsViewer from 'MainApp/components/StatsViewer'
import { CurrentUserState } from 'MainApp/modules/currentUser/currentUser'
import { exportNgbReferees, exportNgbTeams } from 'MainApp/modules/job/job'
import {
  getNationalGoverningBody,
  SingleNationalGoverningBodyState
} from 'MainApp/modules/nationalGoverningBody/nationalGoverningBody'
import { RootState } from 'MainApp/rootReducer'

import ActionsButton from './ActionsButton'
import NgbTables from './NgbTables'
import Sidebar from './Sidebar'

type IdParams = { id: string }

enum ModalType {
  Export = 'export',
  Team = 'team',
  Edit = 'edit',
}

const NgbProfile = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id } } } = props
  const [openModal, setOpenModal] = useState<ModalType>()
  const dispatch = useDispatch()
  const history = useHistory()
  const { ngb, socialAccounts, refereeCount, teamCount, stats } = useSelector(
    (state: RootState): SingleNationalGoverningBodyState => state.nationalGoverningBody, shallowEqual
  )
  const { currentUser, roles } = useSelector((state: RootState): CurrentUserState => state.currentUser, shallowEqual)

  useEffect(() => {
    if (id) {
      dispatch(getNationalGoverningBody(parseInt(id, 10)))
    }
  }, [id, dispatch])

  if (!ngb) return null
  const isUserReferee = roles.length === 1 && roles.includes('referee')
  const isUserNgbAdmin = roles.includes('ngb_admin') && Number(id) === currentUser.ownedNgbId

  if (isUserReferee) {
    history.goBack()
  } else if (!roles.includes('iqa_admin') && !isUserNgbAdmin) {
    history.goBack()
  }

  const handleOpenModal = (type: ModalType) => () => setOpenModal(type)
  const handleCloseModal = () => setOpenModal(null)
  const handleExport = (type: ExportType) => {
    handleCloseModal()

    switch(type) {
      case ExportType.Team:
        dispatch(exportNgbTeams(id));
        break;
      case ExportType.Referee:
        dispatch(exportNgbReferees(id));
        break;
    }
  }
  const handleImportClick = () => history.push(`/import/team_${id}`)

  const renderModals = () => {
    switch(openModal) {
      case ModalType.Export:
        return <ExportModal open={true} onClose={handleCloseModal} onExport={handleExport} />
      case ModalType.Team:
        return <TeamEditModal open={true} onClose={handleCloseModal} showClose={true} ngbId={id} />
      case ModalType.Edit:
        return <NgbEditModal open={true} onClose={handleCloseModal} showClose={true} ngbId={parseInt(id, 10)} />
      default:
        return null
    }
  }

  return (
    <div className="w-5/6 mx-auto my-8">
      <div className="flex justify-between w-full mb-8">
        <h1 className="w-full text-4xl text-navy-blue font-extrabold">{ngb.name}</h1>
        <ActionsButton 
          onEditClick={handleOpenModal(ModalType.Edit)}
          onImportClick={handleImportClick}
          onExportClick={handleOpenModal(ModalType.Export)}
          onTeamClick={handleOpenModal(ModalType.Team)}
        />
      </div>
      <div className="flex w-full flex-col md:flex-row">
        <Sidebar 
          ngb={ngb} 
          socialAccounts={socialAccounts} 
          refereeCount={refereeCount} 
          teamCount={teamCount} 
          isEditing={false} 
          ngbId={id} 
        />
        <div className="flex flex-col w-full md:w-4/5 md:pl-8">
          <StatsViewer stats={stats} />
          <NgbTables ngbId={parseInt(id, 10)} />
        </div>
      </div>
      {renderModals()}
    </div>
  )
}

export default NgbProfile
