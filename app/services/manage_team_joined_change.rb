module Services
  class ManageTeamJoinedChange
    attr_reader :prev_date, :new_date, :team, :all_stats
    attr_accessor :stats_to_add, :stats_to_remove

    def initialize(prev_date:, new_date:, team:)
      @prev_date = prev_date
      @new_date = new_date
      @team = team
      @all_stats = @team.national_governing_body.stats
    end

    # potential cases
    # 1. new date is before the previous date
    #    - update all later stats except the one that includes prev date, don't remove
    # 2. new date is in the same month as the previous date
    #    - no changes
    # 3. new date is after the previous date
    #    - remove the stats in the diff between dates
    def perform
      new_stat_index, prev_stat_index = find_indexes
      return if new_stat_index == prev_stat_index

      if new_stat_index >= 0
        stats_to_add = most_recent_ordered_stats[prev_stat_index..new_stat_index] if new_stat_index > prev_stat_index
        if prev_stat_index > new_stat_index
          stats_to_remove = most_recent_ordered_stats[new_stat_index..prev_stat_index]
        end
      elsif prev_stat_index > 0
        # remove one from the previous stats, as they are now invalid
        last_stat_date = most_recent_ordered_stats.first.end_time
        if new_date > last_stat_date
          stats_to_remove = most_recent_ordered_stats[0..prev_stat_index]
        else
          # new date is in the past, add to all stats up to the prev stat
          stats_to_add = most_recent_ordered_stats[(prev_stat_index + 1)..-1]
        end
      end

      stats_to_add&.each { |stat| update_stat('add', stat) }
      stats_to_remove&.each { |stat| update_stat('remove', stat) }
    end

    private

    def most_recent_ordered_stats
      @most_recent_ordered_stats ||= all_stats.order(end_time: :desc)
    end

    def includes_date?(stat, date)
      (stat.start..stat.end_time).cover?(date)
    end

    def count_attrs
      @count_attrs ||= {
        team_status: "#{team.status}_teams_count",
        team_group: "#{team.group_affiliation}_teams_count",
      }
    end

    def update_stat(change_type, stat)
      current_status_count = stat.send(count_attrs[:team_status])
      current_group_count = stat.send(count_attrs[:team_group])
      num_to_add = change_type == 'add' ? 1 : -1

      if !team.other?
        stat.assign_attributes(count_attrs[:team_status] => current_status_count + num_to_add)
      end
      if !team.not_applicable?
        stat.assign_attributes(count_attrs[:team_group] => current_group_count + num_to_add)
      end

      stat.assign_attributes(total_teams_count: stat.total_teams_count + num_to_add)
      stat.save!
    end

    def find_indexes(stats = most_recent_ordered_stats)
      new_stat_index = -1
      prev_stat_index = -1

      stats.each_with_index do |stat, index|
        prev_stat_index = index if includes_date?(stat, prev_date)
        new_stat_index = index if includes_date?(stat, new_date)
      end

      return new_stat_index, prev_stat_index
    end
  end
end
