using Harmony;
using Klei;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    public class RecipeLoader
    {
        private static string path = Application.streamingAssetsPath + "/recipes/";

        public class ElementEntry
        {
            public string material { get; set; }
            public float amount { get; set; }
        }

        public class RecipeEntry
        {
            public string id { get; set; }

            public string description { get; set; }

            public string fabricator { get; set; }
            public string[] fabricators { get; set; }

            public ElementEntry[] ingredients { get; set; }
            public ElementEntry[] results { get; set; }

            public float time { get; set; }

            public ComplexRecipe.RecipeNameDisplay nameDisplay { get; set; }

            public int sortOrder { get; set; }

            public string requiredTech { get; set; }
        }

        private class RecipeEntryCollection : Yaml.Collection<RecipeEntry>
        {
            public RecipeEntry[] recipes { get; set; }

            public RecipeEntry[] Entries => recipes;
        }

        private static ComplexRecipe.RecipeElement[] MakeElements(ElementEntry[] entries)
        {
            return entries.Select(entry => new ComplexRecipe.RecipeElement(entry.material, entry.amount)).ToArray();
        }

        public static void AddRecipe(RecipeEntry entry)
        {
            var ingredients = MakeElements(entry.ingredients);
            var results = MakeElements(entry.results);

            var fabricators = new List<Tag>();
            if (entry.fabricator != null) fabricators.Add(entry.fabricator);
            if (entry.fabricators != null) fabricators.AddRange(entry.fabricators.Select(x => x.ToTag()));

            var firstFabricator = fabricators.Count > 0 ? fabricators[0].ToString() : "missing_fabricator";

            var id = entry.id ?? ComplexRecipeManager.MakeRecipeID(firstFabricator, ingredients, results);

            Log.Spam($"Adding recipe {id}");

            if (ingredients.Length == 0)
            {
                Log.Error($"Recipe {id} has no ingredients");
                return;
            }
            if (results.Length == 0)
            {
                Log.Error($"Recipe {id} has no results");
                return;
            }
            if (fabricators.Count == 0)
            {
                Log.Error($"Recipe {id} has no fabricator");
                return;
            }

            var recipe = new ComplexRecipe(id, ingredients, results);
            recipe.time = entry.time;
            recipe.nameDisplay = entry.nameDisplay;
            recipe.description = entry.description;
            recipe.fabricators = fabricators;
            recipe.sortOrder = entry.sortOrder;
            recipe.requiredTech = entry.requiredTech;
        }

        [PatchOnce]
        [HarmonyPatch(typeof(SupermaterialRefineryConfig), nameof(SupermaterialRefineryConfig.ConfigureBuildingTemplate))]
        public class SupermaterialRefineryConfig_ConfigureBuildingTemplate_Patch
        {
            public static bool Prepare() => InitOnce.Patch;

            public static void Postfix()
            {
                List<RecipeEntry> recipes = Yaml.CollectFromYAML<RecipeEntry, RecipeEntryCollection>(path);
                Log.Info($"Adding {recipes.Count} recipes");

                foreach ( RecipeEntry entry in recipes )
                {
                    AddRecipe(entry);
                }
            }
        }
    }
}
