using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sunflowerology.Common.Systems
{
    internal class SmartCursorSystem : ModSystem
    {
        /// <summary>
        /// Static list to hold the relationship between grass seeds and the tiles they can be placed on.
        /// </summary>
        public static List<Tuple<int, int, int>> GrassTileRelationship = new();

        public override void Load()
        {
            On_SmartCursorHelper.Step_GrassSeeds += SmartCursorCustomSeedModification;
        }

        public override void Unload()
        {
            On_SmartCursorHelper.Step_GrassSeeds -= SmartCursorCustomSeedModification;
        }

        public static void SmartCursorCustomSeedModification(Terraria.GameContent.On_SmartCursorHelper.orig_Step_GrassSeeds orig, object providedInfo1, ref int focusedX, ref int focusedY)
        {
            var providedInfo = (SmartCursorHelper.SmartCursorUsageInfo)providedInfo1;
            if (focusedX > -1 || focusedY > -1)
                return;

            int type = providedInfo.item.type;
            if (type < 0 || !ItemID.Sets.GrassSeeds[type])
                return;

            SmartCursorHelper._targets.Clear();
            for (int i = providedInfo.reachableStartX; i <= providedInfo.reachableEndX; i++)
            {
                for (int j = providedInfo.reachableStartY; j <= providedInfo.reachableEndY; j++)
                {
                    Tile tile = Main.tile[i, j];
                    bool flag = !Main.tile[i - 1, j].active() || !Main.tile[i, j + 1].active() || !Main.tile[i + 1, j].active() || !Main.tile[i, j - 1].active();
                    bool flag2 = !Main.tile[i - 1, j - 1].active() || !Main.tile[i - 1, j + 1].active() || !Main.tile[i + 1, j + 1].active() || !Main.tile[i + 1, j - 1].active();
                    if (tile.active() && !tile.inActive() && (flag || flag2))
                    {
                        bool flag3 = false;
                        switch (type)
                        {
                            default:
                                flag3 = tile.type == 0;
                                break;
                            case 59:
                            case 2171:
                                flag3 = tile.type == 0 || tile.type == 59;
                                break;
                            case 194:
                            case 195:
                                flag3 = tile.type == 59;
                                break;
                            case 5214:
                                flag3 = tile.type == 57;
                                break;
                        }

                        foreach (var relationship in GrassTileRelationship)
                        {
                            if (relationship.Item1 == type && relationship.Item2 == tile.type)
                            {
                                flag3 = true;
                                break;
                            }
                        }

                        if (flag3)
                            SmartCursorHelper._targets.Add(new Tuple<int, int>(i, j));
                    }
                }
            }

            if (SmartCursorHelper._targets.Count > 0)
            {
                float num = -1f;
                Tuple<int, int> tuple = SmartCursorHelper._targets[0];
                for (int k = 0; k < SmartCursorHelper._targets.Count; k++)
                {
                    float num2 = Vector2.Distance(new Vector2(SmartCursorHelper._targets[k].Item1, SmartCursorHelper._targets[k].Item2) * 16f + Vector2.One * 8f, providedInfo.mouse);
                    if (num == -1f || num2 < num)
                    {
                        num = num2;
                        tuple = SmartCursorHelper._targets[k];
                    }
                }

                if (Collision.InTileBounds(tuple.Item1, tuple.Item2, providedInfo.reachableStartX, providedInfo.reachableStartY, providedInfo.reachableEndX, providedInfo.reachableEndY))
                {
                    focusedX = tuple.Item1;
                    focusedY = tuple.Item2;
                }
            }

            SmartCursorHelper._targets.Clear();
        }

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
