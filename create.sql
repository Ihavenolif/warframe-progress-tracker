-- Remove conflicting tables
DROP TABLE IF EXISTS clan CASCADE;
DROP TABLE IF EXISTS companion CASCADE;
DROP TABLE IF EXISTS component CASCADE;
DROP TABLE IF EXISTS invitation CASCADE;
DROP TABLE IF EXISTS item CASCADE;
DROP TABLE IF EXISTS player CASCADE;
DROP TABLE IF EXISTS player_items CASCADE;
DROP TABLE IF EXISTS registered_user CASCADE;
DROP TABLE IF EXISTS warframe CASCADE;
DROP TABLE IF EXISTS weapon CASCADE;
DROP TABLE IF EXISTS component_companion CASCADE;
DROP TABLE IF EXISTS player_clan CASCADE;
DROP TABLE IF EXISTS player_component CASCADE;
DROP TABLE IF EXISTS warframe_component CASCADE;
DROP TABLE IF EXISTS weapon_component CASCADE;
DROP TABLE IF EXISTS clan_invitation CASCADE;
-- End of removing

CREATE TABLE clan (
    id SERIAL NOT NULL,
    name VARCHAR(256) NOT NULL,
    leader_id INTEGER NOT NULL
);
ALTER TABLE clan ADD CONSTRAINT pk_clan PRIMARY KEY (id);
ALTER TABLE clan ADD CONSTRAINT uc_clan_name UNIQUE (name);

CREATE TABLE clan_invitation (
    id SERIAL NOT NULL,
    clan_id INTEGER NOT NULL,
    player_id INTEGER NOT NULL
);
ALTER TABLE clan_invitation ADD CONSTRAINT pk_clan_invitation PRIMARY KEY (id);

CREATE TABLE component (
    name VARCHAR(256) NOT NULL
);
ALTER TABLE component ADD CONSTRAINT pk_component PRIMARY KEY (name);

CREATE TABLE item (
    name VARCHAR(256) NOT NULL,
    nameraw VARCHAR(256) NOT NULL,
    type VARCHAR(256) NOT NULL,
    item_class VARCHAR(256) NOT NULL
);
ALTER TABLE item ADD CONSTRAINT pk_item PRIMARY KEY (name);

CREATE TABLE player (
    id SERIAL NOT NULL,
    username VARCHAR(256) NOT NULL,
    mastery_rank INTEGER NOT NULL,
    registered_user_id INTEGER NOT NULL
);
ALTER TABLE player ADD CONSTRAINT pk_player PRIMARY KEY (id);
ALTER TABLE player ADD CONSTRAINT uc_player_username UNIQUE (username);

CREATE TABLE player_items (
    item_name VARCHAR(256) NOT NULL,
    player_id INTEGER NOT NULL,
    state INTEGER NOT NULL
);
ALTER TABLE player_items ADD CONSTRAINT pk_player_items PRIMARY KEY (item_name, player_id);

CREATE TABLE registered_user (
    id SERIAL NOT NULL,
    player_id INTEGER,
    username VARCHAR(256) NOT NULL,
    password_hash VARCHAR(256) NOT NULL,
    salt VARCHAR(256) NOT NULL
);
ALTER TABLE registered_user ADD CONSTRAINT pk_registered_user PRIMARY KEY (id);
ALTER TABLE registered_user ADD CONSTRAINT uc_registered_user_username UNIQUE (username);
ALTER TABLE registered_user ADD CONSTRAINT u_fk_registered_user_player UNIQUE (player_id);

CREATE TABLE player_clan (
    clan_id INTEGER NOT NULL,
    player_id INTEGER NOT NULL
);
ALTER TABLE player_clan ADD CONSTRAINT pk_player_clan PRIMARY KEY (clan_id, player_id);

CREATE TABLE component_player (
    name VARCHAR(256) NOT NULL,
    id INTEGER NOT NULL
);
ALTER TABLE component_player ADD CONSTRAINT pk_component_player PRIMARY KEY (name, id);

CREATE TABLE component_item (
    component_name VARCHAR(256) NOT NULL,
    item_name VARCHAR(256) NOT NULL
);
ALTER TABLE component_item ADD CONSTRAINT pk_component_item PRIMARY KEY (component_name, item_name);

ALTER TABLE player_items ADD CONSTRAINT fk_player_items_item FOREIGN KEY (item_name) REFERENCES item (name) ON DELETE CASCADE;
ALTER TABLE player_items ADD CONSTRAINT fk_player_items_player FOREIGN KEY (player_id) REFERENCES player (id) ON DELETE CASCADE;

ALTER TABLE registered_user ADD CONSTRAINT fk_registered_user_player FOREIGN KEY (player_id) REFERENCES player (id) ON DELETE CASCADE;

ALTER TABLE player_clan ADD CONSTRAINT fk_player_clan_clan FOREIGN KEY (clan_id) REFERENCES clan (id) ON DELETE CASCADE;
ALTER TABLE player_clan ADD CONSTRAINT fk_player_clan_player FOREIGN KEY (player_id) REFERENCES player (id) ON DELETE CASCADE;

ALTER TABLE component_player ADD CONSTRAINT fk_component_player_component FOREIGN KEY (name) REFERENCES component (name) ON DELETE CASCADE;
ALTER TABLE component_player ADD CONSTRAINT fk_component_player_player FOREIGN KEY (id) REFERENCES player (id) ON DELETE CASCADE;

ALTER TABLE component_item ADD CONSTRAINT fk_component_item_component FOREIGN KEY (component_name) REFERENCES component (name) ON DELETE CASCADE;
ALTER TABLE component_item ADD CONSTRAINT fk_component_item_item FOREIGN KEY (item_name) REFERENCES item (name) ON DELETE CASCADE;

ALTER TABLE clan ADD CONSTRAINT fk_clan_player FOREIGN KEY (leader_id) REFERENCES player (id);

ALTER TABLE clan_invitation ADD CONSTRAINT fk_clan_invitation_clan FOREIGN KEY (clan_id) REFERENCES clan (id);
ALTER TABLE clan_invitation ADD CONSTRAINT fk_clan_invitation_player FOREIGN KEY (player_id) REFERENCES player (id);