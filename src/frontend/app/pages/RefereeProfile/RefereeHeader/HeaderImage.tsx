import React from "react";

import UploadedImage from "../../../components/UploadedImage";
import { useUpdateCurrentUserAvatarMutation } from "../../../store/serviceApi";

type HeaderImageProps = {
  avatarUrl: string;
  id: string;
  isEditable: boolean;
};

const HeaderImage = (props: HeaderImageProps) => {
  const { avatarUrl, id, isEditable } = props;
  const [updateUserAvatar, { error: updateUserAvatarError }] = useUpdateCurrentUserAvatarMutation();

  const handleSubmit = (file: File) => {
    // at the moment RTK Query code gen doesn't support multipart form requests
    var payload = new FormData();
    payload.append("avatarBlob", file);
    fetch("/api/v2/Users/me/avatar", {
      method: "PUT",
      // let the browser set Content-Type header based on the payload
      body: payload,
    }).then(() => {
      // invoke a call that results in 415 Media type not supported, but invalidates the cache and will make another call to get uploaded avatar url
      updateUserAvatar({body: {}})
    });
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
