import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux';

import { UpdatedUserRequest } from 'MainApp/apis/user';
import LanguageDropdown from 'MainApp/components/LanguageDropdown';
import { CurrentUserState, updateUser } from 'MainApp/modules/currentUser/currentUser';
import { getLanguages, LanguagesState } from 'MainApp/modules/language/languages';
import { RootState } from 'MainApp/rootReducer';

const Settings = () => {
  const [updatedUser, setUpdatedUser] = useState<UpdatedUserRequest>()
  const [isEditing, setIsEditing] = useState(false)
  const { currentUser, language, id } = useSelector(
    (state: RootState): CurrentUserState => state.currentUser, shallowEqual
  )
  const { languages } = useSelector((state: RootState): LanguagesState => state.languages, shallowEqual)
  const dispatch = useDispatch()

  useEffect(() => {
    if (!languages.length) {
      dispatch(getLanguages());
    }
    setUpdatedUser({ languageId: currentUser?.languageId })
  }, [])

  const handleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const { value, name } = event.target

    setUpdatedUser({ ...updatedUser, [name]: value })
  }

  const handleEditClick = () => setIsEditing(true)
  const handleEditCancel = () => setIsEditing(false)
  const handleSave = () => {
    dispatch(updateUser(id, updatedUser))
    handleEditCancel()
  }

  const renderDropdown = () => (
    <LanguageDropdown
      name="languageId"
      value={updatedUser?.languageId?.toString() || ''}
      languages={languages}
      hasError={false}
      onChange={handleChange}
    />
  )

  const renderLanguage = () => {
    if (!language) {
      return <div>Set your application language by editing your settings</div>
    }

    const {
      attributes: { longName, shortRegion },
    } = language;
    const regionText = shortRegion ? ` - ${shortRegion}` : ''

    return <div>{`${longName}${regionText}`}</div>
  }

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
  }

  return (
    <div className="w-5/6 mx-auto my-8">
      <div className="w-full flex justify-between items-center my-8">
        <h1 className="text-4xl font-extrabold">Settings</h1>
        <div>
          {isEditing ? (
            renderEditButtons()
          ) : (
            <button
              type="button"
              className="rounded bg-green py-2 px-6"
              onClick={handleEditClick}
            >
              Edit
            </button>
          )}
        </div>
      </div>
      <div className="border p-4">
        <div className="w-1/2">
          <label>Application Language</label>
          {isEditing ? renderDropdown() : renderLanguage()}
        </div>
      </div>
    </div>
  );
}

export default Settings
