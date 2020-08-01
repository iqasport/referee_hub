class BackfillNgbStats < ActiveRecord::Migration[6.0]
  def up
    # start april 2020 until october 2018
    beginning_date = DateTime.new(2020,4,30,23,59,59)
    end_date = DateTime.new(2018,10,31,23,59,59)
    dates = []

    while !dates.include?(end_date)
      if dates.length < 1
        dates.push(beginning_date)
        next
      end

      last_date = dates.last
      next_date = last_date.prev_month.end_of_month
      if last_date.year == 2018 && last_date.month == 11
        next_date = end_date
      end
      dates.push(next_date)
    end

    # grab all ngb ids
    ngb_ids = NationalGoverningBody.all.pluck(:id)

    # iterate over each end time and each ngb
    dates.each do |date|
      ngb_ids.each do |ngb_id|
        # enqueue a job to generate their stats for the date
        SingleNgbStat.perform_later(
          ngb_id,
          date.to_s,
        )
      end
    end
  end

  def down
    NationalGoverningBodyStat.where.not(id: 1).destroy_all
  end
end
