import classnames from "classnames";
import { capitalize } from "lodash";
import { DateTime } from "luxon";
import React, { useEffect, useMemo, useRef, useState } from "react";

import { toDateTime } from "../../../utils/dateUtils";

import MultiInput from "../../MultiInput";
import Modal, { ModalProps, ModalSize } from "../Modal/Modal";
import { NgbTeamViewModel, SocialAccount, TeamGroupAffiliation, TeamStatus, useCreateNgbTeamMutation, useUpdateNgbTeamMutation, useUploadTeamLogoMutation } from "../../../store/serviceApi";
import { urlType } from "../../../utils/socialUtils";

const STATUS_OPTIONS: TeamStatus[] = ["competitive", "developing", "inactive"];
const TYPE_OPTIONS: TeamGroupAffiliation[] = ["community", "university", "youth", "national"];
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
  logoUrl: null,
  description: null,
  contactEmail: null,
};

const validateEmail = (email: string): boolean => {
  if (!email) return true; // Email is optional
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

const validateInput = (team: NgbTeamViewModel): string[] => {
  const errors: string[] = [];
  
  // Check required fields
  Object.keys(team).forEach((dataKey: string) => {
    if (REQUIRED_FIELDS.includes(dataKey as keyof NgbTeamViewModel) && !team[dataKey]) {
      errors.push(dataKey);
    }
  });
  
  // Validate email format if provided
  if (team.contactEmail && !validateEmail(team.contactEmail)) {
    errors.push("contactEmail");
  }
  
  return errors;
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
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const formType = teamId ? "Edit" : "New";
  const hasError = (dataKey: string): boolean => errors?.includes(dataKey);

  const [createTeam, {data: createTeamData, error: createTeamError, isLoading: isCreateTeamLoading}] = useCreateNgbTeamMutation();
  const [updateTeam, {data: updateTeamData, error: updateTeamError, isLoading: isUpdateTeamLoading}] = useUpdateNgbTeamMutation();
  const [uploadLogo, {isLoading: isUploadingLogo}] = useUploadTeamLogoMutation();
  // TODO: handle errors and show loading spinner
  // upon submit it should wait until *Data is not undefined before closing the modal

  useEffect(() => {
    if (team && teamId) {
      // copy data over to the local state for mutation
      setUrls(team.socialAccounts.map(sa => sa.url))
      setNewTeam({ ...team});
      // Set logo preview if team has a logo
      if (team.logoUrl) {
        setLogoPreview(team.logoUrl);
      }
    }
  }, [team]);

  const handleSubmit = async () => {
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
    
    try {
      let createdOrUpdatedTeam;
      if (teamId) {
        const result = await updateTeam({ngb: ngbId, teamId: teamId, ngbTeamViewModel: teamObject});
        if ('error' in result) {
          console.error("Failed to update team:", result.error);
          return;
        }
        createdOrUpdatedTeam = result.data;
      } else {
        const result = await createTeam({ngb: ngbId, ngbTeamViewModel: teamObject});
        if ('error' in result) {
          console.error("Failed to create team:", result.error);
          return;
        }
        createdOrUpdatedTeam = result.data;
      }

      // Upload logo if a file was selected
      if (logoFile && createdOrUpdatedTeam?.teamId) {
        try {
          const uploadResult = await uploadLogo({
            teamId: createdOrUpdatedTeam.teamId,
            logoBlob: logoFile
          }).unwrap();
          console.log("Logo uploaded successfully:", uploadResult);
        } catch (error) {
          console.error("Failed to upload logo:", error);
          setErrors([...(errors || []), "logoUpload"]);
          // Don't close the modal if logo upload fails
          return;
        }
      }

      setHasChangedTeam(false);
      onClose();
    } catch (error) {
      console.error("Unexpected error during team save:", error);
    }
  };

  const handleDataChange = (dataKey: string, newValue: string | null) => {
    setNewTeam({ ...newTeam, [dataKey]: newValue });
  };

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { value } = event.target;
    const { name } = event.target;

    setHasChangedTeam(true);
    handleDataChange(name, value);
  };

  const handleUrlChange = (newUrls: string[]) => {
    setHasChangedTeam(true);
    setUrls(newUrls);
  };

  const handleLogoChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      // Validate file is an image
      if (!file.type.startsWith("image/")) {
        setErrors([...(errors || []), "logoFile"]);
        return;
      }
      
      // Validate file size (max 5 MB)
      const maxSize = 5 * 1024 * 1024;
      if (file.size > maxSize) {
        setErrors([...(errors || []), "logoSize"]);
        return;
      }

      // Clear any previous logo errors
      if (errors) {
        setErrors(errors.filter(e => e !== "logoFile" && e !== "logoSize"));
      }

      setLogoFile(file);
      setHasChangedTeam(true);

      // Create preview
      const reader = new FileReader();
      reader.onloadend = () => {
        setLogoPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleRemoveLogo = () => {
    setLogoFile(null);
    setLogoPreview(null);
    setHasChangedTeam(true);
    handleDataChange('logoUrl', null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
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
        <div className="w-full my-8">
          <label>
            <span className="text-gray-700">Team Logo (Optional)</span>
            <div className="mt-2">
              {logoPreview ? (
                <div className="flex items-center gap-4">
                  <img 
                    src={logoPreview} 
                    alt="Team logo preview" 
                    className="w-32 h-32 object-cover rounded border"
                  />
                  <button
                    type="button"
                    onClick={handleRemoveLogo}
                    className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
                  >
                    Remove Logo
                  </button>
                </div>
              ) : (
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/*"
                  onChange={handleLogoChange}
                  className={classnames("form-input mt-1 block w-full", {
                    "border border-red-500": hasError("logoFile") || hasError("logoSize"),
                  })}
                />
              )}
              {hasError("logoFile") && (
                <span className="text-red-500 text-sm">Please select an image file</span>
              )}
              {hasError("logoSize") && (
                <span className="text-red-500 text-sm">File size must not exceed 5 MB</span>
              )}
              {hasError("logoUpload") && (
                <span className="text-red-500 text-sm">Failed to upload logo. Please try again.</span>
              )}
              <p className="text-sm text-gray-500 mt-1">
                Max file size: 5 MB. Supported formats: JPG, PNG, GIF
              </p>
            </div>
          </label>
        </div>
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
            <span className="text-gray-700">Description</span>
            <textarea
              className="form-textarea mt-1 block w-full"
              placeholder="Brief description of the team"
              name="description"
              onChange={(e) => {
                setHasChangedTeam(true);
                handleDataChange(e.target.name, e.target.value);
              }}
              value={newTeam.description || ""}
              rows={3}
            />
          </label>
        </div>
        <div className="w-full my-8">
          <label>
            <span className="text-gray-700">Contact Email</span>
            <input
              type="email"
              className={classnames("form-input mt-1 block w-full", {
                "border border-red-500": hasError("contactEmail"),
              })}
              placeholder="team@example.com"
              name="contactEmail"
              onChange={handleInputChange}
              value={newTeam.contactEmail || ""}
            />
            {hasError("contactEmail") && (
              <span className="text-red-500">Please enter a valid email address</span>
            )}
          </label>
        </div>
        <div className="w-full my-8">
          <label>
            <span className="text-gray-700">Social Media (Optional)</span>
            <MultiInput onChange={handleUrlChange} values={originalUrls} />
          </label>
        </div>
        <div className="w-full text-center">
          <button
            type="button"
            className={classnames("uppercase text-xl py-4 px-8 rounded-lg bg-green text-white", {
              "opacity-50 cursor-default": !hasChangedTeam || isCreateTeamLoading || isUpdateTeamLoading || isUploadingLogo,
            })}
            onClick={handleSubmit}
            disabled={!hasChangedTeam || isCreateTeamLoading || isUpdateTeamLoading || isUploadingLogo}
          >
            {(isCreateTeamLoading || isUpdateTeamLoading || isUploadingLogo) ? "Saving..." : "Done"}
          </button>
        </div>
      </form>
    </Modal>
  );
};

export default TeamEditModal;
