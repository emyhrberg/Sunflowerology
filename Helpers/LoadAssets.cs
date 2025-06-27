using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace Sunflowerology.Helpers
{
    /// <summary>
    /// Static class to hold all assets used in the mod.
    /// </summary>
    public static class Ass
    {
        // Add assets here
        // public static Asset<Texture2D> EditorIcon;

        // This bool automatically initializes all specified assets
        public static bool Initialized { get; set; }

        static Ass()
        {
            foreach (FieldInfo field in typeof(Ass).GetFields())
            {
                if (field.FieldType == typeof(Asset<Texture2D>))
                {
                    string modName = "Sunflowerology";
                    string path = field.Name;
                    var asset = ModContent.Request<Texture2D>($"{modName}/Assets/{path}", AssetRequestMode.AsyncLoad);
                    field.SetValue(null, asset);
                }
            }
        }
    }

    /// <summary>
    /// System that automatically initializes assets
    /// </summary>
    public class LoadAssets : ModSystem
    {
        public override void Load()
        {
            _ = Ass.Initialized;
        }
    }
}