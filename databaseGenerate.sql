CREATE TABLE IF NOT EXISTS Votes(
    UID bigint primary key not null,
    VotedTeamId bigint,
    VotedCaptainIds bigint[]
)