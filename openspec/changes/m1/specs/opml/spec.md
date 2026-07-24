# opml Specification Delta

## ADDED Requirements

### Requirement: Subscriptions can be exported to OPML 2.0

The system SHALL export the current subscription list to an OPML 2.0 document
with one `<outline>` per subscription carrying its title and XML URL.

#### Scenario: Export produces a valid OPML document

- **WHEN** two subscriptions are exported
- **THEN** the resulting OPML text is well-formed, has `version="2.0"`, and contains
  one `outline` element per subscription with the correct `xmlUrl` and `title`

### Requirement: Subscriptions can be imported from OPML

The system SHALL import subscription feed URLs from an OPML document, returning
the list of feed URLs to subscribe to (parsing both RSS and generic outlines).

#### Scenario: Feed URLs are extracted from outlines

- **WHEN** an OPML document with three `outline` elements (two with `xmlUrl`, one
  without) is imported
- **THEN** exactly the two feed URLs with `xmlUrl` are returned
