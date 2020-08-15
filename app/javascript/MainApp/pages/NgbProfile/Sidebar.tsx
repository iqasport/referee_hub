import {
  faFacebookSquare,
  faInstagramSquare,
  faTwitterSquare,
  faYoutubeSquare
} from '@fortawesome/free-brands-svg-icons'
import { faComments } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import classnames from 'classnames'
import { capitalize, words } from 'lodash'
import React from 'react'
import { useDispatch } from 'react-redux'

import DataLabel from '../../components/DataLabel'
import UploadedImage from '../../components/UploadedImage'
import { updateNgbLogo } from '../../modules/nationalGoverningBody/nationalGoverningBody'
import { DataAttributes, IncludedAttributes } from '../../schemas/getNationalGoverningBodySchema'

type SocialConfig = {
  [key: string]: {
    color: string;
    icon: typeof faFacebookSquare;
  }
}

const socialConfig: SocialConfig = {
  'facebook': {
    color: 'hover:text-blue-400',
    icon: faFacebookSquare,
  },
  'instagram': {
    color: 'hover:text-pink-400',
    icon: faInstagramSquare,
  },
  'other': {
    color: 'hover:text-green',
    icon: faComments
  },
  'twitter': {
    color: 'hover:text-blue',
    icon: faTwitterSquare,
  },
  'youtube': {
    color: 'hover:text-red-600',
    icon: faYoutubeSquare,
  }
}

type SidebarProps = {
  ngb: DataAttributes;
  socialAccounts: IncludedAttributes[];
  teamCount: number;
  refereeCount: number;
  ngbId: string;
  isEditing: boolean;
}

const Sidebar = (props: SidebarProps) => {
  const { ngb, socialAccounts, teamCount, refereeCount, ngbId } = props
  const dispatch = useDispatch()

  const handleLogoUpdate = (file: File) => {
    dispatch(updateNgbLogo(ngbId, file))
  }

  const renderSocialMedia = (account: IncludedAttributes, index) => {
    const iconConfig = socialConfig[account.accountType]
    return (
      <a
        key={`${account.accountType}-${index}`}
        href={account.url}
        target="_blank"
        className={classnames("mr-4", iconConfig.color)}
      >
        <FontAwesomeIcon icon={iconConfig.icon} className="text-3xl" />
      </a>
    )
  }

  return (
    <div
      className=
      "flex flex-row flex-wrap mb-4 md:m-0 md:flex-col w-full md:w-1/4 md:border-r-2 md:border-gray-700 md:pr-8"
    >
      <div className="flex justify-center">
        <UploadedImage
          imageAlt="national governing body logo"
          imageUrl={ngb.logoUrl}
          onSubmit={handleLogoUpdate}
          isEditable={true}
        />
      </div>
      <div className="w-full flex flex-row justify-between mt-8">
        <DataLabel label="teams" customClass="flex-shrink">
          <h3 className="uppercase text-navy-blue font-extrabold pt-2 text-2xl">{teamCount}</h3>
        </DataLabel>
        <DataLabel label="referees" customClass="flex-shrink">
          <h3 className="uppercase text-navy-blue font-extrabold pt-2 text-2xl">{refereeCount}</h3>
        </DataLabel>
        <DataLabel label="players" customClass="flex-shrink">
          <h3 className="uppercase text-navy-blue font-extrabold pt-2 text-2xl">{ngb.playerCount}</h3>
        </DataLabel>
      </div>
      <div className="w-full flex flex-row justify-between">
        <DataLabel label="acronym" customClass="flex-shrink">
          <h3 className="uppercase text-navy-blue font-bold pt-2">{ngb.acronym}</h3>
        </DataLabel>
        <DataLabel label="membership status" customClass="flex-shrink">
          <h3 className="text-navy-blue font-bold pt-2">
            {words(ngb.membershipStatus).map(word => capitalize(word)).join(' ')}
          </h3>
        </DataLabel>
        <DataLabel label="region" customClass="flex-shrink">
          <h3 className="text-navy-blue font-bold pt-2">{words(ngb.region).map(word => capitalize(word)).join(' ')}</h3>
        </DataLabel>
      </div>
      <DataLabel label="website" customClass="w-full">
        <h3 className="text-navy-blue font-bold pt-2 truncate">
          <a href={ngb.website} target="_blank">{ngb.website}</a>
        </h3>
      </DataLabel>
      <DataLabel label="social media" customClass="w-full">
        <div className="flex w-full mt-2 flex-wrap">
          {socialAccounts.map(renderSocialMedia)}
        </div>
      </DataLabel>
    </div>
  )
}

export default Sidebar
