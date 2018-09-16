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
    link_data = parsed_data['link']
    results_data = parsed_data['result']

    find_test(test_data) if test_data
    find_link(link_data) if link_data
    update_test_link if @test.link_id.blank?

    create_test_results(results_data) if results_data
  end

  def hmac_header_valid?
    header_val = request.headers['HTTP_X_CLASSMARKER_HMAC_SHA256']
    return false if header_val.blank?

    expected = header_val.split(/,/).first
    actual = calculate_signature(request.raw_post)

    ActiveSupport::SecurityUtils.secure_compare(actual, expected)
  end

  def calculate_signature(data)
    secret = 'a secret string'

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

  def find_test(test_data)
    @test = Test.find_or_create_by(cm_test_id: test_data['test_id'], name: test_data['test_name'])
  end

  def find_link(link_data)
    @link = Link.find_or_create_by(
      cm_link_id: link_data['link_id'],
      cm_link_url_id: link_data['link_url_id'],
      name: link_data['link_name']
    )
  end

  def update_test_link
    @test.update!(link_id: @link.id)
  end

  def create_test_results(results_data)
    test_results_hash = {
      certificate_url: results_data['certificate_url'],
      duration: results_data['duration'],
      minimum_pass_percentage: results_data['percentage_passmark'],
      passed: results_data['passed'],
      percentage: results_data['data'],
      points_available: results_data['points_available'],
      points_scored: results_data['points_scored'],
      time_finished: results_data['time_finished'],
      time_started: results_data['time_started'],
      cm_link_result_id: results_data['link_result_id'],
      link_id: @link.id,
      referee_id: Random.new.rand(100...1000)
    }
    TestResult.create!(test_results_hash)
  end
end