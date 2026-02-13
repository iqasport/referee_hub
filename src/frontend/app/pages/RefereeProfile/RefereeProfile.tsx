import React, { useState } from "react";

import TestResultCards from "../../components/TestResultCards";

import RefereeHeader from "./RefereeHeader";
import RefereeLocation from "./RefereeLocation";
import RefereeTeam from "./RefereeTeam";
import { IdParams } from "./types";
import { useGetRefereeQuery, useGetTestAttemptsQuery, useUpdateCurrentRefereeMutation } from "../../store/serviceApi";
import { RefereeLocationOptions } from "./RefereeLocation/RefereeLocation";
import { RefereeTeamOptions } from "./RefereeTeam/RefereeTeam";
import { getErrorString } from "../../utils/errorUtils";
import { useNavigate, useNavigationParams } from "../../utils/navigationUtils";

const RefereeDetails = () => {
  const { refereeId } = useNavigationParams<"refereeId">();
  const [isEditing, setIsEditing] = useState(false);

  const { currentData: referee } = useGetRefereeQuery({ userId: refereeId });
  const [editableReferee, setReferee] = useState<RefereeLocationOptions & RefereeTeamOptions>(referee);
  const [updateReferee, { error: updateRefereeError }] = useUpdateCurrentRefereeMutation();

  const handleChange = (newState: RefereeLocationOptions | RefereeTeamOptions) => {
    setReferee({...editableReferee, ...newState});
  }

  const buttonClick = () => {
    if (isEditing) {
      setIsEditing(false);
      updateReferee({ refereeUpdateViewModel: editableReferee });
    }
    else {
      setIsEditing(true);
    }
  }

  return (<div className="flex flex-col w-full lg:w-1/2 xl:w-1/2 rounded-lg bg-gray-100 p-4 mb-8">
    <div className="flex justify-between items-center mb-4">
      <h3 className="border-b-2 border-green text-xl text-center">Details</h3>
      { refereeId == "me" && <button
        type="button"
        className="border-2 border-green text-green text-center px-4 py-2 rounded bg-white"
        onClick={buttonClick}
      >
        { isEditing ? "Save" : "Edit" }
      </button>}
    </div>
    { updateRefereeError && <div>
      Error: {getErrorString(updateRefereeError)}
    </div>}
    <RefereeLocation
      locations={{
        primaryNgb: editableReferee.primaryNgb,
        secondaryNgb: editableReferee.secondaryNgb,
      }}
      isEditing={isEditing}
      onChange={handleChange}
    />
    <RefereeTeam
      teams={{
        coachingTeam: editableReferee.coachingTeam,
        playingTeam: editableReferee.playingTeam,
        nationalTeam: editableReferee.nationalTeam,
      }}
      locations={{
        primaryNgb: editableReferee.primaryNgb,
        secondaryNgb: editableReferee.secondaryNgb,
      }}
      isEditing={isEditing}
      onChange={handleChange}
      isOwnProfile={refereeId === "me"}
    />
  </div>);
}

const RefereeProfile = () => {
  const navigate = useNavigate();
  const { refereeId } = useNavigationParams<"refereeId">();

  //const [isCertificationModalOpen, setIsCertificationModalOpen] = useState(false);
  
  const { currentData: referee, error: refereeGetError } = useGetRefereeQuery({ userId: refereeId });
  const { data: testAttempts, error: testAttemptsError } = useGetTestAttemptsQuery(undefined, {skip: refereeId !== "me"})

  if (refereeGetError) return <p style={{color: "red"}}>{getErrorString(refereeGetError)}</p>;
  if (!referee) return null;

  const isCertificationsVisible = refereeId === "me"; // TODO || isIqaAdmin;
  //const handleCertificationModalClose = () => setIsCertificationModalOpen(false);

  return (
    <>
      <div className="m-auto w-full my-10 px-4 xl:w-3/4 xl:px-0">
        <RefereeHeader
          name={referee.name}
          certifications={referee.acquiredCertifications}
          isEditable={refereeId === "me"}
        />
        <div className="flex flex-col lg:flex-row xl:flex-row w-full">
          <RefereeDetails />
          {isCertificationsVisible && testAttempts && (
            <div className="flex flex-col w-full lg:w-1/2 xl:w-1/2 rounded-lg bg-gray-100 p-4 lg:ml-8 xl:ml-8">
              <div className="flex justify-between items-center mb-4">
                <h3 className="border-b-2 border-green text-xl text-center">Certifications</h3>
                <button
                  type="button"
                  className="border-2 border-green text-green text-center px-4 py-2 rounded bg-white"
                  onClick={() => navigate(`/referees/${refereeId}/tests`)}
                >
                  {/* TODO - wrap it in a component to modify behavior based on admin/ref/viewer - isIqaAdmin && !referee.isEditable ? "Manage Certifications" : "Take Tests" */}
                  Take Tests
                </button>
              </div>
              <TestResultCards testResults={testAttempts} />
            </div>
          )}
        </div>
      </div>
      {/* <AdminCertificationsModal
        open={isCertificationModalOpen}
        refereeId={id}
        onClose={handleCertificationModalClose}
      /> */}
    </>
  );
};

export default RefereeProfile;
