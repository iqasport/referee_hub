class TestResultSerializer
  include FastJsonapi::ObjectSerializer

  attributes :duration,
             :minimum_pass_percentage,
             :passed,
             :percentage,
             :points_available,
             :points_scored,
             :time_finished,
             :time_started

  attribute :test_level do |test_result, _params|
    test_result&.link&.test&.level
  end
end
