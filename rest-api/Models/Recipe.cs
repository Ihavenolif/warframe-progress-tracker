using System;
using System.Collections.Generic;

namespace rest_api.Models;

public partial class Recipe
{
    public string unique_name { get; set; } = null!;

    public string result_item { get; set; } = null!;

    public virtual ICollection<Recipe_ingredient> recipe_ingredients { get; set; } = new List<Recipe_ingredient>();

    public virtual Item result_itemNavigation { get; set; } = null!;

    public virtual Item unique_nameNavigation { get; set; } = null!;
}
