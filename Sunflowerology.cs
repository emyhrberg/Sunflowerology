using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Sunflowerology.Common.PacketHandlers;
using Sunflowerology.Common.Shaders;
using System.IO;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sunflowerology
{
    public class Sunflowerology : Mod
    {

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            ModNetHandler.HandlePacket(reader, whoAmI);
        }

        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            var screenRef = ModContent.Request<Effect>("Sunflowerology/Assets/Effects/Shockwave", AssetRequestMode.AsyncLoad);

            Filters.Scene[ShockwaveShaderData.ShockwaveSceneFilterName] = new Filter(new ShockwaveShaderData(screenRef, "ShockwavePass"), EffectPriority.VeryHigh);
        }

        public override void Unload()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            if (Filters.Scene[ShockwaveShaderData.ShockwaveSceneFilterName]?.IsActive() == true)
                Filters.Scene.Deactivate(ShockwaveShaderData.ShockwaveSceneFilterName);
        }
    }
}
