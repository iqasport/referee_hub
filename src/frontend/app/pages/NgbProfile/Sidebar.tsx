import {
  faFacebookSquare,
  faInstagramSquare,
  faTwitterSquare,
  faYoutubeSquare
} from "@fortawesome/free-brands-svg-icons";
import { faComments, faUser } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import classnames from "classnames";
import { capitalize, words } from "lodash";
import React from "react";

import DataLabel from "../../components/DataLabel";
import UploadedImage from "../../components/UploadedImage";
import { NgbInfoViewModelRead, SocialAccount, useUpdateNgbAvatarMutation } from "../../store/serviceApi";

type SocialConfig = {
  [key: string]: {
    color: string;
    icon: typeof faFacebookSquare;
  };
};

const socialConfig: SocialConfig = {
  facebook: {
    color: "hover:text-blue",
    icon: faFacebookSquare,
  },
  instagram: {
    color: "hover:text-pink-400",
    icon: faInstagramSquare,
  },
  other: {
    color: "hover:text-green",
    icon: faComments,
  },
  twitter: {
    color: "hover:text-blue",
    icon: faTwitterSquare,
  },
  youtube: {
    color: "hover:text-red-600",
    icon: faYoutubeSquare,
  },
};

type SidebarProps = {
  ngb: NgbInfoViewModelRead;
};

const Sidebar = (props: SidebarProps) => {
  const { ngb } = props;

  const [updateNgbAvatar] = useUpdateNgbAvatarMutation();

  const handleLogoUpdate = (file: File) => {
    // at the moment RTK Query code gen doesn't support multipart form requests
    const payload = new FormData();
    payload.append("avatarBlob", file);
    fetch(`/api/v2/Ngbs/${ngb.countryCode}/avatar`, {
      method: "PUT",
      // let the browser set Content-Type header based on the payload
      body: payload,
    }).then(() => {
      // invoke a call that results in 415 Media type not supported, but invalidates the cache and will make another call to get uploaded avatar url
      updateNgbAvatar({body: {}, ngb: ngb.countryCode})
    });
  };

  const renderSocialMedia = (account: SocialAccount, index) => {
    const iconConfig = socialConfig[account.type];
    return (
      <a
        key={`${account.type}-${index}`}
        href={account.url}
        target="_blank"
        rel="noopener noreferrer"
        className={classnames("mr-4", iconConfig.color)}
      >
        <FontAwesomeIcon icon={iconConfig.icon} className="text-3xl" />
      </a>
    );
  };

  const renderAdminEmail = (email: string, index) => {
    return (
      <a
        key={`email-${index}`}
        href={`mailto:${email}`}
        className={classnames("mr-4", "hover:underline")}
        style={{overflow: "hidden"}}
      >
        <FontAwesomeIcon icon={faUser} className="text-xl" />
        {` ${email}`}
      </a>
    );
  }

  return (
    <div className="flex flex-row flex-wrap mb-4 md:m-0 md:flex-col w-full md:w-1/4 md:border-r-2 md:border-gray-700 md:pr-8">
      <div className="flex justify-center">
        <UploadedImage
          imageAlt="national governing body logo"
          imageUrl={ngb.avatarUri}
          onSubmit={handleLogoUpdate}
          isEditable={true}
        />
      </div>
      <div className="w-full flex flex-row justify-between mt-8">
        <DataLabel label="teams" customClass="flex-shrink">
          <h3 className="uppercase text-navy-blue font-extrabold pt-2 text-2xl">{ngb.currentStats.totalTeamsCount}</h3>
        </DataLabel>
        <DataLabel label="referees" customClass="flex-shrink">
          <h3 className="uppercase text-navy-blue font-extrabold pt-2 text-2xl">{ngb.currentStats.totalRefereesCount}</h3>
        </DataLabel>
        <DataLabel label="players" customClass="flex-shrink">
          <h3 className="uppercase text-navy-blue font-extrabold pt-2 text-2xl">
            {ngb.playerCount}
          </h3>
        </DataLabel>
      </div>
      <div className="w-full flex flex-row justify-between">
        <DataLabel label="acronym" customClass="flex-shrink">
          <h3 className="uppercase text-navy-blue font-bold pt-2">{ngb.acronym}</h3>
        </DataLabel>
        <DataLabel label="membership status" customClass="flex-shrink">
          <h3 className="text-navy-blue font-bold pt-2">
            {words(ngb.membershipStatus)
              .map((word) => capitalize(word))
              .join(" ")}
          </h3>
        </DataLabel>
        <DataLabel label="region" customClass="flex-shrink">
          <h3 className="text-navy-blue font-bold pt-2">
            {words(ngb.region)
              .map((word) => capitalize(word))
              .join(" ")}
          </h3>
        </DataLabel>
      </div>
      <DataLabel label="website" customClass="w-full">
        <h3 className="text-navy-blue font-bold pt-2 truncate hover:underline">
          <a href={ngb.website} rel="noopener noreferrer" target="_blank">
            {ngb.website}
          </a>
        </h3>
      </DataLabel>
      <DataLabel label="social media" customClass="w-full">
        <div className="flex w-full mt-2 flex-wrap">{ngb.socialAccounts.map(renderSocialMedia)}</div>
      </DataLabel>
      <DataLabel label="admin emails" customClass="w-full">
        <div className="flex w-full mt-2 flex-wrap">{ngb.adminEmails.map(renderAdminEmail)}</div>
      </DataLabel>
    </div>
  );
};

export default Sidebar;
