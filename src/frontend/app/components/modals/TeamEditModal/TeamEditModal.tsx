import classnames from "classnames";
import { capitalize } from "lodash";
import { DateTime } from "luxon";
import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";

import { UpdateTeamRequest } from "../../../apis/team";
import { createTeam, getTeam, updateTeam } from "../../../modules/team/team";
import { RootState } from "../../../rootReducer";
import { toDateTime } from "../../../utils/dateUtils";

import MultiInput from "../../MultiInput";
import Modal, { ModalProps, ModalSize } from "../Modal/Modal";

const STATUS_OPTIONS = ["competitive", "developing", "inactive"];
const TYPE_OPTIONS = ["community", "university", "youth"];
const REQUIRED_FIELDS = ["name", "city", "country", "groupAffiliation", "status"];
const DATE_FORMAT = "yyyy-LL-dd";
const initialNewTeam: UpdateTeamRequest = {
  city: "",
  country: "",
  groupAffiliation: null,
  joinedAt: "",
  name: "",
  nationalGoverningBodyId: "",
  state: "",
  status: null,
  urls: [],
};

const validateInput = (team: UpdateTeamRequest): string[] => {
  return Object.keys(team).filter((dataKey: string) => {
    if (REQUIRED_FIELDS.includes(dataKey) && !team[dataKey]) {
      return true;
    }
    return false;
  });
};

interface TeamEditModalProps extends Omit<ModalProps, "size"> {
  teamId?: string;
  ngbId: string;
}

const TeamEditModal = (props: TeamEditModalProps) => {
  const { teamId, onClose, ngbId } = props;

  const [errors, setErrors] = useState<string[]>();
  const [hasChangedTeam, setHasChangedTeam] = useState(false);
  const [newTeam, setNewTeam] = useState<UpdateTeamRequest>(initialNewTeam);
  const [urls, setUrls] = useState<string[]>();
  const { team, socialAccounts } = useSelector((state: RootState) => state.team, shallowEqual);
  const dispatch = useDispatch();

  const formType = teamId ? "Edit" : "New";
  const hasError = (dataKey: string): boolean => errors?.includes(dataKey);
  const currentDay = DateTime.local().toFormat(DATE_FORMAT);

  useEffect(() => {
    if (teamId) {
      dispatch(getTeam(teamId, ngbId));
    }
  }, [teamId, dispatch]);

  useEffect(() => {
    if (team && teamId) {
      const existingUrls = socialAccounts.length
        ? socialAccounts.map((account) => account.url)
        : [];
      setNewTeam({ ...team, urls: existingUrls, nationalGoverningBodyId: ngbId });
    }
  }, [team, socialAccounts]);

  const handleSubmit = () => {
    const validationErrors = validateInput(newTeam);
    if (validationErrors.length) {
      setErrors(validationErrors);
      return null;
    }

    const teamToSend = { ...newTeam, urls, nationalGoverningBodyId: ngbId };
    if (teamId) {
      dispatch(updateTeam(teamId, teamToSend));
    } else {
      dispatch(createTeam(teamToSend));
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

    if (name === "joinedAt") {
      value = DateTime.fromFormat(value, DATE_FORMAT).toSQL().toString();
    }

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
              value={newTeam.state}
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
              placeholder="Select the age group"
              name="groupAffiliation"
              onChange={handleInputChange}
              value={newTeam.groupAffiliation || ""}
            >
              <option value="" />
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
              placeholder="Select the playing status"
              name="status"
              onChange={handleInputChange}
              value={newTeam.status || ""}
            >
              <option value="" />
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
            <MultiInput onChange={handleUrlChange} values={newTeam.urls || []} />
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
