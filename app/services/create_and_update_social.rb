module Services
  class CreateAndUpdateSocial
    attr_reader :action, :owner
    attr_accessor :urls

    InvalidActionError = Class.new(StandardError)
    OwnerMissingError = Class.new(StandardError)
    ACTION_ERROR = 'Must provide a valid action'.freeze
    OWNER_ERROR = 'Must provide an owner if doing an update action'.freeze
    VALID_ACTIONS = %i[create update].freeze

    def initialize(urls, action, owner = nil)
      @urls = urls
      @action = action
      @owner = owner
    end

    def perform
      return nil if urls.blank?
      raise InvalidActionError, ACTION_ERROR unless VALID_ACTIONS.include?(action)
      raise OwnerMissingError, OWNER_ERROR unless valid_owner?

      @urls = update_social_accounts if update_action?

      generate_social_accounts
    end

    private

    def update_action?
      action == :update
    end

    def create_action?
      action == :create
    end

    def valid_owner?
      create_action? || (update_action? && owner.present?)
    end

    def generate_social_accounts
      urls.map do |url|
        SocialAccount.new(
          url: url,
          account_type: SocialAccount.match_account_type(url)
        )
      end
    end

    def update_social_accounts
      existing_urls = owner.social_accounts.pluck(:url)
      urls_to_remove = existing_urls - urls
      urls_to_add = urls - existing_urls

      SocialAccount.where(url: urls_to_remove).destroy_all if urls_to_remove.present?

      urls_to_add
    end
  end
end
