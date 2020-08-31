import { capitalize } from 'lodash'
import { DateTime } from 'luxon'
import React, { useEffect } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux'

import { IdAttributes } from 'MainApp/apis/referee'
import { getCertifications } from 'MainApp/modules/certification/certifications'
import { createCertification, revokeCertification } from 'MainApp/modules/certification/refCertification'
import { fetchReferee } from 'MainApp/modules/referee/referee'
import { RootState } from 'MainApp/rootReducer'
import { Datum } from 'MainApp/schemas/getCertificationsSchema'
import { getVersion } from 'MainApp/utils/newCertUtils'

import Modal, { ModalProps, ModalSize } from '../Modal/Modal'

type Omitted = 'showClose' | 'size'

interface AdminCertificationsModalProps extends Omit<ModalProps, Omitted> {
  refereeId: string;
}

const AdminCertificationsModal = (props: AdminCertificationsModalProps) => {
  const { open, refereeId, onClose } = props

  const dispatch = useDispatch()
  const { certifications, id } = useSelector((state: RootState) => state.referee, shallowEqual)
  const { certifications: allCerts } = useSelector((state: RootState) => state.certifications, shallowEqual)

  const refCertIds = certifications.map((cert) => cert.id)

  useEffect(() => {
    if (id !== refereeId) {
      dispatch(fetchReferee(refereeId))
    }
  }, [id, refereeId])

  useEffect(() => {
    if (!allCerts) {
      dispatch(getCertifications())
    }
  }, [allCerts])

  const handleAddClick = (certificationId: string) => () => {
    const receivedAt = DateTime.local().toString()
    const newCert = { certificationId, refereeId, receivedAt }

    dispatch(createCertification(newCert))
  }

  const handleRevokeClick = (certificationId: string) => () => {
    const revokedAt = DateTime.local().toString()
    const updatedCert = { revokedAt, refereeId }

    dispatch(revokeCertification(updatedCert, certificationId))
  }

  const renderCert = (cert: IdAttributes, isRefCert: boolean) => {
    return (
      <div key={cert.id} className="w-full rounded bg-white p-2 flex justify-between items-center my-2">
        <div className="w-1/2 flex justify-between items-center">
          <h3 className="text-xl font-bold text-navy-blue">{capitalize(cert.level)}</h3>
          <h4 className="text-lg text-navy-blue">{`Rulebook ${getVersion(cert.version)}`}</h4>
        </div>
        <div className="w-1/2 flex justify-end">
          {isRefCert && (
            <button
              className="bg-red-500 rounded text-white py-2 px-4"
              onClick={handleRevokeClick(cert.id)}
            >
              Revoke
            </button>
          )}
          {!isRefCert && (
            <button
              className="bg-blue-darker rounded text-white py-2 px-4"
              onClick={handleAddClick(cert.id)}
            >
              Add
            </button>
          )}
        </div>
      </div>
    )
  }

  const renderRefCertification = (certification: IdAttributes) => renderCert(certification, true)

  const renderCertification = (certification: Datum) => {
    if (refCertIds.includes(certification.id)) return null

    return renderCert({id: certification.id, ...certification.attributes}, false)
  }

  return (
    <Modal open={open} showClose={true} onClose={onClose} size={ModalSize.Large}>
      <h1>Manage Referee Certifications</h1>
      <div className="w-full p-4">
        {certifications?.map(renderRefCertification)}
        {allCerts?.map(renderCertification)}
      </div>
    </Modal>
  )
}

export default AdminCertificationsModal
