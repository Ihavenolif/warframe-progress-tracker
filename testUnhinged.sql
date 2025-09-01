-- Active: 1746967109684@@127.0.0.1@5432@warframe_tracker@public
with missing_items as (
    select * from item where xp_required is not null and not exists( 
         select 1 from player_items_mastery 
         where player_items_mastery.player_id = 101 --parameter player_id
         and item.unique_name = player_items_mastery.unique_name 
    )
), recipe_items as (
    select missing_items.*, recipe_item.name as recipe_name, recipe_item.unique_name as recipe_item_unique_name from missing_items
    left join recipe on recipe.result_item = missing_items.unique_name
    left join item recipe_item on recipe.unique_name = recipe_item.unique_name
), player_items_filtered as (
    select * from player_items
    where player_items.player_id = 101 --parameter player_id
), player_recipe_items as (
    select recipe_items.*, coalesce(player_items_filtered.item_count, 0) as recipe_owned_count from recipe_items
    left join player_items_filtered 
    on recipe_items.recipe_item_unique_name = player_items_filtered.unique_name
), player_recipes_with_ingredients as (
    select player_recipe_items.unique_name, json_agg(
        json_build_object(
            'name', item_component.name,
            'uniqueName', item_component.unique_name,
            'countOwned', coalesce(owned_ingredient.item_count, 0),
            'countRequired', recipe_ingredients.ingredient_count,
            'isCraftable', recipe_for_ingredient.unique_name is not null,
            'recipeOwned', owned_recipe_for_ingredient.item_count is not null and owned_recipe_for_ingredient.item_count > 0
        )
    ) filter (where item_component.unique_name is not null) as components_json from player_recipe_items
    left join recipe_ingredients on player_recipe_items.recipe_item_unique_name = recipe_ingredients.recipe_name
    left join item item_component on recipe_ingredients.item_ingredient = item_component.unique_name
    left join player_items_filtered owned_ingredient on item_component.unique_name = owned_ingredient.unique_name
    left join recipe recipe_for_ingredient on item_component.unique_name = recipe_for_ingredient.result_item
    left join player_items_filtered owned_recipe_for_ingredient on owned_recipe_for_ingredient.unique_name = recipe_for_ingredient.unique_name
    group by player_recipe_items.unique_name

)
select * from player_recipes_with_ingredients;