import React, { useState } from "react";

import CheckoutModal from "../../components/modals/CheckoutModal";
import RefereeTestsTable from "../../components/tables/RefereeTestsTable";
import Toast from "../../components/Toast";
import { useNavigationParams } from "../../utils/navigationUtils";

type IdParams = {
  refereeId: string;
};

const RefereeTests = () => {
  const { refereeId } = useNavigationParams<"refereeId">();
  if (refereeId !== "me") {
    return <p>Cannot view tests for another referee.</p>;
  }
  const [checkoutOpen, setCheckoutOpen] = useState(false);
  const paymentStatus = new URLSearchParams(window.location.search).get("paymentStatus");
  
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
        <RefereeTestsTable refId={refereeId} />
      </div>
      <CheckoutModal open={checkoutOpen} onClose={handleCheckoutClose} />
    </div>
  );
};

export default RefereeTests;
