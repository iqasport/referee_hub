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
        className={classnames("mr-4 text-sm text-gray-700 hover:underline")}
        style={{overflow: "hidden"}}
      >
        <FontAwesomeIcon icon={faUser} className="text-xl" />
        {` ${email}`}
      </a>
    );
  }

  return (
    <aside className="card card-mb card-sticky">
      <div className="flex justify-center mb-4">
        <UploadedImage
          imageAlt="national governing body logo"
          imageUrl={ngb.avatarUri}
          onSubmit={handleLogoUpdate}
          isEditable={true}
        />
      </div>

      <div className="stats-list card-mb">
        <div className="stats-item">
          <span className="stats-label">Teams</span>
          <span className="stats-value">{ngb.currentStats.totalTeamsCount}</span>
        </div>
        <div className="stats-item">
          <span className="stats-label">Referees</span>
          <span className="stats-value">{ngb.currentStats.totalRefereesCount}</span>
        </div>
        <div className="stats-item">
          <span className="stats-label">Players</span>
          <span className="stats-value">{ngb.playerCount}</span>
        </div>
      </div>

      <div className="stats-list card-mb">
        <div className="stats-item">
          <span className="stats-label">Acronym</span>
          <span className="stats-value uppercase">{ngb.acronym}</span>
        </div>
        <div className="stats-item">
          <span className="stats-label">Membership</span>
          <span className="stats-value">
            {words(ngb.membershipStatus)
              .map((word) => capitalize(word))
              .join(" ")}
          </span>
        </div>
        <div className="stats-item">
          <span className="stats-label">Region</span>
          <span className="stats-value">
            {words(ngb.region)
              .map((word) => capitalize(word))
              .join(" ")}
          </span>
        </div>
      </div>

      <DataLabel label="website" customClass="w-full mt-0 mb-4">
        <h3 className="text-sm text-gray-800 font-semibold pt-2 truncate hover:underline">
          <a href={ngb.website} rel="noopener noreferrer" target="_blank">
            {ngb.website}
          </a>
        </h3>
      </DataLabel>

      <DataLabel label="social media" customClass="w-full mt-0 mb-4">
        <div className="flex w-full mt-2 flex-wrap">{ngb.socialAccounts.map(renderSocialMedia)}</div>
      </DataLabel>

      <DataLabel label="admin emails" customClass="w-full mt-0">
        <div className="flex w-full mt-2 flex-wrap">{ngb.adminEmails.map(renderAdminEmail)}</div>
      </DataLabel>
    </aside>
  );
};

export default Sidebar;
