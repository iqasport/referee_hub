import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { RouteComponentProps } from "react-router-dom";

import CheckoutModal from "MainApp/components/modals/CheckoutModal";
import RefereeTestsTable from "MainApp/components/tables/RefereeTestsTable";
import Toast from "MainApp/components/Toast";
import { fetchReferee, RefereeState } from "MainApp/modules/referee/referee";
import { RootState } from "MainApp/rootReducer";

type IdParams = {
  refereeId: string;
};

const RefereeTests = (props: RouteComponentProps<IdParams>) => {
  const {
    match: {
      params: { refereeId },
    },
  } = props;
  const [checkoutOpen, setCheckoutOpen] = useState(false);
  const [paymentStatus, setPaymentStatus] = useState<string>(null);
  const dispatch = useDispatch();
  const { referee, id } = useSelector(
    (state: RootState): RefereeState => state.referee,
    shallowEqual
  );

  useEffect(() => {
    if (!referee || id !== refereeId) {
      dispatch(fetchReferee(refereeId));
    }
  }, [refereeId]);

  useEffect(() => {
    const [, status] = window.location.href.split("=");
    if (status !== paymentStatus) {
      setPaymentStatus(status);
    }
  }, [paymentStatus]);

  const handleCheckoutOpen = () => setCheckoutOpen(true);
  const handleCheckoutClose = () => setCheckoutOpen(false);

  const renderPaymentMessage = () => {
    const paymentText = paymentStatus === "success" ? "successful" : "cancelled";

    return <Toast message={`Your head referee certification test payment was ${paymentText}`} />;
  };

  return (
    <div className="w-full relative">
      {paymentStatus && renderPaymentMessage()}
      <div className="w-5/6 mx-auto my-8">
        <div className="w-full flex justify-between items-center my-8">
          <h1 className="text-4xl font-extrabold">Certifications</h1>
          <div>
            <button className="bg-green text-white rounded py-2 px-4" onClick={handleCheckoutOpen}>
              Submit Head Referee Payment
            </button>
          </div>
        </div>
        <p>
          Information: If you have previous certification for rulebook 2020-2022 you may see a recertification test.
          There will be a single recertification test for the highest certification you received.
          There's only 1 attempt at recertification. Passing will grant you all previously held certifications.
          If you've previously held HR certification but feel you want to recertify for AR or SR only, please contact the IQA.
          After a failed attempt at recertification, initial certification tests for rulebook 2022-2023 will be shown.
        </p>
        <RefereeTestsTable refId={refereeId} />
      </div>
      <CheckoutModal open={checkoutOpen} refId={refereeId} onClose={handleCheckoutClose} />
    </div>
  );
};

export default RefereeTests;
