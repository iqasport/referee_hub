import { loadStripe } from "@stripe/stripe-js";
import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";

import { getCertifications } from "MainApp/modules/certification/certifications";
import { createSession } from "MainApp/modules/checkout/checkout";
import { getProducts } from "MainApp/modules/checkout/products";
import { fetchCurrentUser } from "MainApp/modules/currentUser/currentUser";
import { RootState } from "MainApp/rootReducer";
import { Datum } from "MainApp/schemas/getCertificationsSchema";
import { GetProductsSchema } from "MainApp/schemas/getProductsSchema";

import Loader from "../../Loader";
import Modal, { ModalSize } from "../Modal/Modal";

interface CheckoutModalProps {
  refId: string;
  open: boolean;
  onClose: () => void;
}

const CheckoutModal = (props: CheckoutModalProps) => {
  const [isRedirect, setIsRedirect] = useState(false);
  const dispatch = useDispatch();
  const { allProducts, error: productsError } = useSelector(
    (state: RootState) => state.products,
    shallowEqual
  );
  const { certifications } = useSelector((state: RootState) => state.certifications, shallowEqual);
  const { sessionId, isLoading } = useSelector((state: RootState) => state.checkout, shallowEqual);
  const { certificationPayments, currentUser } = useSelector(
    (state: RootState) => state.currentUser,
    shallowEqual
  );

  const products = allProducts.filter((product) => (product.name as string).includes("Referee Exam Fee"));

  const showLoader = isLoading || isRedirect;
  const hasPaidForAllCerts = (): boolean => {
    const unpaidProducts = products.filter((product) => {
      const certificationId = findCertification(product)?.id;
      return !certificationPayments.includes(parseInt(certificationId, 10));
    });

    return !unpaidProducts.length;
  };

  useEffect(() => {
    if (!products?.length && !productsError) {
      dispatch(getProducts());
    }
  }, [products]);

  useEffect(() => {
    if (!certifications?.length) {
      dispatch(getCertifications());
    }
  }, [certifications]);

  useEffect(() => {
    if (!currentUser) {
      dispatch(fetchCurrentUser());
    }
  }, [currentUser]);

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

  const findCertification = (product: GetProductsSchema): Datum => {
    let version = "twentytwo";
    if (product.name.match(/2018-/)) version = "eighteen";
    else if (product.name.match(/2020-/)) version = "twenty";

    return certifications?.find(
      ({ attributes }) => attributes.level === "head" && attributes.version === version
    );
  };

  const handleCheckout = (product: GetProductsSchema) => () => {
    const certificationId = findCertification(product)?.id;
    const price = product.prices[0].id;

    dispatch(createSession({ certificationId, price }));
  };

  const renderProduct = (product: GetProductsSchema) => {
    if (!product.active) return null;

    const certificationId = findCertification(product)?.id;
    if (certificationPayments.includes(parseInt(certificationId, 10))) return null;

    const price = product.prices[0];
    const formattedPrice = `$${price.unit_amount / 100} ${price.currency}`;

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
    );
  };

  return (
    <Modal open={props.open} showClose={true} size={ModalSize.Large} onClose={props.onClose}>
      <h1 className="font-bold text-xl text-navy-blue text-center">Head Ref Certifications</h1>
      {!showLoader && products?.map(renderProduct)}
      {hasPaidForAllCerts() && (
        <h2 className="text-center text-lg text-navy-blue my-8">
          All certifications paid for, you may close this window
        </h2>
      )}
      {showLoader && <Loader />}
      {productsError && (
        <h2 className="text-center text-lg text-navy-blue my-8">{productsError}</h2>
      )}
    </Modal>
  );
};

export default CheckoutModal;
