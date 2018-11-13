require 'base64'
require 'openssl'

class ClassmarkerController < ApplicationController
  # no need for CSRF
  skip_before_action :verify_authenticity_token

  before_action :verify_hmac_signature
  before_action :verify_payload_json

  def webhook
    save_webhook_data request.raw_post
    head :ok
  end

  private

  class InvalidPayloadError < StandardError
    def initialize
      super 'Payload must be valid JSON'
    end
  end

  class InvalidHMACError < StandardError
    def initialize
      super 'Invalid HMAC signature'
    end
  end

  def verify_hmac_signature
    raise InvalidHMACError unless hmac_header_valid?
  end

  def verify_payload_json
    raise InvalidPayloadError unless payload_json?
  end

  def save_webhook_data(data)
    parsed_data = JSON.parse(data)
    test_data = parsed_data['test']
    results_data = parsed_data['result']
    referee = Referee.find_by(id: results_data['cm_user_id'])
    test_level = determine_level(test_data['test_name'])

    create_test_attempt(test_level, referee) if referee.present?
    create_test_results(results_data, test_level, referee) if referee.present?
  end

  def hmac_header_valid?
    header_val = request.headers['HTTP_X_CLASSMARKER_HMAC_SHA256']
    return false if header_val.blank?

    expected = header_val.split(/,/).first
    actual = calculate_signature(request.raw_post)

    ActiveSupport::SecurityUtils.secure_compare(actual, expected)
  end

  def calculate_signature(data)
    secret = 'EaQjqazCRdSbbjQ'

    digest = OpenSSL::Digest.new('sha256')
    Base64.encode64(OpenSSL::HMAC.digest(digest, secret, data)).strip
  end

  def payload_json?
    JSON.parse(request.raw_post)
    true
  rescue
    false
  end

  def timestamped_filename(extension = '.json')
    Time.zone.now.strftime('%Y-%m-%d_%H-%M-%S') + extension
  end

  def create_test_results(results_data, test_level, referee)
    test_results_hash = {
      certificate_url: results_data['certificate_url'],
      duration: results_data['duration'],
      minimum_pass_percentage: results_data['percentage_passmark'],
      passed: results_data['passed'],
      percentage: results_data['percentage'],
      points_available: results_data['points_available'],
      points_scored: results_data['points_scored'],
      time_finished: results_data['time_finished'],
      time_started: results_data['time_started'],
      cm_link_result_id: results_data['link_result_id'],
      test_level: test_level,
      referee_id: referee.id
    }
    TestResult.create(test_results_hash)
  rescue => e
    logger.error e
  end

  def create_test_attempt(test_level, referee)
    data_hash = {
      test_level: test_level,
      referee_id: referee.id
    }

    TestAttempt.create(data_hash)
  rescue => e
    logger.error e
  end

  def determine_level(test_name)
    return 'snitch' if /snitch/i.match?(test_name)
    return 'assistant' if /assistant/i.match?(test_name)
    return 'head' if /head/i.match?(test_name)

    'snitch'
  end

  def in_cool_down_period?(referee, test_level)
    return false unless referee

    # if more than one test_attempt exists for the test level and the latest attempt is in the cool down period
    latest_test_attempt = referee.test_attempts.where(test_level: test_level).order('created_at DESC').first

    latest_test_attempt.try(:in_cool_down_period?, false)
  end
end
