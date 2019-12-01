require 'rails_helper'

describe Services::AddRolesToUsers do
  let(:users) { create_list :user, 5, disable_ensure_role: true }
  let(:admin_users) { create_list :user, 3, admin: true, disable_ensure_role: true }
  let(:combined_users) { users.concat(admin_users) }

  subject { described_class.new(users: combined_users).perform }

  it 'creates referee users' do
    expect { subject }.to change { users.first.roles.count }.by(1)
  end

  it 'creates iqa admin users' do
    expect { subject }.to change { admin_users.first.roles.count }.by(2)
  end
end
