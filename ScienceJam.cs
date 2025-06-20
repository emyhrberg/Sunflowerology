using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ScienceJam
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class ScienceJam : Mod
    {
        /// <summary>
        /// A list of tuples that defines the relationship between grass tiles and the tiles they can be placed on.
        /// </summary>
        public static List<Tuple<int, int, int>> GrassTileRelationship = new();

        /// <summary>
        /// Checks if a tile is solid, meaning it has a tile and is not actuated, and is in the Main.tileSolid array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsTileSolid(int x, int y)
        {
            Tile tile = Framing.GetTileSafely(x, y);
            return tile.HasTile && tile.HasUnactuatedTile && Main.tileSolid[tile.TileType];
        }
    }
}
