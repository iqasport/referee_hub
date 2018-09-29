import React from 'react'
import ReactDOM from 'react-dom'
import PropTypes from 'prop-types'
import scriptLoader from 'react-async-script-loader'

class PaypalButton extends React.Component {
  static propTypes = {
    isScriptLoaded: PropTypes.bool.isRequired,
    isScriptLoadSucceed: PropTypes.bool.isRequired,
    currencyConfig: PropTypes.arrayOf(PropTypes.shape({
      total: PropTypes.number,
      currency: PropTypes.string
    })).isRequired,
    client: PropTypes.string.isRequired,
    env: PropTypes.string.isRequired,
    commit: PropTypes.bool,
    onSuccess: PropTypes.func.isRequired,
    onError: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired
  }

  static defaultProps = {
    commit: true
  }

  constructor(props) {
    super(props)

    this.state = { showButton: false }

    window.React = React // eslint-disable-line
    window.ReactDOM = ReactDOM // eslint-disable-line
  }

  componentDidMount() {
    const { isScriptLoaded, isScriptLoadSucceed } = this.props

    if (isScriptLoaded && isScriptLoadSucceed) {
      this.setState({ showButton: true });
    }
  }

  componentWillReceiveProps(nextProps) {
    const { showButton } = this.state
    const { isScriptLoaded, isScriptLoadSucceed } = nextProps
    const { isScriptLoaded: oldScriptLoaded } = this.props

    const isLoadedButWasntLoadedBefore = !showButton && !oldScriptLoaded && isScriptLoaded

    if (isLoadedButWasntLoadedBefore) {
      if (isScriptLoadSucceed) {
        this.setState({ showButton: true })
      }
    }
  }

  handlePayment = () => {
    const {
      currencyConfig,
      env,
      client
    } = this.props

    /* eslint-disable no-undef */
    return paypal.rest.payment.create(env, client, {
      transactions: [
        currencyConfig.map(({ total, currency }) => ({ amount: { total, currency } }))
      ],
    });
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
    const { showButton } = this.state
    const {
      env,
      commit,
      client,
      onError,
      onCancel
    } = this.props;

    /* eslint-disable react/jsx-no-undef */
    return (
      <div>
        {
          showButton
          && (
            <paypal.Button.react
              env={env}
              client={client}
              commit={commit}
              payment={this.handlePayment}
              onAuthorize={this.handleAuthorize}
              onCancel={onCancel}
              onError={onError}
            />
          )
        }
      </div>
    )
  }
}

export default scriptLoader('https://www.paypalobjects.com/api/checkout.js')(PaypalButton)
