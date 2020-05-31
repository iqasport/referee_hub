class SingleNgbStat < ApplicationJob
  queue_as :mailers

  def perform(ngb_id, end_time)
    Services::GenerateNgbStat.new(ngb_id, end_time).perform
  end
end
