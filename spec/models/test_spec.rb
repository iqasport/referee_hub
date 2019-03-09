# == Schema Information
#
# Table name: tests
#
#  id                      :bigint(8)        not null, primary key
#  description             :text             not null
#  language                :string
#  level                   :integer          default("snitch")
#  minimum_pass_percentage :integer          default(80), not null
#  name                    :string
#  negative_feedback       :text
#  positive_feedback       :text
#  time_limit              :integer          default(18), not null
#  created_at              :datetime         not null
#  updated_at              :datetime         not null
#  certification_id        :integer
#

require 'rails_helper'

RSpec.describe Test, type: :model do
  subject { test.save }

  context 'when level has changed' do
    context 'and the certification does not match the level' do
      let!(:bad_cert) { create :certification }
      let!(:good_cert) { create :certification, :snitch }
      let(:test) { build :test, certification: bad_cert }

      it 'connects the correct certification' do
        expect { subject }.to change { test.certification_id }.to(good_cert.id)
      end
    end
  end
end
