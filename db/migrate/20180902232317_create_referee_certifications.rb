class CreateRefereeCertifications < ActiveRecord::Migration[5.2]
  def change
    create_table :referee_certifications do |t|
      t.integer :referee_id, null: false
      t.integer :certification_id, null: false
      t.datetime :received_at
      t.datetime :revoked_at
      t.datetime :renewed_at

      t.timestamps
    end

    add_index :referee_certifications, %i[referee_id certification_id], where: '(revoked_at IS NULL)', unique: true
  end
end
