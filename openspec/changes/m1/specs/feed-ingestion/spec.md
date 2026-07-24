# feed-ingestion Specification Delta

## ADDED Requirements

### Requirement: The engine parses RSS and Atom feeds into normalized records

The system SHALL parse a podcast RSS or Atom feed (via
`System.ServiceModel.Syndication`) and produce a normalized subscription
metadata record plus an ordered list of episode records.

#### Scenario: A standard RSS 2.0 podcast feed is parsed

- **WHEN** the engine ingests a well-formed RSS 2.0 feed containing channel and
  item elements
- **THEN** it returns one subscription record and one episode record per item,
  with title, publish date, GUID, and audio enclosure URL populated

#### Scenario: An Atom feed is parsed

- **WHEN** the engine ingests a well-formed Atom feed
- **THEN** it returns a normalized subscription and episode records just like RSS

### Requirement: Episodes without an audio enclosure are skipped

The system SHALL ignore feed items that have no playable audio enclosure so the
library never contains unplayable entries.

#### Scenario: Text-only items are dropped

- **WHEN** a feed item has no audio enclosure URL
- **THEN** it is excluded from the returned episode list

### Requirement: Missing GUIDs fall back to the audio URL

The system SHALL use an item's explicit GUID when present, and otherwise fall
back to the audio enclosure URL as the deduplication key.

#### Scenario: GUID falls back to audio URL when absent

- **WHEN** a feed item has no GUID element
- **THEN** the episode's GUID equals its audio enclosure URL

### Requirement: The engine extracts duration when available

The system SHALL parse the iTunes `duration` element into seconds when present,
defaulting to zero when absent or unparsable.

#### Scenario: Duration parses simple seconds

- **WHEN** an item has `<itunes:duration>3600</itunes:duration>`
- **THEN** the episode's `DurationSeconds` is 3600

#### Scenario: Duration parses HH:MM:SS form

- **WHEN** an item has `<itunes:duration>1:02:03</itunes:duration>`
- **THEN** the episode's `DurationSeconds` is 3723

#### Scenario: Missing duration defaults to zero

- **WHEN** an item has no duration element
- **THEN** the episode's `DurationSeconds` is 0
