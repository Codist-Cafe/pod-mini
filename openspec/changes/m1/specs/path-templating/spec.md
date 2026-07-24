# path-templating Specification Delta

## ADDED Requirements

### Requirement: Device paths render from tokens

The system SHALL render a destination path pattern using the tokens
`{ShowTitle}`, `{PublishDate}` (with optional .NET format suffix), and `{Title}`,
substituting sanitized segment values. Unknown tokens are left untouched.

#### Scenario: Tokens are substituted with sanitization

- **WHEN** the pattern `{ShowTitle}/{PublishDate:yyyy-MM-dd}_{Title}.mp3` is
  rendered for a show "Soft? Daily" published 2026-07-24 titled "Net 10"
- **THEN** the result is `Soft Daily/2026-07-24_Net 10.mp3`

#### Scenario: Unknown tokens are preserved

- **WHEN** a pattern contains `{Foo}` and `{ShowTitle}`
- **THEN** `{Foo}` is left as-is in the output and `{ShowTitle}` is substituted

### Requirement: Date tokens honor a custom format suffix

The system SHALL apply the .NET date format given after the token name and colon
(for example `{PublishDate:yyyy-MM-dd}`), defaulting to ISO date when no format
is supplied.

#### Scenario: A custom date format is applied

- **WHEN** `{PublishDate:yyyy_MM_dd}` is rendered for 2026-07-24
- **THEN** the segment becomes `2026_07_24`
