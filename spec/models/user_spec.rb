# == Schema Information
#
# Table name: users
#
#  id                     :bigint           not null, primary key
#  admin                  :boolean          default(FALSE)
#  bio                    :text
#  confirmation_sent_at   :datetime
#  confirmation_token     :string
#  confirmed_at           :datetime
#  current_sign_in_at     :datetime
#  current_sign_in_ip     :inet
#  email                  :string           default(""), not null
#  encrypted_password     :string           default(""), not null
#  export_name            :boolean          default(TRUE)
#  failed_attempts        :integer          default(0), not null
#  first_name             :string
#  invitation_accepted_at :datetime
#  invitation_created_at  :datetime
#  invitation_limit       :integer
#  invitation_sent_at     :datetime
#  invitation_token       :string
#  invitations_count      :integer          default(0)
#  invited_by_type        :string
#  last_name              :string
#  last_sign_in_at        :datetime
#  last_sign_in_ip        :inet
#  locked_at              :datetime
#  pronouns               :string
#  remember_created_at    :datetime
#  reset_password_sent_at :datetime
#  reset_password_token   :string
#  show_pronouns          :boolean          default(FALSE)
#  sign_in_count          :integer          default(0), not null
#  submitted_payment_at   :datetime
#  unlock_token           :string
#  created_at             :datetime         not null
#  updated_at             :datetime         not null
#  invited_by_id          :bigint
#  stripe_customer_id     :string
#
# Indexes
#
#  index_users_on_confirmation_token                 (confirmation_token) UNIQUE
#  index_users_on_email                              (email) UNIQUE
#  index_users_on_invitation_token                   (invitation_token) UNIQUE
#  index_users_on_invitations_count                  (invitations_count)
#  index_users_on_invited_by_id                      (invited_by_id)
#  index_users_on_invited_by_type_and_invited_by_id  (invited_by_type,invited_by_id)
#  index_users_on_reset_password_token               (reset_password_token) UNIQUE
#  index_users_on_unlock_token                       (unlock_token) UNIQUE
#

require 'rails_helper'

RSpec.describe User, type: :model do
  let(:user) { build :user }

  subject { user.save }

  it 'should default to false for show_pronouns attribute' do
    subject
    expect(user.show_pronouns).to eq false
  end

  it 'should create a referee role if one is not passed initially' do
    subject
    expect(user.reload.roles.length).to eq 1
    expect(user.reload.roles.first.access_type).to eq 'referee'
  end

  context 'with an associated NGB' do
    let(:ngb_1) { create :national_governing_body, name: 'United States' }
    let(:ngb_2) { create :national_governing_body, name: 'France' }
    let(:ngb_3) { create :national_governing_body, name: 'Australia' }

    before { user.national_governing_bodies << [ngb_1, ngb_2] }

    it 'returns the correct ngbs' do
      subject
      expect(user.national_governing_bodies.pluck(:id)).to include(ngb_1.id, ngb_2.id)
    end
  end

  context 'with an associated role' do
    let(:user) { build :user, roles_attributes: [{ access_type: 'iqa_admin' }] }

    it 'only creates the passed role' do
      subject
      expect(user.roles.length).to eq 1
      expect(user.roles.first.access_type).to eq 'iqa_admin'
    end
  end

  context '#available_tests' do
    let(:user) { create :user }
    let(:service_double) { double(return_value: :perform) }

    before do
      allow(Services::FindAvailableUserTests).to receive(:new).with(user).and_return(service_double)
      allow(service_double).to receive(:perform).and_return(true)
    end

    subject { user.available_tests }

    it 'calls the service' do
      expect(Services::FindAvailableUserTests).to receive(:new).with(user).once

      subject
    end
  end
end
