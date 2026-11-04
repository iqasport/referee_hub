# Team-manager assignment notifications

When an NGB admin or team manager assigns a team manager, the notification says
`You can now manage team <team name>.` The team identifier remains in the related
entity fields for navigation, but is not used as the display name.

The notification service resolves the name when creating the notification.
If the team does not exist, creation fails rather than saving an ID-only message.
Subsequent team renames do not rewrite notification history.

The `UseTeamNamesInManagerNotifications` data migration updates existing
team-manager assignment messages that exactly match the old ID-based template.
It preserves notification IDs, dates, read/archive state, and navigation links.
Custom messages, other notification types, and notifications for deleted teams
are unchanged. Rolling back the migration does not restore the incorrect messages.
