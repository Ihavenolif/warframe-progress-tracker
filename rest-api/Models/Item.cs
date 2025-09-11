using System;
using System.Collections.Generic;

namespace rest_api.Models;

public partial class Item
{
    public string? name { get; set; }

    public string unique_name { get; set; } = null!;

    public string type { get; set; } = null!;

    public string item_class { get; set; } = null!;

    public int? xp_required { get; set; }

    public virtual ICollection<Player_item> player_items { get; set; } = new List<Player_item>();

    public virtual ICollection<Player_items_mastery> player_items_masteries { get; set; } = new List<Player_items_mastery>();

    public virtual ICollection<Recipe_ingredient> recipe_ingredients { get; set; } = new List<Recipe_ingredient>();

    public virtual ICollection<Recipe> reciperesult_itemNavigations { get; set; } = new List<Recipe>();

    public virtual Recipe? recipeunique_nameNavigation { get; set; }
}
