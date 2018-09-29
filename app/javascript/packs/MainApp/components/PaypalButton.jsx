import React from 'react'
import ReactDOM from 'react-dom'
import PropTypes from 'prop-types'
import paypal from 'paypal-checkout'

const PAYPAL_CLIENT = {
  sandbox: process.env.PAYPAL_CLIENT_ID_SANDBOX,
  production: process.env.PAYPAL_CLIENT_ID_PRODUCTION
};

const PayPalButton = paypal.Button.driver('react', { React, ReactDOM })

class PaypalButton extends React.Component {
  static propTypes = {
    env: PropTypes.string,
    commit: PropTypes.bool,
    onSuccess: PropTypes.func.isRequired,
    onError: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired
  }

  static defaultProps = {
    commit: true,
    env: process.env.NODE_ENV === 'production' ? 'production' : 'sandbox'
  }

  handlePayment = () => {
    const { env } = this.props

    return paypal.rest.payment.create(env, PAYPAL_CLIENT, {
      transactions: [
        {
          amount: {
            total: '15.00',
            currency: 'EUR'
          }
        }
      ]
    })
  }

  handleAuthorize = (data, actions) => {
    const { onSuccess } = this.props

    actions.payment.execute()
      .then(() => {
        const payment = {
          paid: true,
          cancelled: false,
          payerID: data.payerID,
          paymentID: data.paymentID,
          paymentToken: data.paymentToken,
          returnUrl: data.returnUrl,
        }

        onSuccess(payment)
      })
  }

  render() {
    const {
      env,
      commit,
      onError,
      onCancel
    } = this.props;

    return (
      <div>
        <PayPalButton
          env={env}
          client={PAYPAL_CLIENT}
          commit={commit}
          payment={this.handlePayment}
          onAuthorize={this.handleAuthorize}
          onCancel={onCancel}
          onError={onError}
        />
      </div>
    )
  }
}

export default PaypalButton
