# == Schema Information
#
# Table name: tests
#
#  id               :bigint(8)        not null, primary key
#  level            :integer          default("snitch")
#  name             :string
#  created_at       :datetime         not null
#  updated_at       :datetime         not null
#  certification_id :integer
#  cm_test_id       :integer
#  link_id          :integer
#
# Indexes
#
#  index_tests_on_cm_test_id  (cm_test_id) UNIQUE
#

require 'rails_helper'

RSpec.describe Test, type: :model do
  let!(:certification) { create :certification, :snitch }
  let(:test) { build :test }

  subject { test.save }

  context 'when level has changed' do
    it 'connects the correct certification' do
      expect { subject }.to change { test.certification_id }.to(certification.id)
    end
  end
end
