module Services
  class AddRolesToUsers
    attr_reader :users
    attr_accessor :filtered_by_admin

    def initialize(users: [])
      @users = users
    end

    def perform
      @filtered_by_admin = users.group_by(&:admin?)

      create_referee_roles
      create_admin_roles
    end

    private

    def create_referee_roles
      filtered_by_admin[false].each do |user|
        Role.create!(user: user, access_type: 'referee')
      end
    end

    def create_admin_roles
      filtered_by_admin[true].each do |user|
        Role.create!(user: user, access_type: 'referee')
        Role.create!(user: user, access_type: 'iqa_admin')
      end
    end
  end
end
