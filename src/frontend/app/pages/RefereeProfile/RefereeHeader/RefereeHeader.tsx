import { faUser } from "@fortawesome/free-regular-svg-icons";
import { faMapPin } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { capitalize } from "lodash";
import React, { useEffect, useState } from "react";
import { PickByValue } from "utility-types";

import Toggle from "../../../components/Toggle";
import { getRefereeCertVersion } from "../../../utils/certUtils";
import { toDateTime } from "../../../utils/dateUtils";
import HeaderButtons from "./HeaderButtons";
import HeaderImage from "./HeaderImage";
import HeaderName from "./HeaderName";
import { Certification, useGetUserAvatarQuery, useGetUserDataQuery, useUpdateCurrentUserDataMutation } from "../../../store/serviceApi";
import { useNavigationParams } from "../../../utils/navigationUtils";

type HeaderProps = {
  name: string;
  certifications: Certification[];
  isEditable: boolean;
};

const RefereeHeader = (props: HeaderProps) => {
  const { refereeId } = useNavigationParams<"refereeId">();
  const { certifications, name, isEditable } = props;

  const { data: user, error: getUserError } = useGetUserDataQuery({ userId: refereeId });
  const { data: userAvatar, error: getUserAvatarError } = useGetUserAvatarQuery({ userId: refereeId });
  const [updateUser, { error: updateUserError }] = useUpdateCurrentUserDataMutation();

  const [isEditing, setIsEditing] = useState(false);
  const [editableUser, setUser] = useState(user);

  // update editable user once the API returns
  useEffect(() => {
    setUser(user)
  }, [user]);

  const onEditClick = () => setIsEditing(true);
  const onSubmit = () => {
    setIsEditing(false);
    updateUser({userDataViewModel: editableUser})
  }
  const onCancel = () => {
    setIsEditing(false);
    setUser(user);
  }

  const handleStringChange = (stateKey: keyof PickByValue<typeof user, string>) => (
    event: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>
  ) => {
    const value = event.currentTarget.value;
    setUser({...editableUser, [stateKey]: value});
  };

  const handleToggleChange = (stateKey: keyof PickByValue<typeof user, boolean>) => (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.currentTarget.checked;
    setUser({...editableUser, [stateKey]: value});
  };

  const renderCertifications = () => {
    if (isEditing) return null;

    return certifications.map((certification) => (
      <div
        key={`${certification.level}-${certification.version}`}
        className="bg-white text-green border border-green py-2 px-6 rounded mr-5 mt-4"
      >
        {`${capitalize(certification.level)} (${getRefereeCertVersion(certification)})`}
      </div>
    ));
  };

  const renderPronouns = (): JSX.Element | null => {
    if (!isEditing && user.showPronouns) {
      return (
        <h2 className="text-l">
          <FontAwesomeIcon icon={faUser} className="mr-2" />
          {user.pronouns}
        </h2>
      );
    } else if (isEditing) {
      return (
        <div className="flex items-center">
          <Toggle
            name="showPronouns"
            label="Show Pronouns?"
            onChange={handleToggleChange("showPronouns")}
            checked={editableUser.showPronouns}
          />
          <input
            className="form-input"
            type="text"
            value={editableUser.pronouns ?? ""}
            onChange={handleStringChange("pronouns")}
            placeholder="Pronouns"
          />
        </div>
      );
    } else {
      return null;
    }
  };

  const renderJoined = (): JSX.Element | null => {
    if (isEditing) return null;

    const joinDate = toDateTime(user.createdAt).year;
    return (
      <h2 className="text-l">
        <FontAwesomeIcon className="mr-2" icon={faMapPin} />
        {`Joined ${joinDate}`}
      </h2>
    );
  };

  const renderBio = () => {
    if (!isEditing) {
      return user.bio;
    } else {
      return (
        <textarea
          aria-multiline="true"
          className="bg-gray-200 rounded p-4 text-lg block w-full mb-4"
          style={{ resize: "none" }}
          onChange={handleStringChange("bio")}
          value={editableUser.bio ?? ""}
          placeholder="Bio"
        />
      );
    }
  };

  if (!user) return <></>;

  // TODO: render errors
  return (
    <div className="flex flex-col lg:flex-row xl:flex-row">
      <HeaderImage avatarUrl={userAvatar as string} id={refereeId} isEditable={isEditable} />
      <div className="w-5/6 ml-8">
        <div className="flex flex-col items-center my-8 md:flex-row lg:flex-row xl:flex-row">
          <div className="flex-shrink w-full lg:mr-5 xl:mr-5 md:w-2/3 lg:w-2/3 xl:w-2/3">
            {
              <HeaderName
                isEditing={isEditing}
                onChange={handleStringChange}
                onToggleChange={handleToggleChange}
                updatedValues={editableUser}
                originalValues={user}
                name={name}
              />
            }
          </div>
          <div className="flex items-center flex-wrap">{renderCertifications()}</div>
          <div className="justify-end hidden md:flex lg:flex xl:flex">
            {isEditable && (
              <HeaderButtons
                isEditing={isEditing}
                onEdit={onEditClick}
                onSubmit={onSubmit}
                onCancel={onCancel}
              />
            )}
          </div>
        </div>
        <div className="flex mb-8 justify-between w-56">
          {renderJoined()}
          {renderPronouns()}
        </div>
        <div className="text-2xl mb-4">{renderBio()}</div>
      </div>
    </div>
  );
};

export default RefereeHeader;
