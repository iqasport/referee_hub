# == Schema Information
#
# Table name: exported_csvs
#
#  id             :bigint           not null, primary key
#  export_options :json             not null
#  processed_at   :datetime
#  sent_at        :datetime
#  type           :string
#  url            :string
#  created_at     :datetime         not null
#  updated_at     :datetime         not null
#  user_id        :integer          not null
#
# Indexes
#
#  index_exported_csvs_on_user_id  (user_id)
#

FactoryBot.define do
  factory :exported_csv do
    type { 'ExportedCsv::TeamExport' }
    user
  end
end
