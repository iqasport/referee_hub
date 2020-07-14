# == Schema Information
#
# Table name: certifications
#
#  id           :bigint           not null, primary key
#  display_name :string           default(""), not null
#  level        :integer          not null
#  version      :integer          default("eighteen")
#  created_at   :datetime         not null
#  updated_at   :datetime         not null
#
# Indexes
#
#  index_certifications_on_level_and_version  (level,version) UNIQUE
#

require 'rails_helper'

RSpec.describe Certification, type: :model do
  let(:certification) { build :certification }

  subject { certification.save }

  it 'is a valid object' do
    expect(certification).to be_valid
    expect { subject }.to_not raise_error
  end
end
