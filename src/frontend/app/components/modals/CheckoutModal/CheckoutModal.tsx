import { loadStripe } from "@stripe/stripe-js";
import React, { useEffect, useState } from "react";

import Loader from "../../Loader";
import Modal, { ModalSize } from "../Modal/Modal";
import { getErrorString } from "../../../utils/errorUtils";
import { CertificationProduct, useCreatePaymentSessionMutation, useGetAvailablePaymentsQuery } from "../../../store/serviceApi";

interface CheckoutModalProps {
  open: boolean;
  onClose: () => void;
}

const CheckoutModal = (props: CheckoutModalProps) => {
  const [isRedirect, setIsRedirect] = useState(false);

  const { currentData: products, error: getProductsError, isLoading: isProductsLoading } = useGetAvailablePaymentsQuery();
  const productsError = getErrorString(getProductsError);
  
  const [createCheckoutSession, {data: sessionResponse, error: createSessionError, isLoading: isCreateSessionLoading}] = useCreatePaymentSessionMutation();
  const sessionId = sessionResponse?.sessionId;

  const showLoader = isProductsLoading || isCreateSessionLoading || isRedirect;
 
  useEffect(() => {
    if (sessionId) {
      setIsRedirect(true);
      loadStripe(process.env.STRIPE_PUBLISHABLE_KEY)
        .then((stripe) => {
          stripe.redirectToCheckout({
            sessionId,
          });
        })
        .catch((error) => {
          // tslint:disable-next-line
          console.error(error);
          setIsRedirect(false);
        })
        .finally(() => {
          setIsRedirect(false);
        });
    }
  }, [sessionId]);

  const handleCheckout = (product: CertificationProduct) => () => {
    createCheckoutSession(product.item);
  };

  const renderProduct = (product: CertificationProduct) => {
    const price = product.price;
    const formattedPrice = `$${price.unitPrice} ${price.currency}`;
    const available = product.status === "Available";

    return (
      <div key={product.price.priceId} className="my-8 p-4 flex justify-between bg-white rounded">
        <div className="w-2/3 flex flex-col">
          <p className={`font-semibold text-lg ${available ? "text-navy-blue" : "text-gray-500"}`}>{product.displayName}</p>
          <p className="font-light text-sm text-gray-500">{formattedPrice}</p>
        </div>
        <button
          className={`py-2 px-4 ${available ? "bg-blue-darker" : "bg-green-lighter"} text-white rounded`}
          disabled={!available}
          onClick={available ? handleCheckout(product) : () => {}}
        >
          { available ? "Checkout" : "Purchased" }
        </button>
      </div>
    );
  };

  return (
    <Modal open={props.open} showClose={true} size={ModalSize.Large} onClose={props.onClose}>
      <h1 className="font-bold text-xl text-navy-blue text-center">Head Ref Certifications</h1>
      {!showLoader && products?.map(renderProduct)}
      {showLoader && <Loader />}
      {productsError && (
        <h2 className="text-center text-lg text-navy-blue my-8">{productsError}</h2>
      )}
      {createSessionError && (
        <h2 className="text-center text-lg text-navy-blue my-8">{getErrorString(createSessionError)}</h2>
      )}
    </Modal>
  );
};

export default CheckoutModal;
