import React from "react";

import DragNDrop from "../../components/DragNDrop";

interface UploadStepProps {
  onFileUpload: (selectedFile: File) => void;
  uploadedFile?: File;
}

const UploadStep = (props: UploadStepProps) => {
  const { uploadedFile } = props;
  const fileName = uploadedFile ? uploadedFile.name : "No file uploaded";

  return (
    <div className="w-1/2 mx-auto my-4 py-12">
      <DragNDrop {...props} />
      <p className="my-4">Uploaded File:</p>
      <p>{fileName}</p>
    </div>
  );
};

export default UploadStep;
