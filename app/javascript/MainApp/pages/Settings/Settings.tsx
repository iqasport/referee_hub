import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux';

import { UpdatedUserRequest } from 'MainApp/apis/user';
import LanguageDropdown from 'MainApp/components/LanguageDropdown';
import { CurrentUserState } from 'MainApp/modules/currentUser/currentUser';
import { getLanguages, LanguagesState } from 'MainApp/modules/language/languages';
import { RootState } from 'MainApp/rootReducer';

const Settings = () => {
  const [updatedUser, setUpdatedUser] = useState<UpdatedUserRequest>()
  const { currentUser } = useSelector((state: RootState): CurrentUserState => state.currentUser, shallowEqual)
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

  return (
    <div className="w-5/6 mx-auto my-8">
      <div className="w-full flex justify-between items-center my-8">
        <h1 className="text-4xl font-extrabold">Settings</h1>
      </div>
      <div className="border p-4">
        <div className="w-1/2">
          <label>Application Language</label>
          <LanguageDropdown
            name="languageId"
            value={updatedUser?.languageId?.toString() || ''}
            languages={languages}
            hasError={false}
            onChange={handleChange}
          />
        </div>
      </div>
    </div>
  );
}

export default Settings
