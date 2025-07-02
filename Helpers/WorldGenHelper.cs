using System;
using System.Collections.Generic;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Sunflowerology.Helpers
{
    public static class WorldGenHelper
    {
        /// <summary>
        /// Inserts a world gen task after a specified pass name.
        /// </summary>
        /// <param name="tasks">The list of GenPasses.</param>
        /// <param name="placeAfterPass">The name of the pass to insert after.</param>
        /// <param name="mod">Your mod instance.</param>
        /// <param name="taskName">The name of the new pass.</param>
        /// <param name="message">Message to display during generation.</param>
        /// <param name="action">The actual generation logic to run.</param>
        /// <param name="weight">Weight of the gen task (affects progress bar).</param>
        public static void InsertWorldGenPass(
            Mod mod,
            List<GenPass> tasks,
            string placeAfterPass,
            string taskName,
            string message,
            Action<GenerationProgress> action,
            float weight = 100f)
        {
            int index = tasks.FindIndex(gp => gp.Name.Equals(placeAfterPass, StringComparison.OrdinalIgnoreCase));
            if (index == -1)
            {
                Log.Warn($"WorldGenHelper: Could not find pass '{placeAfterPass}' to insert after.");
                return;
            }

            var customPass = new SimpleGenPass($"{mod.Name}: {taskName}", weight, message, action);
            tasks.Insert(index + 1, customPass);
        }

        private sealed class SimpleGenPass : GenPass
        {
            private readonly Action<GenerationProgress> _apply;
            private readonly string _message;

            public SimpleGenPass(string name, float loadWeight, string message, Action<GenerationProgress> apply)
                : base(name, loadWeight)
            {
                _apply = apply;
                _message = message;
            }

            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = _message;
                _apply?.Invoke(progress);
            }
        }
    }
}
