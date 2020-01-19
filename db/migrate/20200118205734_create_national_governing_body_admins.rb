class CreateNationalGoverningBodyAdmins < ActiveRecord::Migration[5.2]
  def change
    create_table :national_governing_body_admins do |t|
      t.references :user, foreign_key: true, index: { unique: true }, null: false
      t.references :national_governing_body,
        foreign_key: true,
        index: { name: 'index_national_governing_body_admins_on_ngb_id' },
        null: false

      t.timestamps
    end
  end
end
