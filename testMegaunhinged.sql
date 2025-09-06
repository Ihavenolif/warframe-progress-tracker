-- Active: 1746967109684@@127.0.0.1@5432@warframe_tracker@public

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

-- Full query for one player
select xp_items_with_recipes_and_components.name,
    xp_items_with_recipes_and_components.unique_name, 
    xp_items_with_recipes_and_components.item_class,
    xp_items_with_recipes_and_components.type,
    xp_items_with_recipes_and_components.xp_required,
    xp_items_with_recipes_and_components.recipe_unique_name,
    xp_items_with_recipes_and_components.recipe_name, 
    player_items_mastery.xp_gained as xp_gained,
    bp_ownership.item_count is not null and bp_ownership.item_count > 0 as blueprint_owned,
    json_agg(
        json_build_object(
            'name', xp_items_with_recipes_and_components.component_name,
            'uniqueName', xp_items_with_recipes_and_components.component_unique_name,
            'countOwned', COALESCE(component_ownership.item_count, 0),
            'countRequired', xp_items_with_recipes_and_components.ingredient_count,
            'isCraftable', xp_items_with_recipes_and_components.component_bp_unique_name is not null,
            'blueprintOwned', component_bp_ownership.item_count is not null and component_bp_ownership.item_count > 0
        )
    ) filter (where player_items_mastery.xp_gained is null and xp_items_with_recipes_and_components.component_unique_name is not null) as components_json 
from xp_items_with_recipes_and_components
full join (
    select * from player_items_mastery where player_id = 101 --parameter player_id
) player_items_mastery on xp_items_with_recipes_and_components.unique_name = player_items_mastery.unique_name
left join (
    select * from player_items where player_id = 101 --parameter player_id
) bp_ownership on xp_items_with_recipes_and_components.recipe_unique_name = bp_ownership.unique_name and player_items_mastery.xp_gained is null 
left join (
    select * from player_items where player_id = 101 --parameter player_id
) component_ownership on xp_items_with_recipes_and_components.component_unique_name = component_ownership.unique_name and player_items_mastery.xp_gained is null
left join (
    select * from player_items where player_id = 101 --parameter player_id
) component_bp_ownership on xp_items_with_recipes_and_components.component_bp_unique_name = component_bp_ownership.unique_name and player_items_mastery.xp_gained is null
group by xp_items_with_recipes_and_components.name,
    xp_items_with_recipes_and_components.unique_name, 
    xp_items_with_recipes_and_components.item_class,
    xp_items_with_recipes_and_components.type,
    xp_items_with_recipes_and_components.xp_required,
    xp_items_with_recipes_and_components.recipe_unique_name,
    xp_items_with_recipes_and_components.recipe_name,
    player_items_mastery.xp_gained,
    bp_ownership.item_count;

-- Query for just player data
select 
    xp_items_with_recipes_and_components.unique_name, 
    player_items_mastery.xp_gained as xp_gained,
    bp_ownership.item_count is not null and bp_ownership.item_count > 0 as blueprint_owned,
    json_agg(
        json_build_object(
            'name', xp_items_with_recipes_and_components.component_name,
            'uniqueName', xp_items_with_recipes_and_components.component_unique_name,
            'countOwned', COALESCE(component_ownership.item_count, 0),
            'countRequired', xp_items_with_recipes_and_components.ingredient_count,
            'isCraftable', xp_items_with_recipes_and_components.component_bp_unique_name is not null,
            'blueprintOwned', component_bp_ownership.item_count is not null and component_bp_ownership.item_count > 0
        )
    ) filter (where player_items_mastery.xp_gained is null and xp_items_with_recipes_and_components.component_unique_name is not null) as components_json 
from xp_items_with_recipes_and_components
full join (
    select * from player_items_mastery where player_id = 101 --parameter player_id
) player_items_mastery on xp_items_with_recipes_and_components.unique_name = player_items_mastery.unique_name
left join (
    select * from player_items where player_id = 101 --parameter player_id
) bp_ownership on xp_items_with_recipes_and_components.recipe_unique_name = bp_ownership.unique_name and player_items_mastery.xp_gained is null 
left join (
    select * from player_items where player_id = 101 --parameter player_id
) component_ownership on xp_items_with_recipes_and_components.component_unique_name = component_ownership.unique_name and player_items_mastery.xp_gained is null
left join (
    select * from player_items where player_id = 101 --parameter player_id
) component_bp_ownership on xp_items_with_recipes_and_components.component_bp_unique_name = component_bp_ownership.unique_name and player_items_mastery.xp_gained is null
group by xp_items_with_recipes_and_components.unique_name,
    player_items_mastery.xp_gained,
    bp_ownership.item_count;