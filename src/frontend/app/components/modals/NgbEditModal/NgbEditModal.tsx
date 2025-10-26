import classnames from "classnames";
import { capitalize, words } from "lodash";
import React, { useEffect, useMemo, useState } from "react";

import MultiInput from "../../MultiInput";
import Modal, { ModalProps, ModalSize } from "../Modal/Modal";
import { AdminNgbUpdateModel, NgbMembershipStatus, NgbRegion, NgbUpdateModel, useAdminCreateNgbMutation, useAdminUpdateNgbMutation, useGetCurrentUserQuery, useGetNgbInfoQuery, useUpdateNgbMutation } from "../../../store/serviceApi";
import { urlType } from "../../../utils/socialUtils";

const REQUIRED_FIELDS: (keyof AdminNgbUpdateModel)[] = ["name", "region", "membershipStatus", "acronym"];
const REGION_OPTIONS: NgbRegion[] = ["north_america", "south_america", "europe", "africa", "asia"];
const MEMBERSHIP_OPTIONS: NgbMembershipStatus[] = ["area_of_interest", "emerging", "developing", "full"];
const initialNewNgb: AdminNgbUpdateModel & NgbUpdateModel = {
  acronym: "",
  country: "",
  membershipStatus: null,
  name: "",
  playerCount: 0,
  region: null,
  socialAccounts: [],
  website: "",
};

const validateInput = (ngb: AdminNgbUpdateModel & NgbUpdateModel): string[] => {
  return Object.keys(ngb).filter((dataKey: string) => {    
    if (REQUIRED_FIELDS.includes(dataKey as keyof AdminNgbUpdateModel) && !ngb[dataKey]) {
      return true;
    }

    // if website is not empty it needs to be a valid URL
    if (dataKey as keyof AdminNgbUpdateModel === "website" && ngb[dataKey]) {
      try {
        new URL(ngb[dataKey]);
      } catch {
        return true;
      }
    }

    return false;
  });
};

interface NgbEditModalProps extends Omit<ModalProps, "size"> {
  ngbId?: string;
}

const NgbEditModal = (props: NgbEditModalProps) => {
  const { ngbId, onClose } = props;

  const [errors, setErrors] = useState<string[]>();
  const [hasChangedNgb, setHasChangedNgb] = useState(false);
  const [newNgb, setNewNgb] = useState(initialNewNgb);
  const [urls, setUrls] = useState<string[]>([]);

  const { currentData: currentUser } = useGetCurrentUserQuery();
  const roles = currentUser?.roles?.map(r => r.roleType);

  const formType = ngbId ? "Edit" : "New";
  const hasError = (dataKey: string): boolean => errors?.includes(dataKey);
  const isIqaAdmin = roles.includes("IqaAdmin");

  const {data: ngb} = useGetNgbInfoQuery({ ngb: ngbId }, {skip: !ngbId});
  const existingUrls = useMemo(() => ngb?.socialAccounts.map((account) => account.url) || [], [ngbId]);

  const [updateNgb] = useUpdateNgbMutation();
  const [adminUpdateNgb] = useAdminUpdateNgbMutation();
  const [adminCreateNgb] = useAdminCreateNgbMutation();

  useEffect(() => {
    if (ngb && ngbId) {
      setNewNgb({
        acronym: ngb.acronym,
        country: ngb.country,
        membershipStatus: ngb.membershipStatus,
        name: ngb.name,
        playerCount: ngb.playerCount,
        region: ngb.region,
        website: ngb.website,
      });
    }
  }, [ngb]);

  const handleSubmit = () => {
    const validationErrors = validateInput(newNgb);
    if (validationErrors.length) {
      setErrors(validationErrors);
      return null;
    }

    const ngbToSend: AdminNgbUpdateModel & NgbUpdateModel = {
      ...newNgb,
      socialAccounts: urls.map(url => ({url, type: urlType(url) }))
    };
    if (ngbId) {
      if (isIqaAdmin) {
        adminUpdateNgb({ngb: ngbId, adminNgbUpdateModel: ngbToSend});
      } else {
        updateNgb({ngb: ngbId, ngbUpdateModel: ngbToSend});
      }
    } else {
      // TODO: get country code for creating NGB some other way instead of reusing acronym
      adminCreateNgb({ngb: ngbToSend.acronym, adminNgbUpdateModel: ngbToSend});
    }

    setHasChangedNgb(false);
    onClose();
  };

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = event.target;
    let newValue: string | number = value;

    if (!hasChangedNgb) setHasChangedNgb(true);
    if (name === "playerCount") newValue = parseInt(value, 10);

    setNewNgb({ ...newNgb, [name]: newValue });
  };

  const handleUrlChange = (newUrls: string[]) => {
    if (!hasChangedNgb) setHasChangedNgb(true);
    setUrls(newUrls);
  };

  const renderOption = (value: string) => {
    return (
      <option key={value} value={value}>
        {words(value)
          .map((word) => capitalize(word))
          .join(" ")}
      </option>
    );
  };

  return (
    <Modal {...props} size={ModalSize.Large}>
      <h2 className="text-center text-xl font-semibold my-8">{`${formType} National Governing Body`}</h2>
      <form>
        <label className="block">
          <span className="text-gray-700">Name</span>
          <input
            className={classnames("form-input mt-1 block w-full", {
              "border border-red-500": hasError("name"),
            })}
            placeholder="US Quidditch"
            name="name"
            onChange={handleInputChange}
            value={newNgb.name || ""}
          />
          {hasError("name") && <span className="text-red-500">Name cannot be blank</span>}
        </label>
        <label className="block my-8">
          <span className="text-gray-700">Country</span>
          <input
            className="form-input mt-1 block w-full"
            placeholder="United States"
            name="country"
            onChange={handleInputChange}
            value={newNgb.country || ""}
          />
        </label>
        <div className="flex w-full my-8">
          <label className="w-1/2 mr-4">
            <span className="text-gray-700">Region</span>
            <select
              disabled={!isIqaAdmin}
              className={classnames("form-select mt-1 block w-full", {
                "border border-red-500": hasError("region"),
              })}
              name="region"
              onChange={handleInputChange}
              value={newNgb.region || ""}
            >
              <option className="italic" value="">Select region</option>
              {REGION_OPTIONS.map(renderOption)}
            </select>
            {hasError("region") && <span className="text-red-500">Region cannot be blank</span>}
          </label>
          <label className="w-1/2">
            <span className="text-gray-700">Membership Status</span>
            <select
              disabled={!isIqaAdmin}
              className={classnames("form-select mt-1 block w-full", {
                "border border-red-500": hasError("membershipStatsus"),
              })}
              name="membershipStatus"
              onChange={handleInputChange}
              value={newNgb.membershipStatus || ""}
            >
              <option className="italic" value="">Select status</option>
              {MEMBERSHIP_OPTIONS.map(renderOption)}
            </select>
            {hasError("membershipStatus") && (
              <span className="text-red-500">Membership status cannot be blank</span>
            )}
          </label>
        </div>
        <div className="flex w-full my-8 justify-between">
          <label className="w-1/2 mr-4">
            <span className="text-gray-700">Acronym</span>
            <input
              className={classnames("form-input mt-1 block w-full", {
                "border border-red-500": hasError("acronym"),
              })}
              placeholder="USQ"
              name="acronym"
              onChange={handleInputChange}
              value={newNgb.acronym || ""}
            />
          {hasError("acronym") && <span className="text-red-500">Acronym cannot be blank</span>}
          </label>
          <label className="w-1/2">
            <span className="text-gray-700">Player Count</span>
            <input
              type="number"
              min="0"
              className="form-input mt-1 block w-full"
              name="playerCount"
              onChange={handleInputChange}
              value={newNgb.playerCount || 0}
            />
          </label>
        </div>
        <label className="block">
          <span className="text-gray-700">Website</span>
          <input
            type="url"
            className={classnames("form-input mt-1 block w-full", {
              "border border-red-500": hasError("website"),
            })}
            placeholder="https://www.usquidditch.org"
            name="website"
            onChange={handleInputChange}
            value={newNgb.website || ""}
          />
          {hasError("website") && (
              <span className="text-red-500">Website must be a valid URL (like &apos;https://...&apos;)</span>
            )}
        </label>
        <div className="w-full my-8">
          <label>
            <span className="text-gray-700">Social Media</span>
            <MultiInput onChange={handleUrlChange} values={existingUrls} />
          </label>
        </div>
        <div className="w-full text-center">
          <button
            type="button"
            className={classnames("uppercase text-xl py-4 px-8 rounded-lg bg-green text-white", {
              "opacity-50 cursor-default": !hasChangedNgb,
            })}
            onClick={handleSubmit}
            disabled={!hasChangedNgb}
          >
            Done
          </button>
        </div>
      </form>
    </Modal>
  );
};

export default NgbEditModal;
