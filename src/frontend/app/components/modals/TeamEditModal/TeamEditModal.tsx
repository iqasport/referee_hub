import classnames from "classnames";
import { capitalize } from "lodash";
import { DateTime } from "luxon";
import React, { useEffect, useMemo, useState } from "react";

import { toDateTime } from "../../../utils/dateUtils";

import MultiInput from "../../MultiInput";
import Modal, { ModalProps, ModalSize } from "../Modal/Modal";
import { NgbTeamViewModel, SocialAccount, TeamGroupAffiliation, TeamStatus, useCreateNgbTeamMutation, useUpdateNgbTeamMutation } from "../../../store/serviceApi";
import { urlType } from "../../../utils/socialUtils";

const STATUS_OPTIONS: TeamStatus[] = ["competitive", "developing", "inactive"];
const TYPE_OPTIONS: TeamGroupAffiliation[] = ["community", "university", "youth"];
const REQUIRED_FIELDS: (keyof NgbTeamViewModel)[] = ["name", "city", "country", "groupAffiliation", "status"];
const DATE_FORMAT = "yyyy-MM-dd";

const currentDay = DateTime.local().toFormat(DATE_FORMAT);
const initialNewTeam: NgbTeamViewModel = {
  city: "",
  country: "",
  groupAffiliation: null,
  joinedAt: currentDay,
  name: "",
  state: "",
  status: null,
  socialAccounts: [],
};

const validateInput = (team: NgbTeamViewModel): string[] => {
  return Object.keys(team).filter((dataKey: string) => {
    if (REQUIRED_FIELDS.includes(dataKey as keyof NgbTeamViewModel) && !team[dataKey]) {
      return true;
    }
    return false;
  });
};

interface TeamEditModalProps extends Omit<ModalProps, "size"> {
  teamId?: string;
  ngbId: string;
  team?: NgbTeamViewModel;
}

const TeamEditModal = (props: TeamEditModalProps) => {
  const { team, teamId, onClose, ngbId } = props;

  // original urls are used as input to the url editor which needs a constant initial value
  const originalUrls = useMemo(() => team?.socialAccounts.map(sa => sa.url) || [], [team]);

  const [errors, setErrors] = useState<string[]>();
  const [hasChangedTeam, setHasChangedTeam] = useState(false);
  const [newTeam, setNewTeam] = useState<NgbTeamViewModel>(initialNewTeam);
  const [urls, setUrls] = useState<string[]>();

  const formType = teamId ? "Edit" : "New";
  const hasError = (dataKey: string): boolean => errors?.includes(dataKey);

  const [createTeam, {data: createTeamData, error: createTeamError, isLoading: isCreateTeamLoading}] = useCreateNgbTeamMutation();
  const [updateTeam, {data: updateTeamData, error: updateTeamError, isLoading: isUpdateTeamLoading}] = useUpdateNgbTeamMutation();
  // TODO: handle errors and show loading spinner
  // upon submit it should wait until *Data is not undefined before closing the modal

  useEffect(() => {
    if (team && teamId) {
      // copy data over to the local state for mutation
      setUrls(team.socialAccounts.map(sa => sa.url))
      setNewTeam({ ...team});
    }
  }, [team]);

  const handleSubmit = () => {
    const validationErrors = validateInput(newTeam);
    if (validationErrors.length) {
      setErrors(validationErrors);
      return null;
    }

    const accounts: SocialAccount[] = urls.map(url => ({url, type: urlType(url) }))
    const teamObject: NgbTeamViewModel = {
      ...newTeam,
      socialAccounts: accounts,
      state: newTeam.state || null // state is nullable but needs to be a string for input value
    };
    if (teamId) {
      updateTeam({ngb: ngbId, teamId: teamId, ngbTeamViewModel: teamObject});
    } else {
      createTeam({ngb: ngbId, ngbTeamViewModel: teamObject});
    }

    setHasChangedTeam(false);
    onClose();
  };

  const handleDataChange = (dataKey: string, newValue: string) => {
    setNewTeam({ ...newTeam, [dataKey]: newValue });
  };

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    let { value } = event.target;
    const { name } = event.target;

    setHasChangedTeam(true);
    handleDataChange(name, value);
  };

  const handleUrlChange = (newUrls: string[]) => {
    setHasChangedTeam(true);
    setUrls(newUrls);
  };

  const renderOption = (value: string) => {
    return (
      <option key={value} value={value}>
        {capitalize(value)}
      </option>
    );
  };

  return (
    <Modal {...props} size={ModalSize.Large}>
      <h2 className="text-center text-xl font-semibold my-8">{`${formType} Team`}</h2>
      <form>
        <label className="block">
          <span className="text-gray-700">Name</span>
          <input
            className={classnames("form-input mt-1 block w-full", {
              "border border-red-500": hasError("name"),
            })}
            placeholder="University Quidditch Team"
            name="name"
            onChange={handleInputChange}
            value={newTeam.name}
          />
          {hasError("name") && <span className="text-red-500">Name cannot be blank</span>}
        </label>
        <div className="flex w-full my-8">
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">City</span>
            <input
              className={classnames("form-input mt-1 block w-full", {
                "border border-red-500": hasError("city"),
              })}
              placeholder="Los Angeles"
              name="city"
              onChange={handleInputChange}
              value={newTeam.city}
            />
            {hasError("city") && <span className="text-red-500">City cannot be blank</span>}
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">State/Provence</span>
            <input
              className="form-input mt-1 block w-full"
              placeholder="California"
              name="state"
              onChange={handleInputChange}
              value={newTeam.state || ""}
            />
          </label>
          <label className="w-1/3">
            <span className="text-gray-700">Country</span>
            <input
              className={classnames("form-input mt-1 block w-full", {
                "border border-red-500": hasError("country"),
              })}
              placeholder="United States"
              name="country"
              onChange={handleInputChange}
              value={newTeam.country}
            />
            {hasError("country") && <span className="text-red-500">Country cannot be blank</span>}
          </label>
        </div>
        <div className="flex w-full">
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Type</span>
            <select
              className={classnames("form-select mt-1 block w-full", {
                "border border-red-500": hasError("groupAffiliation"),
              })}
              name="groupAffiliation"
              onChange={handleInputChange}
              value={newTeam.groupAffiliation || ""}
            >
              <option value="">Select the team type</option>
              {TYPE_OPTIONS.map(renderOption)}
            </select>
            {hasError("groupAffiliation") && (
              <span className="text-red-500">Type cannot be blank</span>
            )}
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Status</span>
            <select
              className={classnames("form-select mt-1 block w-full", {
                "border border-red-500": hasError("status"),
              })}
              name="status"
              onChange={handleInputChange}
              value={newTeam.status || ""}
            >
              <option value="">Select the playing status</option>
              {STATUS_OPTIONS.map(renderOption)}
            </select>
            {hasError("status") && <span className="text-red-500">Status cannot be blank</span>}
          </label>
          <label className="w-1/3">
            <span className="text-gray-700">Joined Date</span>
            <input
              type="date"
              min="2007-01-01"
              max={currentDay}
              className="form-input mt-1 block w-full"
              placeholder="YYYY-MM-DD"
              name="joinedAt"
              onChange={handleInputChange}
              value={toDateTime(newTeam.joinedAt).toFormat(DATE_FORMAT)}
            />
          </label>
        </div>
        <div className="w-full my-8">
          <label>
            <span className="text-gray-700">Social Media</span>
            <MultiInput onChange={handleUrlChange} values={originalUrls} />
          </label>
        </div>
        <div className="w-full text-center">
          <button
            type="button"
            className={classnames("uppercase text-xl py-4 px-8 rounded-lg bg-green text-white", {
              "opacity-50 cursor-default": !hasChangedTeam,
            })}
            onClick={handleSubmit}
            disabled={!hasChangedTeam}
          >
            Done
          </button>
        </div>
      </form>
    </Modal>
  );
};

export default TeamEditModal;
