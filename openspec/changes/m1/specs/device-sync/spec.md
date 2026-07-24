# device-sync Specification Delta

## ADDED Requirements

### Requirement: Send-selected copies chosen episodes to the device

The system SHALL copy a caller-selected set of downloaded episodes to a target
device path, rendering each destination via the path template, creating
destination directories as needed.

#### Scenario: Selected episodes are copied

- **WHEN** two downloaded episodes are sent to a device path with a template
- **THEN** both files exist at the rendered destinations and their bytes match the source

### Requirement: Subscription sync performs a delta transfer

The system SHALL sync a subscription to a device by computing which downloaded
episodes are missing on the target and transferring only that delta.

#### Scenario: Only missing episodes are transferred

- **WHEN** three episodes exist locally and one already exists on the device
- **THEN** only the two missing episodes are copied

### Requirement: Duplicate detection skips identical files

The system SHALL detect existing destination files by name and size and skip
copying them, to avoid redundant writes and flash-memory wear.

#### Scenario: A same-name same-size file is skipped

- **WHEN** a destination file with the same name and identical byte size already exists
- **THEN** the source is not copied and the operation counts it as skipped

### Requirement: A space check guards the transfer

The system SHALL verify that the destination volume reports enough free space to
hold the planned transfer before starting, and refuse to transfer when
insufficient.

#### Scenario: Transfer is refused when space is insufficient

- **WHEN** the planned transfer size exceeds the destination's reported free space
- **THEN** no files are copied and the operation reports insufficient space

#### Scenario: Transfer proceeds when space is sufficient

- **WHEN** the planned transfer size is within the destination's reported free space
- **THEN** the transfer proceeds normally
