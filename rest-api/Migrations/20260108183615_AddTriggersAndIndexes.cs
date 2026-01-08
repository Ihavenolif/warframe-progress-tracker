using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggersAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE TRIGGER T_ADD_NAME_TO_BLUEPRINT
                BEFORE INSERT ON ITEM
                FOR EACH ROW
                WHEN (NEW.type = 'Recipe')
                EXECUTE FUNCTION ADD_NAME_TO_BLUEPRINT_TRIGGER();
            ");

            migrationBuilder.Sql(@"
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
            ");

            migrationBuilder.Sql(@"create index idx_xp_items_with_recipes_and_components_unique_name on xp_items_with_recipes_and_components(unique_name);");

            migrationBuilder.Sql(@"create index idx_xp_items_with_recipes_and_components_recipe_unique_name on xp_items_with_recipes_and_components(recipe_unique_name);");

            migrationBuilder.Sql(@"create index idx_xp_items_with_recipes_and_components_component_unique_name on xp_items_with_recipes_and_components(component_unique_name);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS ADD_NAME_TO_BLUEPRINT_TRIGGER();");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS T_ADD_NAME_TO_BLUEPRINT ON ITEM;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS xp_items_with_recipes_and_components;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_xp_items_with_recipes_and_components_unique_name;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_xp_items_with_recipes_and_components_recipe_unique_name;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_xp_items_with_recipes_and_components_component_unique_name;");
        }
    }
}
