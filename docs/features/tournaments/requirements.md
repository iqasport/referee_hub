## Motivation

The Management Hub currently provides functionality to Referees and NGB Admins.
As we look to provide more useful functionality to a wider set a of people engaged with Quadball, we want to enable players to register in the Hub and for tournament scheduling and discovery to take place in the Hub.

Currently tournaments advertise themselves on Facebook groups and use google spreadsheets to collect signups from teams/individuals.
Then teams are asked to fill out spreadsheets with their rosters (players participating) and send those over email to tournament organizers.
The teams also need to ???

By creating a unified experience for tournament management we hope to reduce the cost for both tournament organizers and participating teams, while enabling new organizers a lower barrier of entry for creating a tournament.

## Scope

This document will describe the changes in the Hub backend to facilitate the new experience.
Where things are designed for future expansion there will be a note indicating it.

## Models

### Tournament

Let's start by describing what a Tournament entity holds.

* Id - a unique identifier of the tournament
    * Requires creation of new ID struct with a backing ULID similarly to other IDs - prefix `TR`, no legacy id
* Name - string, tournament name
* Description - string, tournament description - we could maybe enable markdown formatting here?
* StartDate - date, when the tournament starts
* EndDate - date, when the tournament ends (could be same as StartDate for single day events)
* Type - enum, tournament type (Club, National, Youth, Fantasy)
* Location - struct { Country, City, Place }
* Private - bool, if a tournament is private it should only be visible to organizers and participants, new participants are by invitation only

The Tournament will have a collection of TournamentParticipant entities, which is an abstract concept. Right now we will only implement one possible participant - an existing team, while in the future we will enable individual participants and creating virtual teams composed of those.

The TournamentTeamParticipant is a complex object containing both a reference to a Team entity and a selected roster of players from that team participating in this tournament:

* TeamId - team identifier
* TeamName - string, name of the team
* Roster - struct { Players, Coaches, Staff } - lists of user references, where a user reference is a struct { UserId, DisplayName }, however, players also needs to include { Number, Gender } where number is the player's number as present on their jersey (represented as string) and gender is the player's identified gender (represented as string) for the purpose of applying gender rule as per Quadball rules.

We need to note that Gender is a sensitive information about a player and should only be visible to tournament manager and team manager of the participating team.
Also the way we store gender in the database needs to allow easy removal requests (i.e. no duplication) and scheduled deletion after a time period.
We will assume that a user has only one gender they identify with at a time.

The way teams participate in a tournament is either by an invitation from a tournament manager or by a join request initiated by the team manager.
As with TournamentParticipant we need to make those extensible for the future (e.g. once we introduce individual participants).
Maybe we can form a joint structure for TournamentInvite:

* TournamentId
* ParticipantType - "team" initially
* ParticipantId - teamId as string initially
* Initiator - user reference
* CreatedAt - date
* TournamentManagerApproval - struct {ApprovalStatus, LastModifiedDate}
* ParticipantApproval - struct {ApprovalStatus, LastModifiedDate}
* Status - derived from approval statuses of the manager and participant (rejected if at least one rejected, approved if both approved, pending otherwise)

If the participant initiates the join request we get an invite object which is pending tournament manager approval, otherwise manager approval is granted on invite and pending participant accepting invite (providing approval).
Only once an invite is fully approved we add a participant entry.

Invitations should be restricted based on tournament type - i.e. only club teams can join a club tournament, only national teams can join a national tournament, any team can join a fantasy tournament.
We need to add a national team type to TeamGroupAffiliation, and note that we consider both university and community teams club teams.

TournamentManager and TeamManager would be new user roles, parameterized by TournamentId and TeamId appropriately.
As with the NgbAdmin roles, we need a way to express ID matching within a set of values or through an ALL param for site admins.

A tournament should have a banner image - managed similarly to user avatar.

Once a tournament is past its end date it should be considered archived and only the managers are able to modify the tournament object.
Changes to rosters or invitations should be blocked for an archived tournament.

## Database storage

We want to introduce new tables:

* tournaments
* tournament_managers
* tournament_team_participants
* tournament_team_roster_entries
* user_delicate_info (for gender with a last update timestamp)
* tournament_invites

This would be configured with new classes is the Data folder in the Storage project and explicit declarations in the EF DbContext.

## API Endpoints

1. GET `/api/v2/tournaments` - list of tournaments with filter over tournament name and pagination (shared mvc filter)
    * make sure to cover filtering of private tournaments to participants only
2. GET `/api/v2/tournaments/{tournamentId}` - fetching a single tournament
    * if user shouldn't have access, return 404
    * should adhere to the following view model schema:
    ```json
    {
        "id": 1,
        "title": "Lorum ipsum",
        "description": "Longer lorum ipsum",
        "startDate": "2025-12-12",
        "endDate": "2025-12-13",
        "type": "lorum ipsum",
        "country": "Belgium",
        "location": "lorum ipsum",
        "bannerImageUrl": "https://...",
        "organizers": [
            {
                "id": "user-001",
                "name": "Jane Smith"
            }
        ]
    }
    ```
3. POST `/api/v2/tournaments` - create new tournament and become its manager - returns new tournamentId
4. PUT `/api/v2/tournaments/{tournamentId}` - update tournament details (manager only) - updates the entire tournament object
5. PUT `/api/v2/tournaments/{tournamentId}/banner` - upload/update tournament banner image (manager only)
6. GET `/api/v2/tournaments/{tournamentId}/managers` - list of managers of this tournament, including their emails
    * only for managers
7. POST `/api/v2/tournaments/{tournamentId}/managers`- manager can add another manager to help with tournament
8. DELETE `/api/v2/tournaments/{tournamentId}/managers/{userId}` - manager can remove another manager
9. GET `/api/v2/tournaments/{tournamentId}/participants` - list the participants of the tournament
    * should adhere to the following view model schema:
    ```json
    [
        {
            "teamId": "team-001",
            "teamName": "Dragons",
            "type": "team",
            "players": [
                {
                    "userId": "player-001",
                    "userName": "John Doe",
                    "number": "24",
                    "gender": "female",
                }
            ],
            "coaches": [
                {
                    "userId": "user-002",
                    "userName": "John Doe",
                }
            ],
            "staff": [
                {
                    "userId": "user-003",
                    "userName": "John Doe",
                }
            ]
        }
    ]
    ```
10. PUT `/api/v2/tournaments/{tournamentId}/participants/{teamId}` - team manager can modify the roster (players, coaches, staff)
11. DELETE `/api/v2/tournaments/{tournamentId}/participants/{teamId}` - tournament manager can remove a participant - this should allow the removed team to be re-invited later
12. GET `/api/v2/tournaments/{tournamentId}/invites` - manager can see all invites and join requests to the tournament, a participant can see their invite
13. POST `/api/v2/tournaments/{tournamentId}/invites/{participantId}` - manager or participant submits their approval or rejection to the invite (only allowed when their current status is pending)
    * if there's more than one invite, there should only be one pending invite and that's the one we'd modify
14. POST `/api/v2/tournaments/{tournamentId}/invites` - adding new invite
    * the user initiating this needs to be a team manager for the team requesting to join or the tournament manager
    * if the user has both roles, the invite should be approved straight away
    * not allowed if a pending invite for that participant already exists

## Mailing
Whenever a new invite is created or approved/rejected we want to send an email to both parties (managers of tournament and managers of team) to inform them of it.

## Implementation phases
Because this is a lot of changes we want to separate them into vertical slices to be delivered in separate PRs.

* Phase 1: Get and create tournament, new tournament manager role
* Phase 2: Manage tournament managers
* Phase 3: Manage invites and create participants
* Phase 4: Manage team rosters
