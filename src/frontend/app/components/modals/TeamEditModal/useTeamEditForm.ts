import { DateTime } from "luxon";
import React, { useEffect, useMemo, useRef, useState } from "react";

import { urlType } from "../../../utils/socialUtils";
import {
  NgbTeamViewModel,
  SocialAccount,
  useCreateNgbTeamMutation,
  useUpdateNgbTeamMutation,
  useUpdateTeamMutation,
  useUploadTeamLogoMutation,
} from "../../../store/serviceApi";

const DATE_FORMAT = "yyyy-MM-dd";

export const currentDay = DateTime.local().toFormat(DATE_FORMAT);

export const initialNewTeam: NgbTeamViewModel = {
  city: "",
  country: "",
  groupAffiliation: null,
  joinedAt: currentDay,
  name: "",
  state: "",
  status: null,
  socialAccounts: [],
  description: null,
  contactEmail: null,
};

const REQUIRED_FIELDS: (keyof NgbTeamViewModel)[] = ["name", "city", "country", "groupAffiliation", "status"];

const validateEmail = (email: string): boolean => {
  if (!email) return true;
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

const validateInput = (team: NgbTeamViewModel): string[] => {
  const errors: string[] = [];

  Object.keys(team).forEach((dataKey: string) => {
    if (REQUIRED_FIELDS.includes(dataKey as keyof NgbTeamViewModel) && !team[dataKey]) {
      errors.push(dataKey);
    }
  });

  if (team.contactEmail && !validateEmail(team.contactEmail)) {
    errors.push("contactEmail");
  }

  return errors;
};

interface UseTeamEditFormOptions {
  team?: NgbTeamViewModel;
  teamId?: string;
  ngbId?: string;
  open: boolean;
  onClose: () => void;
}

export interface UseTeamEditFormResult {
  errors: string[];
  hasChangedTeam: boolean;
  newTeam: NgbTeamViewModel;
  urls: string[];
  logoPreview: string | null;
  fileInputRef: React.RefObject<HTMLInputElement>;
  formType: string;
  isLoading: boolean;
  hasError: (dataKey: string) => boolean;
  handleSubmit: () => Promise<void>;
  handleInputChange: (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => void;
  handleUrlChange: (newUrls: string[]) => void;
  handleLogoChange: (event: React.ChangeEvent<HTMLInputElement>) => void;
  handleRemoveLogo: () => void;
  originalUrls: string[];
}

export const useTeamEditForm = ({ team, teamId, ngbId, open, onClose }: UseTeamEditFormOptions): UseTeamEditFormResult => {
  const originalUrls = useMemo(() => team?.socialAccounts?.map((sa) => sa.url) || [], [team]);

  const [errors, setErrors] = useState<string[]>([]);
  const [hasChangedTeam, setHasChangedTeam] = useState(false);
  const [newTeam, setNewTeam] = useState<NgbTeamViewModel>(initialNewTeam);
  const [urls, setUrls] = useState<string[]>([]);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const formInitialized = useRef(false);

  const formType = teamId ? "Edit" : "New";

  const [createTeam, { isLoading: isCreateTeamLoading }] = useCreateNgbTeamMutation();
  const [updateNgbTeam, { isLoading: isUpdateNgbTeamLoading }] = useUpdateNgbTeamMutation();
  const [updateTeam, { isLoading: isUpdateTeamLoading }] = useUpdateTeamMutation();
  const [uploadLogo, { isLoading: isUploadingLogo }] = useUploadTeamLogoMutation();

  const isLoading = isCreateTeamLoading || isUpdateNgbTeamLoading || isUpdateTeamLoading || isUploadingLogo;

  useEffect(() => {
    if (team && teamId && !formInitialized.current) {
      setUrls(team.socialAccounts?.map((sa) => sa.url) || []);
      setNewTeam({ ...team });
      setHasChangedTeam(true);
      formInitialized.current = true;
    }
  }, [team, teamId]);

  useEffect(() => {
    if (!open) {
      formInitialized.current = false;
      setHasChangedTeam(false);
      setNewTeam(initialNewTeam);
      setUrls([]);
      setLogoFile(null);
      setLogoPreview(null);
      setErrors([]);
    }
  }, [open]);

  const performSave = async (teamObject: NgbTeamViewModel) => {
    if (teamId && ngbId) {
      return updateNgbTeam({ ngb: ngbId, teamId, ngbTeamViewModel: teamObject });
    } else if (teamId) {
      return updateTeam({ teamId, ngbTeamViewModel: teamObject });
    } else {
      return createTeam({ ngb: ngbId, ngbTeamViewModel: teamObject });
    }
  };

  const uploadLogoIfNeeded = async (savedTeamId: string): Promise<boolean> => {
    if (!logoFile) return true;
    try {
      await uploadLogo({ teamId: savedTeamId, logoBlob: logoFile }).unwrap();
      return true;
    } catch {
      setErrors((prev) => [...prev, "logoUpload"]);
      return false;
    }
  };

  const handleSubmit = async () => {
    const validationErrors = validateInput(newTeam);
    if (validationErrors.length) {
      setErrors(validationErrors);
      return;
    }

    setErrors([]);

    const accounts: SocialAccount[] = urls.map((url) => ({ url, type: urlType(url) }));
    const teamObject: NgbTeamViewModel = {
      ...newTeam,
      socialAccounts: accounts,
      state: newTeam.state || null,
    };

    try {
      const result = await performSave(teamObject);
      if ("error" in result) {
        const errorMessage = teamId ? "Failed to update team" : "Failed to create team";
        setErrors((prev) => [...prev, errorMessage]);
        return;
      }

      if (result.data?.teamId) {
        const logoUploaded = await uploadLogoIfNeeded(result.data.teamId);
        if (!logoUploaded) return;
      }

      setHasChangedTeam(false);
      onClose();
    } catch {
      setErrors((prev) => [...prev, "An unexpected error occurred"]);
    }
  };

  const handleDataChange = (dataKey: string, newValue: string | null) => {
    setNewTeam((prev) => ({ ...prev, [dataKey]: newValue }));
  };

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = event.target;
    setHasChangedTeam(true);
    handleDataChange(name, value);
  };

  const handleUrlChange = (newUrls: string[]) => {
    setHasChangedTeam(true);
    setUrls(newUrls);
  };

  const handleLogoChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith("image/")) {
      setErrors((prev) => [...prev, "logoFile"]);
      return;
    }

    const maxSize = 5 * 1024 * 1024;
    if (file.size > maxSize) {
      setErrors((prev) => [...prev, "logoSize"]);
      return;
    }

    setErrors((prev) => prev.filter((e) => e !== "logoFile" && e !== "logoSize"));
    setLogoFile(file);
    setHasChangedTeam(true);

    const reader = new FileReader();
    reader.onloadend = () => {
      setLogoPreview(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  const handleRemoveLogo = () => {
    setLogoFile(null);
    setLogoPreview(null);
    setHasChangedTeam(true);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const hasError = (dataKey: string): boolean => errors?.includes(dataKey);

  return {
    errors,
    hasChangedTeam,
    newTeam,
    urls,
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
    originalUrls,
  };
};
