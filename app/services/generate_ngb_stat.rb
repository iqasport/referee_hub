module Services
  class GenerateNgbStat
    attr_reader :ngb_id, :start_time, :end_time

    def initialize(ngb_id, end_time)
      @ngb_id = ngb_id
      @end_time = DateTime.parse(end_time)
      @start_time = @end_time - 1.month
    end

    def perform
      raise StandardError, 'ngb_id provided is invalid' unless national_governing_body
      return nil if national_governing_body.stats.by_month(end_time.strftime('%m')).present?

      NationalGoverningBodyStat.create!(
        assistant_referees_count: referee_counts[:assistant],
        community_teams_count: team_counts[:community],   
        competitive_teams_count: team_counts[:competitive], 
        developing_teams_count: team_counts[:developing],  
        end_time: end_time,
        head_referees_count: referee_counts[:head],     
        inactive_teams_count: team_counts[:inactive],
        snitch_referees_count: referee_counts[:snitch],   
        start: start_time,
        team_status_change_count: team_counts[:status_change],
        total_referees_count: referee_counts[:total], 
        total_teams_count: team_counts[:total],       
        uncertified_count: referee_counts[:uncertified],      
        university_teams_count: team_counts[:university],
        youth_teams_count: team_counts[:team_counts],
        national_governing_body_id: ngb_id     
      )
    end

    private

    def national_governing_body
      @national_governing_body ||= NationalGoverningBody.find_by(id: ngb_id)
    end

    def referee_counts
      @referee_counts ||= {
        assistant: referee_scope.assistant.count,
        snitch: referee_scope.snitch.count,
        head: referee_scope.head.count,
        uncertified: referee_scope.uncertified.count,
        total: referee_scope.count
      }
    end

    def team_counts
      @team_counts ||= {
        competitive: team_scope.competitive.count,
        developing: team_scope.developing.count,
        inactive: team_scope.inactive.count,
        university: team_scope.university.count,
        community: team_scope.community.count,
        youth: team_scope.youth.count,
        total: team_scope.count,
        status_change: total_changesets,
      }
    end

    def total_changesets
      @total_changesets ||= team_scope.joins(:team_status_changesets).group('teams.id').count.values.reduce(:+) || 0
    end

    def referee_scope
      @referee_scope ||= national_governing_body.referees.where('users.created_at < ?', end_time)
    end

    def team_scope
      @team_scope ||= national_governing_body.teams.where('teams.joined_at < ?', end_time)
    end
  end
end
