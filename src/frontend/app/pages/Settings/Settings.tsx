import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { Navigate } from "react-router-dom";

import { UpdatedUserRequest } from "../../apis/user";
import { CurrentUserState, updateUser } from "../../modules/currentUser/currentUser";
import { getLanguages, LanguagesState } from "../../modules/language/languages";
import { RootState } from "../../rootReducer";
import { formatLanguage } from "../../utils/langUtils";
import { AppDispatch } from "../../store";

const Settings = () => {
  const [updatedUser, setUpdatedUser] = useState<UpdatedUserRequest>();
  const [isEditing, setIsEditing] = useState(false);
  const { currentUser, language, id } = useSelector(
    (state: RootState): CurrentUserState => state.currentUser,
    shallowEqual
  );
  const { languages } = useSelector(
    (state: RootState): LanguagesState => state.languages,
    shallowEqual
  );
  const dispatch = useDispatch<AppDispatch>();
  if (!currentUser?.enabledFeatures?.includes("i18n")) return <Navigate to="/" replace />;

  useEffect(() => {
    if (!languages.length) {
      dispatch(getLanguages());
    }
    setUpdatedUser({ languageId: currentUser?.languageId });
  }, []);

  const handleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const { value, name } = event.target;

    setUpdatedUser({ ...updatedUser, [name]: value });
  };

  const handleEditClick = () => setIsEditing(true);
  const handleEditCancel = () => setIsEditing(false);
  const handleSave = () => {
    dispatch(updateUser(id, updatedUser));
    handleEditCancel();
  };

  const renderDropdown = () => {
    // Transform language objects to formatted strings for the dropdown
    const languageOptions = languages.map((lang) => ({
      id: lang.id,
      label: formatLanguage(lang),
    }));

    return (
      <div>
        <select
          className="form-select mt-1 block w-full"
          name="languageId"
          onChange={handleChange}
          value={updatedUser?.languageId?.toString() ?? ""}
        >
          <option value="">Select the language</option>
          {languageOptions.map((option) => (
            <option key={option.id} value={option.id}>
              {option.label}
            </option>
          ))}
        </select>
        <div>
          <p className="my-2">
            Don't see your desired language? Email{" "}
            <a className="text-blue-darker" href="mailto:translation@iqasport.org">
              translation@iqasport.org
            </a>{" "}
            to add your language to the IQA.
          </p>
        </div>
      </div>
    );
  };

  const renderLanguage = () => {
    if (!language) {
      return <div>Set your application language by editing your settings</div>;
    }

    return <div>{formatLanguage(language)}</div>;
  };

  const renderEditButtons = () => {
    return (
      <>
        <button
          type="reset"
          className="rounded bg-blue-darker py-2 px-6 text-white mr-4"
          onClick={handleEditCancel}
        >
          Cancel
        </button>
        <button
          type="submit"
          className="rounded border-green border-2 text-green py-2 px-6"
          onClick={handleSave}
        >
          Save
        </button>
      </>
    );
  };

  return (
    <div className="w-5/6 mx-auto my-8">
      <div className="w-full flex justify-between items-center my-8">
        <h1 className="text-4xl font-extrabold">Settings</h1>
        <div>
          {isEditing ? (
            renderEditButtons()
          ) : (
            <button type="button" className="rounded bg-green py-2 px-6" onClick={handleEditClick}>
              Edit
            </button>
          )}
        </div>
      </div>
      <div className="border p-4">
        <div className="w-1/2">
          <label className="font-semibold text-lg">Application Language</label>
          {isEditing ? renderDropdown() : renderLanguage()}
        </div>
      </div>
    </div>
  );
};

export default Settings;
