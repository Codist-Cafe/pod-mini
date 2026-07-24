# download-engine Specification Delta

## ADDED Requirements

### Requirement: Every episode has a finite download state

The system SHALL model download progress with a finite state machine:
`Pending (0)` → `Downloading (1)` → `Downloaded (2)`, with `Failed (3)` as a
terminal recovery state. Transitions are guarded; illegal transitions are
rejected.

#### Scenario: Legal transitions advance the state

- **WHEN** an episode in `Pending` transitions to `Downloading` and then to
  `Downloaded`
- **THEN** the final state is `Downloaded`

#### Scenario: Illegal transitions are rejected

- **WHEN** an episode in `Downloaded` attempts to transition to `Downloading`
- **THEN** the transition is rejected and the state stays `Downloaded`

#### Scenario: A downloading episode can fail

- **WHEN** an episode in `Downloading` fails
- **THEN** its state becomes `Failed` and it can be reset to `Pending` for retry

### Requirement: A bounded channel worker queue downloads with limited concurrency

The system SHALL enqueue downloads into a bounded `System.Threading.Channels`
channel drained by a configurable maximum number of concurrent workers (default
3). Work is processed in FIFO order up to the concurrency cap.

#### Scenario: Concurrency is capped

- **WHEN** more items are enqueued than the configured concurrency
- **THEN** at most `maxConcurrency` items are processed simultaneously

#### Scenario: Pause stops accepting new work

- **WHEN** the queue is paused
- **THEN** it rejects further enqueue attempts until resumed

### Requirement: Downloads support resumable HTTP range requests

The system SHALL attempt resumable transfers using an HTTP `Range` request when
the destination file already exists partially, copying the server response body
to disk and recording the resulting byte count.

#### Scenario: A new download writes the full body

- **WHEN** an episode is downloaded and no partial file exists
- **THEN** the full response body is written and the recorded file size equals the
  body length

#### Scenario: A partial file resumes from its current length

- **WHEN** a partial destination file of N bytes already exists and the server
  supports ranges
- **THEN** the engine issues a `Range: bytes=N-` request and appends the returned
  body after the existing bytes
