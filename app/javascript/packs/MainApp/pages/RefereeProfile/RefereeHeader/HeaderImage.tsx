import React from 'react'
import { useDispatch } from 'react-redux'

import UploadedImage from '../../../components/UploadedImage'
import { updateUserAvatar } from '../../../modules/currentUser/currentUser';

type HeaderImageProps = {
  avatarUrl: string;
  id: string
}

const HeaderImage = (props: HeaderImageProps) => {
  const { avatarUrl, id } = props
  const dispatch = useDispatch();
  
  const handleSubmit = (file: File) => {
    dispatch(updateUserAvatar(id, file))
  }

  return <UploadedImage imageAlt="referee avatar" imageUrl={avatarUrl} onSubmit={handleSubmit} />;
}

export default HeaderImage
