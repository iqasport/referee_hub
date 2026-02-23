import classnames from "classnames";
import { capitalize } from "lodash";
import React from "react";

import { toDateTime } from "../../../utils/dateUtils";

import MultiInput from "../../MultiInput";
import Modal, { ModalProps, ModalSize } from "../Modal/Modal";
import { NgbTeamViewModel, TeamGroupAffiliation, TeamStatus } from "../../../store/serviceApi";
import { currentDay, useTeamEditForm } from "./useTeamEditForm";

const STATUS_OPTIONS: TeamStatus[] = ["competitive", "developing", "inactive"];
const TYPE_OPTIONS: TeamGroupAffiliation[] = ["community", "university", "youth", "national"];
const DATE_FORMAT = "yyyy-MM-dd";

interface TeamLogoSectionProps {
  logoPreview: string | null;
  errors: string[];
  fileInputRef: React.RefObject<HTMLInputElement>;
  onLogoChange: (event: React.ChangeEvent<HTMLInputElement>) => void;
  onRemoveLogo: () => void;
}

const TeamLogoSection = ({ logoPreview, errors, fileInputRef, onLogoChange, onRemoveLogo }: TeamLogoSectionProps) => {
  const hasError = (key: string) => errors?.includes(key);
  return (
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
                onClick={onRemoveLogo}
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
              onChange={onLogoChange}
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
  );
};

interface TeamLocationFieldsProps {
  newTeam: NgbTeamViewModel;
  hasError: (key: string) => boolean;
  onChange: (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => void;
}

const TeamLocationFields = ({ newTeam, hasError, onChange }: TeamLocationFieldsProps) => (
  <div className="flex w-full my-8">
    <label className="w-1/3 mr-4">
      <span className="text-gray-700">City</span>
      <input
        className={classnames("form-input mt-1 block w-full", {
          "border border-red-500": hasError("city"),
        })}
        placeholder="Los Angeles"
        name="city"
        onChange={onChange}
        value={newTeam.city}
      />
      {hasError("city") && <span className="text-red-500">City cannot be blank</span>}
    </label>
    <label className="w-1/3 mr-4">
      <span className="text-gray-700">State/Province</span>
      <input
        className="form-input mt-1 block w-full"
        placeholder="California"
        name="state"
        onChange={onChange}
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
        onChange={onChange}
        value={newTeam.country}
      />
      {hasError("country") && <span className="text-red-500">Country cannot be blank</span>}
    </label>
  </div>
);

interface TeamTypeStatusFieldsProps {
  newTeam: NgbTeamViewModel;
  hasError: (key: string) => boolean;
  onChange: (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => void;
}

const renderOption = (value: string) => (
  <option key={value} value={value}>
    {capitalize(value)}
  </option>
);

const TeamTypeStatusFields = ({ newTeam, hasError, onChange }: TeamTypeStatusFieldsProps) => (
  <div className="flex w-full">
    <label className="w-1/3 mr-4">
      <span className="text-gray-700">Type</span>
      <select
        className={classnames("form-select mt-1 block w-full", {
          "border border-red-500": hasError("groupAffiliation"),
        })}
        name="groupAffiliation"
        onChange={onChange}
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
        onChange={onChange}
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
        onChange={onChange}
        value={toDateTime(newTeam.joinedAt).toFormat(DATE_FORMAT)}
      />
    </label>
  </div>
);

interface TeamContactFieldsProps {
  newTeam: NgbTeamViewModel;
  hasError: (key: string) => boolean;
  originalUrls: string[];
  onChange: (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => void;
  onUrlChange: (newUrls: string[]) => void;
}

const TeamContactFields = ({ newTeam, hasError, originalUrls, onChange, onUrlChange }: TeamContactFieldsProps) => (
  <>
    <div className="w-full my-8">
      <label>
        <span className="text-gray-700">Description</span>
        <textarea
          className="form-textarea mt-1 block w-full"
          placeholder="Brief description of the team"
          name="description"
          onChange={onChange}
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
          onChange={onChange}
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
        <MultiInput onChange={onUrlChange} values={originalUrls} />
      </label>
    </div>
  </>
);

interface TeamEditModalProps extends Omit<ModalProps, "size"> {
  teamId?: string;
  ngbId?: string;
  team?: NgbTeamViewModel;
}

const TeamEditModal = (props: TeamEditModalProps) => {
  const { team, teamId, ngbId, onClose, open } = props;

  const {
    errors,
    hasChangedTeam,
    newTeam,
    originalUrls,
    logoPreview,
    fileInputRef,
    formType,
    isLoading,
    hasError,
    handleSubmit,
    handleInputChange,
    handleUrlChange,
    handleLogoChange,
    handleRemoveLogo,
  } = useTeamEditForm({ team, teamId, ngbId, open, onClose });

  return (
    <Modal {...props} size={ModalSize.Large}>
      <h2 className="text-center text-xl font-semibold my-8">{`${formType} Team`}</h2>
      <form>
        <TeamLogoSection
          logoPreview={logoPreview}
          errors={errors}
          fileInputRef={fileInputRef}
          onLogoChange={handleLogoChange}
          onRemoveLogo={handleRemoveLogo}
        />
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
        <TeamLocationFields newTeam={newTeam} hasError={hasError} onChange={handleInputChange} />
        <TeamTypeStatusFields newTeam={newTeam} hasError={hasError} onChange={handleInputChange} />
        <TeamContactFields
          newTeam={newTeam}
          hasError={hasError}
          originalUrls={originalUrls}
          onChange={handleInputChange}
          onUrlChange={handleUrlChange}
        />
        <div className="w-full text-center">
          <button
            type="button"
            className={classnames("uppercase text-xl py-4 px-8 rounded-lg bg-green text-white", {
              "opacity-50 cursor-default": (!teamId && !hasChangedTeam) || isLoading,
            })}
            onClick={handleSubmit}
            disabled={(!teamId && !hasChangedTeam) || isLoading}
          >
            {isLoading ? "Saving..." : "Done"}
          </button>
        </div>
      </form>
    </Modal>
  );
};

export default TeamEditModal;

