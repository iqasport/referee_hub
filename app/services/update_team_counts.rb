module Services
  class UpdateTeamCounts
    attr_reader :stat

    def initialize(stat)
      @stat = stat
    end

    def perform
      stat.update!(
        competitive_teams_count: team_scope.competitive.count,
        developing_teams_count: team_scope.developing.count,
        inactive_teams_count: team_scope.inactive.count,
        university_teams_count: team_scope.university.count,
        community_teams_count: team_scope.community.count,
        youth_teams_count: team_scope.youth.count,
        total_teams_count: team_scope.count
      )
    end

    private

    def team_scope
      @team_scope ||= stat.national_governing_body.teams.where('teams.joined_at < ?', stat.end_time)
    end
  end
end
