# == Schema Information
#
# Table name: national_governing_body_admins
#
#  id                         :bigint           not null, primary key
#  created_at                 :datetime         not null
#  updated_at                 :datetime         not null
#  national_governing_body_id :bigint           not null
#  user_id                    :bigint           not null
#
# Indexes
#
#  index_national_governing_body_admins_on_ngb_id   (national_governing_body_id)
#  index_national_governing_body_admins_on_user_id  (user_id) UNIQUE
#
# Foreign Keys
#
#  fk_rails_...  (national_governing_body_id => national_governing_bodies.id)
#  fk_rails_...  (user_id => users.id)
#

require 'rails_helper'

RSpec.describe NationalGoverningBodyAdmin, type: :model do
  let!(:user) { create :user }
  let!(:ngb) { create :national_governing_body }
  let(:ngb_admin) { build :national_governing_body_admin, user: user, national_governing_body: ngb }

  subject { ngb_admin.save }

  it 'creates an ngb_admin role' do
    subject

    expect(user.reload.roles.length).to eq 2
    expect(user.ngb_admin?).to be_truthy
  end

  context 'when the user already has an ngb_admin role' do
    let!(:user) { create :user, :ngb_admin }

    it "doesn't create another role" do
      expect { subject }.to_not change { user.reload.roles.length }
    end
  end
end
