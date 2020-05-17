class GenerateNgbStats < ApplicationJob
  queue_as :mailers

  def perform
    end_time = 1.day.ago.to_s

    NationalGoverningBody.pluck(:id).each do |ngb_id|
      SingleNgbStat.perform_later(
        ngb_id,
        end_time,
      )
    end
  end
end
