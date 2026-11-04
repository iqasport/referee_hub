# Team transfer records

A playing-team request is a transfer only when its origin and destination are
different teams. The origin is the player's current team, or the team from their
most recent removal if they currently have no team.

Returning to that same team is an ordinary join request. It still requires team
manager approval unless the team auto-approves requests, but creates no NGB
transfer approval or transfer-approval notification.

The `RemoveSameTeamTransfers` data migration permanently deletes NGB transfer
approvals whose invitation has the same origin and destination team. It also
clears the invitation's origin and internal-transfer markers. Invitations,
their response timestamps, memberships, and player activity history are preserved.
This cleanup applies to pending and completed records and cannot be undone by
rolling back the migration. Genuine transfers and joins without an origin are
unchanged.
