class TestAttemptSerializer
  include FastJsonapi::ObjectSerializer

  attributes :next_attempt_at, :test_level
end
