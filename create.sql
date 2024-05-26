-- Remove conflicting tables
DROP TABLE IF EXISTS clan CASCADE;
DROP TABLE IF EXISTS companion CASCADE;
DROP TABLE IF EXISTS component CASCADE;
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
-- End of removing

CREATE TABLE clan (
    id SERIAL NOT NULL,
    name VARCHAR(256) NOT NULL
);
ALTER TABLE clan ADD CONSTRAINT pk_clan PRIMARY KEY (id);

CREATE TABLE companion (
    name VARCHAR(256) NOT NULL,
    item_class VARCHAR(256) NOT NULL
);
ALTER TABLE companion ADD CONSTRAINT pk_companion PRIMARY KEY (name);

CREATE TABLE component (
    name VARCHAR(256) NOT NULL
);
ALTER TABLE component ADD CONSTRAINT pk_component PRIMARY KEY (name);

CREATE TABLE item (
    name VARCHAR(256) NOT NULL,
    nameraw VARCHAR(256) NOT NULL,
    type VARCHAR(256) NOT NULL
);
ALTER TABLE item ADD CONSTRAINT pk_item PRIMARY KEY (name);

CREATE TABLE player (
    id SERIAL NOT NULL,
    registered_user_id INTEGER NOT NULL,
    username VARCHAR(256) NOT NULL,
    mastery_rank INTEGER NOT NULL
);
ALTER TABLE player ADD CONSTRAINT pk_player PRIMARY KEY (id);
ALTER TABLE player ADD CONSTRAINT uc_player_username UNIQUE (username);
ALTER TABLE player ADD CONSTRAINT u_fk_player_registered_user UNIQUE (registered_user_id);

CREATE TABLE player_items (
    player_id INTEGER NOT NULL,
    item_name VARCHAR(256) NOT NULL,
    state INTEGER NOT NULL
);
ALTER TABLE player_items ADD CONSTRAINT pk_player_items PRIMARY KEY (player_id, item_name);

CREATE TABLE registered_user (
    id SERIAL NOT NULL,
    username VARCHAR(256) NOT NULL,
    password_hash VARCHAR(256) NOT NULL,
    salt VARCHAR(256) NOT NULL
);
ALTER TABLE registered_user ADD CONSTRAINT pk_registered_user PRIMARY KEY (id);
ALTER TABLE registered_user ADD CONSTRAINT uc_registered_user_username UNIQUE (username);

CREATE TABLE warframe (
    name VARCHAR(256) NOT NULL,
    item_class VARCHAR(256) NOT NULL
);
ALTER TABLE warframe ADD CONSTRAINT pk_warframe PRIMARY KEY (name);

CREATE TABLE weapon (
    name VARCHAR(256) NOT NULL,
    item_class VARCHAR(256) NOT NULL
);
ALTER TABLE weapon ADD CONSTRAINT pk_weapon PRIMARY KEY (name);

CREATE TABLE component_companion (
    component_name VARCHAR(256) NOT NULL,
    companion_name VARCHAR(256) NOT NULL
);
ALTER TABLE component_companion ADD CONSTRAINT pk_component_companion PRIMARY KEY (component_name, companion_name);

CREATE TABLE player_clan (
    player_id INTEGER NOT NULL,
    clan_id INTEGER NOT NULL
);
ALTER TABLE player_clan ADD CONSTRAINT pk_player_clan PRIMARY KEY (player_id, clan_id);

CREATE TABLE player_component (
    id INTEGER NOT NULL,
    name VARCHAR(256) NOT NULL
);
ALTER TABLE player_component ADD CONSTRAINT pk_player_component PRIMARY KEY (id, name);

CREATE TABLE warframe_component (
    warframe_name VARCHAR(256) NOT NULL,
    component_name VARCHAR(256) NOT NULL
);
ALTER TABLE warframe_component ADD CONSTRAINT pk_warframe_component PRIMARY KEY (warframe_name, component_name);

CREATE TABLE weapon_component (
    weapon_name VARCHAR(256) NOT NULL,
    component_name VARCHAR(256) NOT NULL
);
ALTER TABLE weapon_component ADD CONSTRAINT pk_weapon_component PRIMARY KEY (weapon_name, component_name);

ALTER TABLE companion ADD CONSTRAINT fk_companion_item FOREIGN KEY (name) REFERENCES item (name) ON DELETE CASCADE;

ALTER TABLE player ADD CONSTRAINT fk_player_registered_user FOREIGN KEY (registered_user_id) REFERENCES registered_user (id) ON DELETE CASCADE;

ALTER TABLE player_items ADD CONSTRAINT fk_player_items_player FOREIGN KEY (player_id) REFERENCES player (id) ON DELETE CASCADE;
ALTER TABLE player_items ADD CONSTRAINT fk_player_items_item FOREIGN KEY (item_name) REFERENCES item (name) ON DELETE CASCADE;

ALTER TABLE warframe ADD CONSTRAINT fk_warframe_item FOREIGN KEY (name) REFERENCES item (name) ON DELETE CASCADE;

ALTER TABLE weapon ADD CONSTRAINT fk_weapon_item FOREIGN KEY (name) REFERENCES item (name) ON DELETE CASCADE;

ALTER TABLE component_companion ADD CONSTRAINT fk_component_companion_componen FOREIGN KEY (component_name) REFERENCES component (name) ON DELETE CASCADE;
ALTER TABLE component_companion ADD CONSTRAINT fk_component_companion_companio FOREIGN KEY (companion_name) REFERENCES companion (name) ON DELETE CASCADE;

ALTER TABLE player_clan ADD CONSTRAINT fk_player_clan_player FOREIGN KEY (player_id) REFERENCES player (id) ON DELETE CASCADE;
ALTER TABLE player_clan ADD CONSTRAINT fk_player_clan_clan FOREIGN KEY (clan_id) REFERENCES clan (id) ON DELETE CASCADE;

ALTER TABLE player_component ADD CONSTRAINT fk_player_component_player FOREIGN KEY (id) REFERENCES player (id) ON DELETE CASCADE;
ALTER TABLE player_component ADD CONSTRAINT fk_player_component_component FOREIGN KEY (name) REFERENCES component (name) ON DELETE CASCADE;

ALTER TABLE warframe_component ADD CONSTRAINT fk_warframe_component_warframe FOREIGN KEY (warframe_name) REFERENCES warframe (name) ON DELETE CASCADE;
ALTER TABLE warframe_component ADD CONSTRAINT fk_warframe_component_component FOREIGN KEY (component_name) REFERENCES component (name) ON DELETE CASCADE;

ALTER TABLE weapon_component ADD CONSTRAINT fk_weapon_component_weapon FOREIGN KEY (weapon_name) REFERENCES weapon (name) ON DELETE CASCADE;
ALTER TABLE weapon_component ADD CONSTRAINT fk_weapon_component_component FOREIGN KEY (component_name) REFERENCES component (name) ON DELETE CASCADE;

