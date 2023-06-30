import { faPlusCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classnames from "classnames";
import React, { useState } from "react";

import Modal, { ModalSize } from "../modals/Modal/Modal";

type UploadedImageProps = {
  imageUrl: string;
  onSubmit: (file: File) => void;
  imageAlt: string;
  isEditable: boolean;
};

const UploadedImage = (props: UploadedImageProps) => {
  const { imageUrl, onSubmit, imageAlt, isEditable } = props;
  const [isOpen, setIsOpen] = useState(false);
  const [tmpUrl, setTmpUrl] = useState<string>();
  const [file, setFile] = useState<File>();

  const handleOpen = () => setIsOpen(true);
  const handleClose = () => setIsOpen(false);

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newFile = event.target.files[0];
    setTmpUrl(URL.createObjectURL(newFile));
    setFile(newFile);
  };

  const handleSubmit = () => {
    onSubmit(file);
    handleClose();
  };

  const renderAvatar = (): JSX.Element | null => {
    const fileSrc = tmpUrl || imageUrl;
    if (!fileSrc) return null;

    return (
      <img src={fileSrc} alt={imageAlt} className="object-cover h-full w-full overflow-hidden" />
    );
  };

  return (
    <>
      <div className="w-40 h-40 bg-gray-400 block relative border-gray-600 border">
        {renderAvatar()}
        {isEditable && (
          <div className="absolute z-1 right-0 bottom-0 pr-1 cursor-pointer" onClick={handleOpen}>
            <FontAwesomeIcon icon={faPlusCircle} className="w-6 h-6 text-gray-700" />
          </div>
        )}
      </div>
      <Modal onClose={handleClose} open={isOpen} showClose={true} size={ModalSize.Medium}>
        <h1 className="my-2 font-bold text-2xl">Upload Image</h1>
        <div className="flex justify-between items-center flex-wrap">
          <input type="file" name="imageUrl" accept="image/*" onChange={handleChange} />
          <button
            type="submit"
            className={classnames("bg-blue-darker py-2 px-6 rounded text-white ml-2", {
              "opacity-50 cursor-not-allowed": !tmpUrl,
            })}
            onClick={handleSubmit}
            disabled={!file}
          >
            Upload
          </button>
        </div>
      </Modal>
    </>
  );
};

export default UploadedImage;
