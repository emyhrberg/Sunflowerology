using ScienceJam.Content.Items.Placeable;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ScienceJam
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class ScienceJam : Mod
    {
        public static List<Tuple<int, int, int>> GrassTileRelationship = new();
        public override void Load()
        {

            Terraria.GameContent.On_SmartCursorHelper.Step_GrassSeeds += SunGrassSeed.SmartCursorCustomSeedModification;
        }

        public override void Unload()
        {

            Terraria.GameContent.On_SmartCursorHelper.Step_GrassSeeds -= SunGrassSeed.SmartCursorCustomSeedModification;
        }

        public static bool IsTileSolid(int x, int y)
        {
            Tile tile = Framing.GetTileSafely(x, y);
            return tile.HasTile && tile.HasUnactuatedTile && Main.tileSolid[tile.TileType];
        }
    }
}
