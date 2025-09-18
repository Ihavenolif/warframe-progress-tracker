-- Active: 1746057700810@@127.0.0.1@5432@warframe_tracker

CREATE TABLE IF NOT EXISTS item (
    name VARCHAR(256),
    unique_name VARCHAR(256) NOT NULL PRIMARY KEY,
    type VARCHAR(256) NOT NULL,
    item_class VARCHAR(256) NOT NULL,
    xp_required INTEGER
);

CREATE TABLE IF NOT EXISTS player (
    id SERIAL PRIMARY KEY,
    username VARCHAR(256) NOT NULL UNIQUE,
    mastery_rank INTEGER NOT NULL DEFAULT 0
);
ALTER TABLE player ADD COLUMN IF NOT EXISTS duviri_skills INT NOT NULL DEFAULT 0;
ALTER TABLE player ADD COLUMN IF NOT EXISTS railjack_skills INT NOT NULL DEFAULT 0;
ALTER TABLE player ADD COLUMN IF NOT EXISTS total_mastery_xp INT NOT NULL DEFAULT 0;

CREATE TABLE IF NOT EXISTS clan (
    id SERIAL PRIMARY KEY,
    name VARCHAR(256) NOT NULL UNIQUE,
    leader_id INTEGER NOT NULL REFERENCES player (id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS clan_invitation (
    id SERIAL NOT NULL PRIMARY KEY,
    clan_id INTEGER REFERENCES clan (id) ON DELETE CASCADE,
    player_id INTEGER REFERENCES player (id) ON DELETE CASCADE,
    invitation_status TEXT NOT NULL DEFAULT 'PENDING' CHECK (invitation_status IN ('PENDING', 'ACCEPTED', 'DECLINED', 'CANCELED'))
);

CREATE TABLE IF NOT EXISTS player_items (
    unique_name VARCHAR(256) REFERENCES item (unique_name) ON DELETE CASCADE,
    player_id INTEGER REFERENCES player (id) ON DELETE CASCADE,
    item_count INTEGER NOT NULL DEFAULT 1,
    PRIMARY KEY (unique_name, player_id)
);

CREATE TABLE IF NOT EXISTS registered_user (
    id SERIAL PRIMARY KEY,
    player_id INTEGER REFERENCES player (id) ON DELETE CASCADE,
    username VARCHAR(256) UNIQUE NOT NULL,
    password_hash VARCHAR(256) NOT NULL,
    roles TEXT[] NOT NULL DEFAULT '{}'
);

CREATE TABLE IF NOT EXISTS refresh_token (
    token VARCHAR(256) PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES registered_user (id) ON DELETE CASCADE,
    issued TIMESTAMP NOT NULL DEFAULT NOW(),
    expires TIMESTAMP NOT NULL,
    revoked BOOLEAN NOT NULL DEFAULT FALSE,
    issued_by_ip VARCHAR(45)
);

CREATE TABLE IF NOT EXISTS player_clan (
    clan_id INTEGER REFERENCES clan (id) ON DELETE CASCADE,
    player_id INTEGER REFERENCES player (id) ON DELETE CASCADE,
    PRIMARY KEY (clan_id, player_id)
);

CREATE TABLE IF NOT EXISTS player_items_mastery (
    unique_name VARCHAR(256) NOT NULL REFERENCES item (unique_name) ON DELETE CASCADE,
    player_id INTEGER NOT NULL REFERENCES player (id) ON DELETE CASCADE,
    xp_gained INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (unique_name, player_id)
);

CREATE TABLE IF NOT EXISTS recipe (
    unique_name VARCHAR(256) PRIMARY KEY REFERENCES item (unique_name) ON DELETE CASCADE,
    result_item VARCHAR(256) NOT NULL REFERENCES item (unique_name) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS recipe_ingredients (
    recipe_name VARCHAR(256) NOT NULL REFERENCES recipe (unique_name) ON DELETE CASCADE,
    item_ingredient VARCHAR(256) NOT NULL REFERENCES item (unique_name) ON DELETE CASCADE,
    ingredient_count INTEGER NOT NULL DEFAULT 1,
    PRIMARY KEY (recipe_name, item_ingredient)
);

CREATE TABLE IF NOT EXISTS missions (
    unique_name VARCHAR(256) PRIMARY KEY,
    name VARCHAR(256) NOT NULL,
    planet VARCHAR(256) NOT NULL,
    mastery_xp INT NOT NULL,
    type VARCHAR(256) NOT NULL
);

CREATE TABLE IF NOT EXISTS player_mission_completion (
    unique_name VARCHAR(256) REFERENCES missions(unique_name),
    player_id INT REFERENCES player(id),
    PRIMARY KEY (unique_name, player_id),
    completes INT DEFAULT 0 NOT NULL,
    sp_complete BOOLEAN DEFAULT FALSE NOT NULL
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

drop MATERIALIZED view if exists xp_items_with_recipes_and_components;

create materialized view xp_items_with_recipes_and_components as (
    with xp_items as (
        select * from item where xp_required is not null
    ),
    xp_items_with_recipes as (
        select xp_items.*, recipe.unique_name as recipe_unique_name, recipe_item.name as recipe_name from xp_items 
        left join recipe on xp_items.unique_name = recipe.result_item
        left join item recipe_item on recipe.unique_name = recipe_item.unique_name
    ),
    xp_items_with_recipes_and_components_ as (
        select xp_items_with_recipes.*, 
        item_component.name as component_name, 
        item_component.unique_name as component_unique_name, 
        recipe_ingredients.ingredient_count as ingredient_count,
        component_bps.unique_name as component_bp_unique_name
        from xp_items_with_recipes
        left join recipe_ingredients on xp_items_with_recipes.recipe_unique_name = recipe_ingredients.recipe_name
        left join item item_component on recipe_ingredients.item_ingredient = item_component.unique_name
        left join recipe component_bps on item_component.unique_name = component_bps.result_item
    )
    select * from xp_items_with_recipes_and_components_
);

create index idx_xp_items_with_recipes_and_components_unique_name on xp_items_with_recipes_and_components(unique_name);
create index idx_xp_items_with_recipes_and_components_recipe_unique_name on xp_items_with_recipes_and_components(recipe_unique_name);
create index idx_xp_items_with_recipes_and_components_component_unique_name on xp_items_with_recipes_and_components(component_unique_name);
