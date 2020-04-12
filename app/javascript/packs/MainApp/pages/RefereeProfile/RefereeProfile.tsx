import { DateTime } from 'luxon'
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps, useHistory } from 'react-router-dom'
import {
  Loader, Message, Tab
} from 'semantic-ui-react'

import { AssociationData, UpdateRefereeRequest } from '../../apis/referee';
import CertificationContent from '../../components/CertificationContent'
import ProfileContent from '../../components/ProfileContent'
import { updateUserPolicy } from '../../modules/currentUser/currentUser'
import { fetchReferee, RefereeState, updateReferee } from '../../modules/referee/referee';
import { RootState } from '../../rootReducer';
import RefereeHeader from './RefereeHeader'
import RefereeLocation from './RefereeLocation'
import RefereeProfileEdit from '../../components/RefereeProfileEdit'
import { DataAttributes, IncludedAttributes } from '../../schemas/getRefereeSchema';

type IdParams = { id: string }
const selectRefereeState = (state: RootState): Omit<RefereeState, 'id'> => {
  return {
    certifications: state.referee.certifications,
    error: state.referee.error,
    isLoading: state.referee.isLoading,
    locations: state.referee.locations,
    ngbs: state.referee.ngbs,
    referee: state.referee.referee,
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

const initialUpdateState = (referee: DataAttributes, locations: IncludedAttributes[]) => {
  const ngbData = locations.reduce((data, location): AssociationData => {
    data[location.nationalGoverningBodyId.toString()] = location.associationType
    return data
  }, {} as AssociationData)

  return {
    bio: referee?.bio,
    exportName: referee?.exportName,
    ngbData,
    pronouns: referee?.pronouns,
    showPronouns: referee?.showPronouns,
    submittedPaymentAt: referee?.submittedPaymentAt,
    teamsData: {},
  }
}

const RefereeProfile = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id }}} = props
  const dispatch = useDispatch()
  const history = useHistory();
  const { isLoading, referee, certifications, error, ngbs, testAttempts, testResults, locations } = useSelector(selectRefereeState, shallowEqual)
  const [paymentState, setPaymentState] = useState<PaymentState>(initialPaymentState)
  const [updatedReferee, setUpdatedReferee] = useState<UpdateRefereeRequest>(initialUpdateState(referee, locations))
  const [isEditing, setIsEditing] = useState(false)

  useEffect(() => {
    if(id) {
      dispatch(fetchReferee(id))
    }
  }, [id, dispatch])

  if (!referee) return null

  const handleRouteChange = (newRoute) => history.push(newRoute)

  const handleSubmit = () => {
    dispatch(updateReferee(updatedReferee, id))
  };

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

  const handleInputChange = (value: string, stateKey: string) => {
    setUpdatedReferee({ ...updatedReferee, [stateKey]: value })
  }
  const handleAssociationChange = (value: AssociationData, stateKey: string) => {
    setUpdatedReferee({...updatedReferee, [stateKey]: value})
  }

  const handleAcceptPolicy = () => dispatch(updateUserPolicy(id, 'accept'))
  const handleRejectPolicy = () => dispatch(updateUserPolicy(id, 'reject'))

  const renderCertificationContent = () => {
    const { isEditable, submittedPaymentAt } = referee

    return (
      <Tab.Pane>
        <CertificationContent
          refereeId={id}
          isEditable={isEditable}
          hasPaid={!!submittedPaymentAt}
          testResults={testResults}
          testAttempts={testAttempts}
          refCertifications={certifications}
          onSuccess={handlePaymentSuccess}
          onError={handlePaymentError}
          onCancel={handlePaymentCancel}
          onRouteChange={handleRouteChange}
        />
      </Tab.Pane>
    )
  }

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
        <RefereeHeader referee={referee} certifications={certifications} />
        <div className="w-full border-b-2 border-navy-blue">
          <h3 className="text-xl">Details</h3>
        </div>
        <div className="flex">
          <RefereeLocation ngbs={ngbs} locations={locations} isEditing={isEditing} onChange={handleAssociationChange} value={updatedReferee.ngbData} />
        </div>
        {isEditing && (
          <button className="rounded border-green border-2 text-green p-4 cursor-pointer" onClick={handleSubmit}>Save Changes</button>
        )}
      </div>
    </>
  );
}

export default RefereeProfile
