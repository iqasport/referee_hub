# Usernames

## Overview

Every user has a unique, editable username (issue [#591](https://github.com/iqasport/referee_hub/issues/591)). Usernames make it possible to:

- Add or remove NGB admins, team managers and tournament managers without knowing the person's email address.
- Invite players to a team by username.
- Share a readable profile link: `/referees/<username>`.

## Format

Usernames are validated identically on the backend (`ManagementHub.Models.Domain.User.UsernameRules`) and the frontend (`app/utils/usernameUtils.ts`):

- 3–30 characters.
- Lowercase letters `a-z`, digits `0-9`, `.`, `_` and `-`.
- Must start and end with a letter or digit.
- Case-insensitive: input is trimmed and lowercased before it is stored or looked up.
- Never contains `@`, so an input can always be told apart from an email address.

Uniqueness is enforced by the `index_users_on_username` unique index.

## Default usernames

- **Migration `AddUsernameToUsers`** backfills every existing user with `firstname.lastname`:
  - Accents are transliterated (`José Núñez` → `jose.nunez`); `ß`, `æ`, `œ` become `ss`, `ae`, `oe`.
  - Remaining unsupported characters are removed, and the base is truncated to 26 characters.
  - Duplicates get a numeric suffix starting at 2 (`jane.doe`, `jane.doe2`, …).
  - Users without a usable name get `user<id>`.
- **New accounts** (sign up, or created through "create user if they don't have an account") get `user<id>`. When the user later sets their first/last name, the default is replaced with a name-based username. A username the user chose themselves is never overwritten.

## Changing a username

Users edit their username in **Basic Details** on their profile page. The field shows the format rules and a live availability check, and Save is disabled while the value is invalid or taken.

API:

| Endpoint | Description |
|----------|-------------|
| `PATCH /api/v2/Users/me/info` | Accepts `username`. Returns 400 for an invalid format and 409 if it is taken. |
| `GET /api/v2/Users/username-availability?username=` | Returns `{ username, isValid, isAvailable }`. The caller's own username counts as available. |
| `GET /api/v2/Users/by-username/{username}` | Resolves a username to `{ userId, username }`. Returns 404 if unknown. |

## Email-or-username inputs

The following inputs accept either an email address or a username:

- NGB admins (add/delete)
- NGB team managers (add/delete)
- Team management: add manager, invite player
- Tournament managers (add)

Inputs containing `@` are handled exactly as before. Anything else that is a valid username is resolved to that user's email on the backend (`UsernameQueryExtensions.ResolveEmailOrUsernameAsync`), and the existing email flow continues. An unknown username returns the same "user not found" response as an unknown email. "Create user if they don't have an account" only works with an email address.

## Profile links

- `/referees/me` and `/referees/U_...` work as before.
- Any other value is treated as a username and resolved via `by-username` before the profile loads; an unknown username shows an error.
- Profiles show `@username` with a **Copy profile link** button.
