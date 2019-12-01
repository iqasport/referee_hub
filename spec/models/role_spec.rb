# == Schema Information
#
# Table name: roles
#
#  id          :bigint(8)        not null, primary key
#  access_type :integer          default("referee"), not null
#  created_at  :datetime         not null
#  updated_at  :datetime         not null
#  user_id     :bigint(8)
#
# Indexes
#
#  index_roles_on_user_id                  (user_id)
#  index_roles_on_user_id_and_access_type  (user_id,access_type) UNIQUE
#
# Foreign Keys
#
#  fk_rails_...  (user_id => users.id)
#

require 'rails_helper'

RSpec.describe Role, type: :model do
  let(:user) { create :user, disable_ensure_role: true }
  let(:role) { build :role, user: user }

  subject { role.valid? }

  it { expect(subject).to be_truthy }

  context 'when a user already has a role' do
    let(:user) { create :user }
    let!(:existing_role) { create :role, user: user, access_type: 'ngb_admin' }
    let(:role) { build :role, user: user, access_type: 'ngb_admin' }

    it { expect(subject).to be_falsey }
  end
end
