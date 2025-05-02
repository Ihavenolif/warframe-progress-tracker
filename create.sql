-- Active: 1746057700810@@127.0.0.1@5432@warframe_tracker
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
DROP TABLE IF EXISTS component_item CASCADE;
DROP TABLE IF EXISTS component_player CASCADE;
DROP TABLE IF EXISTS player_items_mastery CASCADE;
DROP TABLE IF EXISTS recipe CASCADE;
DROP TABLE IF EXISTS recipe_ingredients CASCADE;
-- End of removing

CREATE TABLE item (
    name VARCHAR(256),
    unique_name VARCHAR(256) NOT NULL PRIMARY KEY,
    type VARCHAR(256) NOT NULL,
    item_class VARCHAR(256) NOT NULL,
    xp_required INTEGER
);

CREATE TABLE player (
    id SERIAL PRIMARY KEY,
    username VARCHAR(256) NOT NULL UNIQUE,
    mastery_rank INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE clan (
    id SERIAL PRIMARY KEY,
    name VARCHAR(256) NOT NULL UNIQUE,
    leader_id INTEGER NOT NULL REFERENCES player(id) ON DELETE CASCADE
);

CREATE TABLE clan_invitation (
    id SERIAL NOT NULL,
    clan_id INTEGER REFERENCES clan(id) ON DELETE CASCADE,
    player_id INTEGER REFERENCES player(id) ON DELETE CASCADE,
    PRIMARY KEY (clan_id, player_id)
);

CREATE TABLE player_items (
    unique_name VARCHAR(256) REFERENCES item(unique_name) ON DELETE CASCADE,
    player_id INTEGER REFERENCES player(id) ON DELETE CASCADE,
    item_count INTEGER NOT NULL DEFAULT 1,
    PRIMARY KEY (unique_name, player_id)
);

CREATE TABLE registered_user (
    id SERIAL PRIMARY KEY,
    player_id INTEGER REFERENCES player(id) ON DELETE CASCADE,
    username VARCHAR(256) UNIQUE NOT NULL,
    password_hash VARCHAR(256) NOT NULL
);

CREATE TABLE player_clan (
    clan_id INTEGER REFERENCES clan(id) ON DELETE CASCADE,
    player_id INTEGER REFERENCES player(id) ON DELETE CASCADE,
    PRIMARY KEY (clan_id, player_id)
);

CREATE TABLE player_items_mastery(
    unique_name VARCHAR(256) NOT NULL REFERENCES item(unique_name) ON DELETE CASCADE,
    player_id INTEGER NOT NULL REFERENCES player(id) ON DELETE CASCADE,
    xp_gained INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (unique_name, player_id)
);

CREATE TABLE recipe (
    unique_name VARCHAR(256) PRIMARY KEY REFERENCES item(unique_name) ON DELETE CASCADE,
    result_item VARCHAR(256) NOT NULL REFERENCES item(unique_name) ON DELETE CASCADE
);

CREATE TABLE recipe_ingredients(
    recipe_name VARCHAR(256) NOT NULL REFERENCES recipe(unique_name) ON DELETE CASCADE,
    item_ingredient VARCHAR(256) NOT NULL REFERENCES item(unique_name) ON DELETE CASCADE,
    ingredient_count INTEGER NOT NULL DEFAULT 1,
    PRIMARY KEY (recipe_name, item_ingredient)
);

CREATE OR REPLACE FUNCTION ADD_NAME_TO_BLUEPRINT_TRIGGER()
RETURNS TRIGGER AS $$
BEGIN
    NEW.name := (
        SELECT name
        FROM item
        WHERE item.unique_name = NEW.name
    ) || ' Blueprint';
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER T_ADD_NAME_TO_BLUEPRINT
BEFORE INSERT ON ITEM
FOR EACH ROW
WHEN (NEW.type = 'Recipe')
EXECUTE FUNCTION ADD_NAME_TO_BLUEPRINT_TRIGGER();