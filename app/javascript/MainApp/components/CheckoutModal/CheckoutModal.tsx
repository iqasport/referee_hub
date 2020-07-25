import { loadStripe } from '@stripe/stripe-js';
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux';

import { getCertifications } from 'MainApp/modules/certification/certifications';
import { createSession } from 'MainApp/modules/checkout/checkout';
import { getProducts } from 'MainApp/modules/checkout/products';
import { RootState } from 'MainApp/rootReducer';
import { GetProductsSchema } from 'MainApp/schemas/getProductsSchema';

import Loader from '../Loader';
import Modal, { ModalSize } from '../Modal/Modal';

interface CheckoutModalProps {
  refId: string;
  open: boolean;
}

const CheckoutModal = (props: CheckoutModalProps) => {
  const [isRedirect, setIsRedirect] = useState(false)
  const dispatch = useDispatch()
  const { products } = useSelector((state: RootState) => state.products, shallowEqual)
  const { certifications } = useSelector((state: RootState) => state.certifications, shallowEqual)
  const { sessionId, isLoading } = useSelector((state: RootState) => state.checkout, shallowEqual)

  const showLoader = isLoading || isRedirect

  useEffect(() => {
    if (!products?.length) {
      dispatch(getProducts())
    }
  }, [products])

  useEffect(() => {
    if (!certifications?.length) {
      dispatch(getCertifications())
    }
  }, [certifications])

  useEffect(() => {
    if (sessionId) {
      setIsRedirect(true)
      loadStripe(
        process.env.STRIPE_PUBLISHABLE_KEY
      ).then((stripe) => {
        stripe.redirectToCheckout({
          sessionId
        })
      }).catch((error) => {
        // tslint:disable-next-line
        console.error(error)
        setIsRedirect(false)
      }).finally(() => {
        setIsRedirect(false)
      })
    }
  }, [sessionId])

  const findCertification = (product: GetProductsSchema) => {
    let version = 'twenty'
    if (product.name.match(/2018/)) version = 'eighteen'

    return certifications.find(({ attributes }) => attributes.level === 'head' && attributes.version === version)
  }

  const handleCheckout = (product: GetProductsSchema) => () => {
    const certificationId = findCertification(product)?.id
    const price = product.prices[0].id

    dispatch(createSession({ certificationId, price }))
  }

  const renderProduct = (product: GetProductsSchema) => {
    const price = product.prices[0]
    const formattedPrice = `$${price.unit_amount / 100} ${price.currency}`

    return (
      <div key={product.id} className="my-8 p-4 flex justify-between bg-white rounded">
        <div className="w-2/3 flex flex-col">
          <p className="font-semibold text-lg text-navy-blue">{product.name}</p>
          <p className="font-light text-sm text-gray-500">{formattedPrice}</p>
        </div>
        <button
          className="py-2 px-4 bg-blue-darker text-white rounded"
          onClick={handleCheckout(product)}
        >
          Checkout
        </button>
      </div>
    )
  }

  return (
    <Modal open={props.open} showClose={false} size={ModalSize.Large}>
      <div>
        <h1 className="font-bold text-xl text-navy-blue text-center">Head Ref Certifications</h1>
        {!showLoader && products.map(renderProduct)}
        {showLoader && <Loader />}
      </div>
    </Modal>
  )
}

export default CheckoutModal
