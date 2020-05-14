import { DateTime } from 'luxon'
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps, useHistory } from 'react-router-dom'
import { Message } from 'semantic-ui-react'

import { AssociationData, UpdateRefereeRequest } from '../../apis/referee';
import TestResultCards from '../../components/TestResultCards'
import { updateUserPolicy } from '../../modules/currentUser/currentUser'
import { fetchReferee, updateReferee } from '../../modules/referee/referee';
import RefereeHeader from './RefereeHeader'
import RefereeLocation from './RefereeLocation'
import RefereeTeam from './RefereeTeam'
import { IdParams, PaymentState } from './types'
import { initialPaymentState, initialUpdateState, selectRefereeState } from './utils'

const RefereeProfile = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id }}} = props
  const dispatch = useDispatch()
  const history = useHistory();
  const { referee, certifications, ngbs, locations, teams, id: stateId, testResults } = useSelector(selectRefereeState, shallowEqual)
  const [paymentState, setPaymentState] = useState<PaymentState>(initialPaymentState)
  const [updatedReferee, setUpdatedReferee] = useState<UpdateRefereeRequest>(initialUpdateState(referee, locations, teams))
  const [isEditing, setIsEditing] = useState(false)

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

  const handleRouteChange = (newRoute) => history.push(newRoute)

  const handleSubmit = () => {
    setIsEditing(false)
    dispatch(updateReferee(updatedReferee, id))
  };
  const handleEditClick = () => setIsEditing(true)

  // Payment handlers
  const clearPaymentState = () => setPaymentState(initialPaymentState)
  const handlePaymentError = () => setPaymentState({ success: false, cancel: false, failure: true })
  const handlePaymentCancel = () => setPaymentState({ success: false, cancel: true, failure: false })
  const handlePaymentSuccess = (payment) => {
    const { paid } = payment
    if (paid) {
      setPaymentState({success: true, cancel: false, failure: false})
    }

    dispatch(updateReferee({...updatedReferee, submittedPaymentAt: DateTime.local().toString()}, id))
  }

  const renderPaymentMessage = () => {
    const { failure, success, cancel } = paymentState
    if (!failure && !success && !cancel) return null

    const successMessage = 'Your payment was successful.'
    const errorMessage = 'There was an issue with your payment, please try again.'
    const cancelMessage = 'Your payment was cancelled.'

    let messageProps
    if (failure) {
      messageProps = { error: true }
    } else if (success) {
      messageProps = { positive: true }
    } else if (cancel) {
      messageProps = { warning: true }
    }

    return (
      <Message {...messageProps} size="small" onDismiss={clearPaymentState}>
        <p>
          {success && successMessage}
          {failure && errorMessage}
          {cancel && cancelMessage}
        </p>
      </Message>
    )
  }

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
          , please review and accept no later than June 1st, 2020.
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
      {renderPaymentMessage()}
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
          {referee.isEditable && !isEditing && (
            <div className="flex flex-col w-full lg:w-1/2 xl:w-1/2 rounded-lg bg-gray-100 p-4 lg:ml-8 xl:ml-8">
              <div className="flex justify-between items-center mb-4">
                <h3 className="border-b-2 border-green text-xl text-center">
                  Certifications
                </h3>
                <button type="button" className="border-2 border-green text-green text-center px-4 py-2 rounded bg-white">
                  Take Tests
                </button>
              </div>
              <TestResultCards testResults={testResults} />
            </div>
          )}
        </div>
      </div>
    </>
  );
}

export default RefereeProfile
