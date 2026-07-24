# library-storage Specification Delta

## ADDED Requirements

### Requirement: Folder and file names are sanitized for cross-platform filesystems

The system SHALL sanitize show titles and episode filenames by removing invalid
path characters (`\`, `/`, `:`, `*`, `?`, `"`, `<`, `>`, `|`), collapsing
leading/trailing dots and spaces, and optionally replacing spaces with
underscores.

#### Scenario: Invalid characters are stripped

- **WHEN** a title `What? "No" <way> | done*` is sanitized
- **THEN** the result contains none of `\ / : * ? " < > |`

#### Scenario: Optional space replacement

- **WHEN** sanitization is configured to replace spaces with underscores
- **THEN** every space in the name becomes an underscore

#### Scenario: Reserved Windows names and trailing dots are neutralized

- **WHEN** a name resolves to a reserved name or ends with a dot/space
- **THEN** the sanitized result is safe on FAT32/exFAT/NTFS/ext4 (no trailing dot,
  no bare reserved name)

### Requirement: Filenames exceeding 240 characters are truncated

The system SHALL truncate any filename longer than 240 characters so that it
remains valid on FAT32/exFAT/NTFS filesystems, preserving the file extension.

#### Scenario: Long filename is truncated with extension preserved

- **WHEN** a filename with extension `.mp3` exceeds 240 characters
- **THEN** the sanitized filename is at most 240 characters long and still ends in `.mp3`

### Requirement: The library root resolves local file placement deterministically

The system SHALL resolve each episode's local file path as
`{LibraryRoot}/{SanitizedShowTitle}/{SanitizedFileName}.mp3` and compute the
subscription's local folder name from its title.

#### Scenario: A local path is composed from root, show, and episode

- **WHEN** the library root, show title, and episode filename are provided
- **THEN** the resolved path joins them with the platform separator and each
  segment is sanitized
