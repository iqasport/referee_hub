import {
  faCheckCircle,
  faCircleNotch,
  faExclamationCircle,

} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import { Datum, Meta } from "../../schemas/getTeamsSchema";

interface FinishStepProps {
  meta: Meta;
  error?: string;
  dataType: string;
}

const FinishStep = (props: FinishStepProps) => {
  const { meta, error, dataType } = props;

  const renderError = () => {
    return (
      <>
        <FontAwesomeIcon icon={faExclamationCircle} size="6x" className="text-red-500" />
        <h3 className="text-2xl font-bold my-8">Uh oh! There was an error importing your data</h3>
        <p>
          Please check your file and retry or reach out to
          <a href="mailto:refhub@iqasport.org" className="text-blue-darker underline ml-1">
            refhub@iqasport.org
          </a>
        </p>
      </>
    );
  };

  const renderSuccess = () => {
    return (
      <>
        <FontAwesomeIcon icon={faCheckCircle} size="6x" className="text-green" />
        <h3 className="text-2xl font-bold my-8">Success!</h3>
        <p className="text-xl font-semibold mb-4">{`${meta.total} ${dataType} were imported`}</p>
        <p>Click the Done button to go back to your home screen.</p>
      </>
    );
  };

  const renderLoading = () => (
    <FontAwesomeIcon
      icon={faCircleNotch}
      spin={true}
      pulse={true}
      size="6x"
      className="text-green"
    />
  );

  const renderContent = () => {
    if (error) {
      return renderError();
    } else if (meta) {
      return renderSuccess();
    } else {
      return renderLoading();
    }
  };

  return (
    <div className="w-1/2 mx-auto my-4 py-12">
      <div className="text-center">{renderContent()}</div>
    </div>
  );
};

export default FinishStep;
