import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps, useHistory } from 'react-router-dom'

import { AssociationData, UpdateRefereeRequest } from 'MainApp/apis/referee';
import AdminCertificationsModal from 'MainApp/components/AdminCertificationsModal';
import TestResultCards from 'MainApp/components/TestResultCards'
import { updateUserPolicy } from 'MainApp/modules/currentUser/currentUser'
import { fetchReferee, updateReferee } from 'MainApp/modules/referee/referee';
import { RootState } from 'MainApp/rootReducer';

import RefereeHeader from './RefereeHeader'
import RefereeLocation from './RefereeLocation'
import RefereeTeam from './RefereeTeam'
import { IdParams } from './types'
import { initialUpdateState, selectRefereeState } from './utils'

const RefereeProfile = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id }}} = props
  const [isEditing, setIsEditing] = useState(false)
  const [isCertificationModalOpen, setIsCertificationModalOpen] = useState(false)
  const dispatch = useDispatch()
  const history = useHistory();
  const { referee, certifications, ngbs, locations, teams, id: stateId, testResults } = useSelector(
    selectRefereeState, shallowEqual
  )
  const [updatedReferee, setUpdatedReferee] = useState<UpdateRefereeRequest>(
    initialUpdateState(referee, locations, teams)
  )
  const { roles } = useSelector((state: RootState) => state.currentUser, shallowEqual)

  useEffect(() => {
    if (id) {
      dispatch(fetchReferee(id))
    }
  }, [id, dispatch])

  useEffect(() => {
    if (referee) {
      setUpdatedReferee(initialUpdateState(referee, locations, teams));
    }
  }, [referee])

  if (!referee) return null

  const isIqaAdmin = roles.includes('iqa_admin')
  const isCertificationsVisible = (referee.isEditable && !isEditing) || isIqaAdmin

  const handleSubmit = () => {
    setIsEditing(false)
    dispatch(updateReferee(updatedReferee, id))
  };
  const handleEditClick = () => setIsEditing(true)
  const handleTestsClick = () => {
    if (referee.isEditable) {
      history.push(`/referees/${id}/tests`)
    } else {
      setIsCertificationModalOpen(true)
    }
  }
  const handleCertificationModalClose = () => setIsCertificationModalOpen(false)

  const handleInputChange = (value: string | boolean, stateKey: string) => {
    setUpdatedReferee({ ...updatedReferee, [stateKey]: value })
  }
  const handleAssociationChange = (value: AssociationData, stateKey: string) => {
    setUpdatedReferee({ ...updatedReferee, [stateKey]: value})
  }
  const handleCancel = () => {
    setIsEditing(false)
    setUpdatedReferee(initialUpdateState(referee, locations, teams))
  }

  const handleAcceptPolicy = () => dispatch(updateUserPolicy(id, 'accept'))
  const handleRejectPolicy = () => dispatch(updateUserPolicy(id, 'reject'))

  const renderAcceptPolicy = () => {
    const { isEditable, hasPendingPolicies } = referee
    if (!isEditable) return null
    if (!hasPendingPolicies) return null

    return (
      <div className="w-full bg-yellow-lighter py-4 px-10 flex items-center justify-between">
        <p>
          {'We have updated our '}
          <a target="_blank" rel="noopener noreferrer" href="https://www.iqareferees.org/privacy">Privacy Policy</a>
          , please review and accept.
        </p>
        <div className="flex justify-end">
          <button className="bg-red-300 rounded p-2 mx-2 cursor-pointer" onClick={handleRejectPolicy}>Reject</button>
          <button className="rounded bg-blue p-2 mx-2 cursor-pointer" onClick={handleAcceptPolicy}>Accept</button>
        </div>
      </div>
    )
  }

  return (
    <>
      {renderAcceptPolicy()}
      <div className="m-auto w-full my-10 px-4 xl:w-3/4 xl:px-0">
        <RefereeHeader
          referee={referee}
          certifications={certifications}
          onChange={handleInputChange}
          onEditClick={handleEditClick}
          isEditing={isEditing}
          onSubmit={handleSubmit}
          updatedValues={updatedReferee}
          id={stateId}
          onCancel={handleCancel}
        />
        <div className="flex flex-col lg:flex-row xl:flex-row w-full">
          <div className="flex flex-col w-full lg:w-1/2 xl:w-1/2 rounded-lg bg-gray-100 p-4 mb-8">
            <div className="flex">
              <h3 className="border-b-2 border-green text-xl text-center">
                Details
              </h3>
            </div>
            <RefereeLocation
              ngbs={ngbs}
              locations={locations}
              isEditing={isEditing}
              onChange={handleAssociationChange}
              value={updatedReferee.ngbData}
            />
            <RefereeTeam
              teams={teams}
              locations={locations}
              isEditing={isEditing}
              onChange={handleAssociationChange}
              value={updatedReferee.teamsData}
              isDisabled={locations.length < 1}
            />
          </div>
          {isCertificationsVisible && (
            <div className="flex flex-col w-full lg:w-1/2 xl:w-1/2 rounded-lg bg-gray-100 p-4 lg:ml-8 xl:ml-8">
              <div className="flex justify-between items-center mb-4">
                <h3 className="border-b-2 border-green text-xl text-center">
                  Certifications
                </h3>
                <button
                  type="button"
                  className="border-2 border-green text-green text-center px-4 py-2 rounded bg-white"
                  onClick={handleTestsClick}
                >
                  {isIqaAdmin && !referee.isEditable ? 'Manage Certifications' : 'Take Tests'}
                </button>
              </div>
              <TestResultCards testResults={testResults} />
            </div>
          )}
        </div>
      </div>
      <AdminCertificationsModal
        open={isCertificationModalOpen}
        refereeId={id}
        onClose={handleCertificationModalClose}
      />
    </>
  );
}

export default RefereeProfile
