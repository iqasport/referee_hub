class RenameRefereeCertificationIndex < ActiveRecord::Migration[5.2]
  def change
    rename_index :referee_certifications,
      'index_referee_certifications_on_referee_id_and_certification_id',
      'index_referee_certs_on_ref_id_and_cert_id'
  end
end
