import React from 'react'

type HeaderButtonsProps = {
  isEditing: boolean;
  onEdit: () => void;
  onSubmit: () => void;
}

const HeaderButtons = (props: HeaderButtonsProps) => {
  const editButton = (
    <button
      className="rounded bg-green py-2 px-6 cursor-pointer"
      onClick={props.onEdit}
    >
      Edit
    </button>
  )

  const saveButton = (
    <button
      className="rounded border-green border-2 text-green py-2 px-6 cursor-pointer"
      onClick={props.onSubmit}
    >
      Save Changes
    </button>
  )

  return !props.isEditing ? editButton : saveButton
};

export default HeaderButtons
