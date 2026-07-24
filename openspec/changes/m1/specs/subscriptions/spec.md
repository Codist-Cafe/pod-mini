# subscriptions Specification Delta

## ADDED Requirements

### Requirement: Subscription records are persisted with a unique feed URL

The system SHALL persist each podcast subscription as a record with a title, a
globally unique feed URL, a site URL, description, image URL, a deterministic
local folder name, and creation/last-fetched timestamps.

#### Scenario: Adding a subscription stores required fields

- **WHEN** a subscription is added with a title and feed URL
- **THEN** the stored record has a non-empty title, the exact feed URL, a derived
  local folder name, a non-default `CreatedAt`, and a null `LastFetchedAt`

#### Scenario: Feed URLs must be unique

- **WHEN** a second subscription is added with a feed URL already stored
- **THEN** the system rejects the duplicate and the original record is unchanged

### Requirement: Episodes belong to exactly one subscription and are deduplicated by GUID

The system SHALL store episodes against a subscription and deduplicate incoming
episodes by their `(SubscriptionId, Guid)` pair. Each episode tracks publish
date, duration, audio URL, file size, played state, local file path, and a
download state.

#### Scenario: Re-ingesting a feed updates rather than duplicates episodes

- **WHEN** an episode with an existing GUID for the subscription is ingested again
- **THEN** no new episode row is created and the existing row's metadata is updated

### Requirement: Removing a subscription offers two delete modes

The system SHALL support removing a subscription either by deleting only the
database records (keeping on-disk files) or by deleting both the records and the
subscription's local folder on disk.

#### Scenario: Unsubscribe only keeps files

- **WHEN** a subscription is removed with the "records only" mode
- **THEN** the subscription and its episodes are removed from the database but the
  local folder remains on disk

#### Scenario: Unsubscribe and delete files purges both

- **WHEN** a subscription is removed with the "records and files" mode
- **THEN** the database records are removed and the local folder is deleted from disk

### Requirement: Cascade delete removes a subscription's episodes

The system SHALL cascade-delete all episodes when a subscription is removed, so
no orphaned episode rows remain.

#### Scenario: Episodes are removed with their parent subscription

- **WHEN** a subscription with episodes is deleted
- **THEN** no episode rows referencing that subscription remain in the database
