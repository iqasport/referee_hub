# == Schema Information
#
# Table name: referees
#
#  id                     :bigint(8)        not null, primary key
#  bio                    :text
#  current_sign_in_at     :datetime
#  current_sign_in_ip     :inet
#  email                  :string           default(""), not null
#  encrypted_password     :string           default(""), not null
#  first_name             :string
#  last_name              :string
#  last_sign_in_at        :datetime
#  last_sign_in_ip        :inet
#  pronouns               :string
#  remember_created_at    :datetime
#  reset_password_sent_at :datetime
#  reset_password_token   :string
#  show_pronouns          :boolean          default(FALSE)
#  sign_in_count          :integer          default(0), not null
#  created_at             :datetime         not null
#  updated_at             :datetime         not null
#
# Indexes
#
#  index_referees_on_email                 (email) UNIQUE
#  index_referees_on_reset_password_token  (reset_password_token) UNIQUE
#

require 'rails_helper'

RSpec.describe Referee, type: :model do
  let(:referee) { build :referee }

  subject { referee.save }

  it 'should default to false for show_pronouns attribute' do
    subject
    expect(referee.show_pronouns).to eq false
  end
end
