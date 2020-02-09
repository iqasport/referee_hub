# == Schema Information
#
# Table name: exported_csvs
#
#  id             :bigint(8)        not null, primary key
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

require 'rails_helper'

RSpec.describe ExportedCsv, type: :model do
  pending "add some examples to (or delete) #{__FILE__}"
end
