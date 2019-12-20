﻿using Terraria.ModLoader;

namespace TerrarianBizzareAdventure.Extensions
{
    public static class RecipeExtensions
    {
        public static void AddIngredient<T>(this ModRecipe recipe, int stack = 1) where T : ModItem
        {
            recipe.AddIngredient(ModContent.ItemType<T>(), stack);
        }
    }
}