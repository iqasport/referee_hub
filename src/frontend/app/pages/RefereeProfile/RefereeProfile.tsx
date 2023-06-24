import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { useParams, useNavigate } from "react-router-dom";

import { AssociationData, UpdateRefereeRequest } from "../../apis/referee";
import AdminCertificationsModal from "../../components/modals/AdminCertificationsModal";
import TestResultCards from "../../components/TestResultCards";
import { updateUserPolicy } from "../../modules/currentUser/currentUser";
import { fetchReferee, updateReferee } from "../../modules/referee/referee";
import { RootState } from "../../rootReducer";

import RefereeHeader from "./RefereeHeader";
import RefereeLocation from "./RefereeLocation";
import RefereeTeam from "./RefereeTeam";
import { IdParams } from "./types";
import { useGetRefereeQuery, useGetTestAttemptsQuery, useUpdateCurrentRefereeMutation } from "../../store/serviceApi";
import { RefereeLocationOptions } from "./RefereeLocation/RefereeLocation";
import { RefereeTeamOptions } from "./RefereeTeam/RefereeTeam";

const RefereeDetails = () => {
  const { id } = useParams<IdParams>();
  const [isEditing, setIsEditing] = useState(false);

  const { currentData: referee } = useGetRefereeQuery({ userId: id });
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
      { id == "me" && <button
        type="button"
        className="border-2 border-green text-green text-center px-4 py-2 rounded bg-white"
        onClick={buttonClick}
      >
        { isEditing ? "Save" : "Edit" }
      </button>}
    </div>
    { updateRefereeError && <div>
      Error: {updateRefereeError.toString()}
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
      }}
      locations={{
        primaryNgb: editableReferee.primaryNgb,
        secondaryNgb: editableReferee.secondaryNgb,
      }}
      isEditing={isEditing}
      onChange={handleChange}
    />
  </div>);
}

const RefereeProfile = () => {
  const navigate = useNavigate();
  const { id } = useParams<IdParams>();

  //const [isCertificationModalOpen, setIsCertificationModalOpen] = useState(false);
  
  const { currentData: referee, error: refereeGetError } = useGetRefereeQuery({ userId: id });
  const { data: testAttempts, error: testAttemptsError } = useGetTestAttemptsQuery(undefined, {skip: id !== "me"})

  if (refereeGetError) return <p style={{color: "red"}}>{refereeGetError.toString()}</p>;
  if (!referee) return null;

  const isCertificationsVisible = id === "me"; // TODO || isIqaAdmin;
  //const handleCertificationModalClose = () => setIsCertificationModalOpen(false);

  return (
    <>
      <div className="m-auto w-full my-10 px-4 xl:w-3/4 xl:px-0">
        <RefereeHeader
          name={referee.name}
          certifications={referee.acquiredCertifications}
          isEditable={id === "me"}
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
                  onClick={() => navigate(`/referees/${id}/tests`)}
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
