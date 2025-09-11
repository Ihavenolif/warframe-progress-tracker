using System;
using System.Collections.Generic;

namespace rest_api.Models;

public partial class Recipe_ingredient
{
    public string recipe_name { get; set; } = null!;

    public string item_ingredient { get; set; } = null!;

    public int ingredient_count { get; set; }

    public virtual Item item_ingredientNavigation { get; set; } = null!;

    public virtual Recipe recipe_nameNavigation { get; set; } = null!;
}
