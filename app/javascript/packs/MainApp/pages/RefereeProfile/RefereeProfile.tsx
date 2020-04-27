import { DateTime } from 'luxon'
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps, useHistory } from 'react-router-dom'
import { Message } from 'semantic-ui-react'

import { AssociationData, UpdateRefereeRequest } from '../../apis/referee';
import { updateUserPolicy } from '../../modules/currentUser/currentUser'
import { fetchReferee, RefereeState, updateReferee } from '../../modules/referee/referee';
import { RootState } from '../../rootReducer';
import { DataAttributes, IncludedAttributes } from '../../schemas/getRefereeSchema';
import RefereeHeader from './RefereeHeader'
import RefereeLocation from './RefereeLocation'
import RefereeTeam from './RefereeTeam'

type IdParams = { id: string }
const selectRefereeState = (state: RootState): RefereeState => {
  return {
    certifications: state.referee.certifications,
    error: state.referee.error,
    id: state.referee.id,
    isLoading: state.referee.isLoading,
    locations: state.referee.locations,
    ngbs: state.referee.ngbs,
    referee: state.referee.referee,
    teams: state.referee.teams,
    testAttempts: state.referee.testAttempts,
    testResults: state.referee.testResults,
  };
}

type PaymentState = {
  success: boolean;
  cancel: boolean;
  failure: boolean;
}

const initialPaymentState: PaymentState = {
  cancel: false,
  failure: false,
  success: false,
}

const initialUpdateState = (referee: DataAttributes, locations: IncludedAttributes[], teams: IncludedAttributes[]): UpdateRefereeRequest => {
  const ngbData = locations.reduce((data, location): AssociationData => {
    data[location.nationalGoverningBodyId.toString()] = location.associationType
    return data
  }, {} as AssociationData)
  const teamsData = teams.reduce((data, team): AssociationData => {
    data[team.teamId.toString()] = team.associationType
    return data
  }, {} as AssociationData)

  return {
    bio: referee?.bio,
    exportName: referee?.exportName,
    firstName: referee?.firstName,
    lastName: referee?.lastName,
    ngbData,
    pronouns: referee?.pronouns,
    showPronouns: referee?.showPronouns,
    submittedPaymentAt: referee?.submittedPaymentAt,
    teamsData,
  }
}

const RefereeProfile = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id }}} = props
  const dispatch = useDispatch()
  const history = useHistory();
  const { referee, certifications, ngbs, locations, teams, id: stateId } = useSelector(selectRefereeState, shallowEqual)
  const [paymentState, setPaymentState] = useState<PaymentState>(initialPaymentState)
  const [updatedReferee, setUpdatedReferee] = useState<UpdateRefereeRequest>(initialUpdateState(referee, locations, teams))
  const [isEditing, setIsEditing] = useState(false)
  const [hasUpdated, setHasUpdated] = useState(false)

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
    setHasUpdated(true)
  }
  const handleAssociationChange = (value: AssociationData, stateKey: string) => {
    setUpdatedReferee({ ...updatedReferee, [stateKey]: value})
    setHasUpdated(true)
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
      <div className="m-auto w-3/4 mt-10">
        <RefereeHeader 
          referee={referee} 
          certifications={certifications} 
          onChange={handleInputChange} 
          onEditClick={handleEditClick} 
          isEditing={isEditing} 
          onSubmit={handleSubmit}
          updatedValues={updatedReferee}
          isSaveDisabled={!hasUpdated}
          id={stateId} />
        <div className="w-full border-b-2 border-navy-blue">
          <h3 className="text-xl">Details</h3>
        </div>
        <div className="flex">
          <RefereeLocation ngbs={ngbs} locations={locations} isEditing={isEditing} onChange={handleAssociationChange} value={updatedReferee.ngbData} />
          <RefereeTeam teams={teams} locations={locations} isEditing={isEditing} onChange={handleAssociationChange} value={updatedReferee.teamsData} isDisabled={locations.length < 1} />
        </div>
      </div>
    </>
  );
}

export default RefereeProfile
