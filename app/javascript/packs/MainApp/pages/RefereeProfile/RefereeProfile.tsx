import { DateTime } from 'luxon'
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'
import { RouteComponentProps, useHistory } from 'react-router-dom'
import {
  Button, Loader, Message, Tab
} from 'semantic-ui-react'

import CertificationContent from '../../components/CertificationContent'
import ProfileContent from '../../components/ProfileContent'
import { updateUserPolicy } from '../../modules/currentUser/currentUser'
import { fetchReferee } from '../../modules/referee/getReferee';
import { RootState } from '../../rootReducer';
import RefereeHeader from './RefereeHeader'

type IdParams = { id: string }
// state = {
//   httpStatus: 0,
//   httpStatusText: '',
//   paymentError: false,
//   paymentSuccess: false,
//   paymentCancel: false,
//   changedFirstName: null,
//   changedLastName: null,
//   changedNGBs: null,
//   changedBio: null,
//   changedShowPronouns: null,
//   changedPronouns: null,
//   policyAccepted: false
// }
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

const RefereeProfile = (props: RouteComponentProps<IdParams>) => {
  const { match: { params: { id }}} = props
  const dispatch = useDispatch()
  const history = useHistory();
  const { isLoading, referee, certifications, error, ngbs, testAttempts, testResults } = useSelector((state: RootState) => {
    return {
      certifications: state.referee.certifications,
      error: state.referee.error,
      isLoading: state.referee.isLoading,
      ngbs: state.referee.ngbs,
      referee: state.referee.referee,
      testAttempts: state.referee.testAttempts,
      testResults: state.referee.testResults,
    }
  }, shallowEqual)
  const [paymentState, setPaymentState] = useState<PaymentState>(initialPaymentState)
  
  useEffect(() => {
    if(id) {
      dispatch(fetchReferee(id))
    }
  }, [id, dispatch])

  if (!referee) return null
  // get refereeEditValues() {
  //   const {
  //     referee, changedFirstName, changedLastName, changedNGBs, changedBio, changedShowPronouns, changedPronouns
  //   } = this.state

  //   return {
  //     firstName: changedFirstName || referee.firstName,
  //     lastName: changedLastName || referee.lastName,
  //     nationalGoverningBodies: changedNGBs || referee.nationalGoverningBodies,
  //     bio: changedBio || referee.bio,
  //     showPronouns: changedShowPronouns || referee.showPronouns,
  //     pronouns: changedPronouns || referee.pronouns
  //   }
  // }


  const handleRouteChange = (newRoute) => history.push(newRoute)

  const handleSubmit = () => {
    // const {
    //   referee: {
    //     firstName,
    //     lastName,
    //     bio,
    //     showPronouns,
    //     pronouns,
    //     nationalGoverningBodies
    //   },
    //   changedFirstName,
    //   changedLastName,
    //   changedNGBs,
    //   changedBio,
    //   changedShowPronouns,
    //   changedPronouns
    // } = state

    // const ngbState = changedNGBs
    //   ? changedNGBs.map(ngbId => Number(ngbId))
    //   : nationalGoverningBodies.map(ngb => Number(ngb.id))

    // axios
    //   .patch(this.currentRefereeApiRoute, {
    //     first_name: changedFirstName || firstName,
    //     last_name: changedLastName || lastName,
    //     bio: changedBio || bio,
    //     show_pronouns: changedShowPronouns || showPronouns,
    //     pronouns: changedPronouns || pronouns,
    //     national_governing_body_ids: ngbState
    //   })
    //   .then(this.setComponentStateFromBackendData)
    //   .catch(this.setErrorStateFromBackendData)
  };

  const handlePaymentSuccess = (payment) => {
    const { paid } = payment
    if (paid) {
      setPaymentState({success: true, cancel: false, failure: false})
    }
    //   axios
    //     .patch(this.currentRefereeApiRoute, {
    //       submitted_payment_at: DateTime.local().toString()
    //     })
    //     .then(this.setComponentStateFromBackendData)
    //     .catch(this.setErrorStateFromBackendData)
    // }
  }

  const handlePaymentError = () => {
    setPaymentState({ success: false, cancel: false, failure: true });
  }

  const handlePaymentCancel = () => {
    setPaymentState({ success: false, cancel: true, failure: false });
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

  const clearPaymentState = () => {
    setPaymentState(initialPaymentState)
  }

  const handleGetStartedDismiss = () => {
    // axios
    //   .patch(this.currentRefereeApiRoute, {
    //     getting_started_dismissed_at: DateTime.local().toString()
    //   })
    //   .then(this.setComponentStateFromBackendData)
    //   .catch(this.setErrorStateFromBackendData)
  }

  const handleInputChange = (stateKey: string, value: string) => {
    // this.setState({ [stateKey]: value })
  }

  const handleAcceptPolicy = () => dispatch(updateUserPolicy(id, 'accept'))
  const handleRejectPolicy = () => dispatch(updateUserPolicy(id, 'reject'))

  const renderProfileContent = () => {
    let content
    if (error) {
      content = <h1>{error}</h1>
    } else {
      content = <ProfileContent referee={referee} onDismiss={handleGetStartedDismiss} />
    }

    return (
      <Tab.Pane>
        {isLoading && <Loader active={true} />}
        {content}
      </Tab.Pane>
    )
  }

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
      </div>
    </>
  )
}

export default RefereeProfile
