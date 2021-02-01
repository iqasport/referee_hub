import { faTimes } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classnames from "classnames";
import React, { FunctionComponent } from "react";

export enum ModalSize {
  Small = "small",
  Medium = "medium",
  Large = "large",
}

export interface ModalProps {
  open: boolean;
  onClose?: () => void;
  showClose: boolean;
  size: ModalSize;
}

const Modal: FunctionComponent<ModalProps> = (props) => {
  if (!props.open) return null;

  const closeButton = (
    <div className="cursor-pointer absolute top-0 right-0 m-2" onClick={props.onClose}>
      <FontAwesomeIcon icon={faTimes} />
    </div>
  );

  return (
    <div className="animated fadeIn fixed z-50 inset-0 overflow-auto bg-smoke flex">
      <div
        className={classnames("relative p-8 bg-gray-200 w-full m-auto flex-col", {
          "max-w-3xl": props.size === ModalSize.Large,
          "max-w-md": props.size === ModalSize.Small,
          "max-w-xl": props.size === ModalSize.Medium,
        })}
      >
        {props.showClose && closeButton}
        {props.children}
      </div>
    </div>
  );
};

export default Modal;
