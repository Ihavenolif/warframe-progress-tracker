CREATE TABLE missions (
    unique_name VARCHAR(256) PRIMARY KEY,
    name VARCHAR(256) NOT NULL,
    planet VARCHAR(256) NOT NULL,
    mastery_xp INT NOT NULL,
    type VARCHAR(256) NOT NULL
);

CREATE TABLE player_mission_completion (
    unique_name VARCHAR(256) REFERENCES missions(unique_name),
    player_id INT REFERENCES player(id),
    PRIMARY KEY (unique_name, player_id),
    completes INT DEFAULT 0 NOT NULL,
    sp_complete BOOLEAN DEFAULT FALSE NOT NULL
);