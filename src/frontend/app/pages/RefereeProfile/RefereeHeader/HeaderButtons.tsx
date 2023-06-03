import React from "react";

type HeaderButtonsProps = {
  isEditing: boolean;
  onEdit: () => void;
  onSubmit: () => void;
  onCancel: () => void;
};

const HeaderButtons = (props: HeaderButtonsProps) => {
  const editButton = (
    <button type="button" className="rounded bg-green py-2 px-6" onClick={props.onEdit}>
      Edit
    </button>
  );

  const saveButton = (
    <button
      type="submit"
      className="rounded border-green border-2 text-green py-2 px-6"
      onClick={props.onSubmit}
    >
      Save Changes
    </button>
  );

  const cancelButton = (
    <button
      type="reset"
      onClick={props.onCancel}
      className="rounded bg-blue-darker py-2 px-6 text-white mr-4"
    >
      Cancel
    </button>
  );

  const editingButtons = (
    <div className="flex justify-end w-full">
      {cancelButton}
      {saveButton}
    </div>
  );

  return !props.isEditing ? editButton : editingButtons;
};

export default HeaderButtons;
