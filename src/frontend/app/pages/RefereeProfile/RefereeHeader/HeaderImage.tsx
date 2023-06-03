import React from "react";
import { useDispatch } from "react-redux";

import UploadedImage from "../../../components/UploadedImage";
import { updateUserAvatar } from "../../../modules/currentUser/currentUser";
import { AppDispatch } from "../../../store";

type HeaderImageProps = {
  avatarUrl: string;
  id: string;
  isEditable: boolean;
};

const HeaderImage = (props: HeaderImageProps) => {
  const { avatarUrl, id, isEditable } = props;
  const dispatch = useDispatch<AppDispatch>();

  const handleSubmit = (file: File) => {
    dispatch(updateUserAvatar(id, file));
  };

  return (
    <UploadedImage
      imageAlt="referee avatar"
      imageUrl={avatarUrl}
      onSubmit={handleSubmit}
      isEditable={isEditable}
    />
  );
};

export default HeaderImage;
