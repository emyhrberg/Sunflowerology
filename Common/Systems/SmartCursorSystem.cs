using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScienceJam.Common.Systems
{
    internal class SmartCursorSystem : ModSystem
    {
        // Static list to hold the relationship between grass seeds and the tiles they can be placed on.
        public static List<Tuple<int, int, int>> GrassTileRelationship = new();

        public override void Load()
        {
            On_SmartCursorHelper.Step_GrassSeeds += SmartCursorCustomSeedModification;
        }

        public override void Unload()
        {
            On_SmartCursorHelper.Step_GrassSeeds -= SmartCursorCustomSeedModification;
        }

        public static void SmartCursorCustomSeedModification(Terraria.GameContent.On_SmartCursorHelper.orig_Step_GrassSeeds orig, object providedInfo, ref int focusedX, ref int focusedY)
        {
            //orig(providedInfo, ref focusedX, ref focusedY);
            Type[] nestType = typeof(SmartCursorHelper).GetNestedTypes(BindingFlags.NonPublic);
            FieldInfo providedInfo_item_field = nestType[0].GetField("item");
            FieldInfo providedInfo_reachableStartX_field = nestType[0].GetField("reachableStartX");
            FieldInfo providedInfo_reachableEndX_field = nestType[0].GetField("reachableEndX");
            FieldInfo providedInfo_reachableStartY_field = nestType[0].GetField("reachableStartY");
            FieldInfo providedInfo_reachableEndY_field = nestType[0].GetField("reachableEndY");
            FieldInfo providedInfo_mouse_field = nestType[0].GetField("mouse");
            FieldInfo sCH_targets_field = typeof(SmartCursorHelper).GetField("_targets", BindingFlags.NonPublic | BindingFlags.Static);

            Item providedInfo_item = (Item)providedInfo_item_field.GetValue(providedInfo);
            int providedInfo_reachableStartX = (int)providedInfo_reachableStartX_field.GetValue(providedInfo);
            int providedInfo_reachableEndX = (int)providedInfo_reachableEndX_field.GetValue(providedInfo);
            int providedInfo_reachableStartY = (int)providedInfo_reachableStartY_field.GetValue(providedInfo);
            int providedInfo_reachableEndY = (int)providedInfo_reachableEndY_field.GetValue(providedInfo);
            Vector2 providedInfo_mouse = (Vector2)providedInfo_mouse_field.GetValue(providedInfo);
            List<Tuple<int, int>> targets = (List<Tuple<int, int>>)sCH_targets_field.GetValue(null);

            //Main.NewText($"{}");

            //Main.NewText($"{sCH_targets.GetValue(null)}");

            if (focusedX > -1 || focusedY > -1)
            {
                return;
            }
            int type = providedInfo_item.type;
            if (type < 0 || !ItemID.Sets.GrassSeeds[type])
            {
                return;
            }
            targets.Clear();
            for (int i = providedInfo_reachableStartX; i <= providedInfo_reachableEndX; i++)
            {
                for (int j = providedInfo_reachableStartY; j <= providedInfo_reachableEndY; j++)
                {
                    Tile tile = Main.tile[i, j];

                    bool flag = IsTileSolid(i - 1, j) ||
                        !IsTileSolid(i, j + 1) ||
                        !IsTileSolid(i + 1, j) ||
                        !IsTileSolid(i, j - 1);

                    bool flag2 = !IsTileSolid(i - 1, j - 1) ||
                        !IsTileSolid(i - 1, j + 1) ||
                        !IsTileSolid(i + 1, j + 1) ||
                        !IsTileSolid(i + 1, j - 1);

                    if (tile.HasTile && !tile.IsActuated && (flag || flag2))
                    {
                        bool flag3;
                        switch (type)
                        {
                            default:
                                flag3 = tile.TileType == 0;
                                break;
                            case 59:
                            case 2171:
                                flag3 = tile.TileType == 0 || tile.TileType == 59;
                                break;
                            case 194:
                            case 195:
                                flag3 = tile.TileType == 59;
                                break;
                            case 5214:
                                flag3 = tile.TileType == 57;
                                break;
                        }
                        foreach (var l in GrassTileRelationship)
                        {
                            if (type == l.Item1)
                            {
                                flag3 = tile.TileType == l.Item2;
                                if (flag3)
                                {
                                    break;
                                }
                            }
                        }

                        if (flag3)
                            targets.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            if (targets.Count > 0)
            {

                float num = -1f;
                Tuple<int, int> tuple = targets[0];
                for (int k = 0; k < targets.Count; k++)
                {
                    float num2 = Vector2.Distance(new Vector2((float)targets[k].Item1, (float)targets[k].Item2) * 16f + Vector2.One * 8f, providedInfo_mouse);
                    if (num == -1f || num2 < num)
                    {
                        num = num2;
                        tuple = targets[k];
                    }
                }
                if (Collision.InTileBounds(tuple.Item1, tuple.Item2, providedInfo_reachableStartX, providedInfo_reachableStartY, providedInfo_reachableEndX, providedInfo_reachableEndY))
                {
                    focusedX = tuple.Item1;
                    focusedY = tuple.Item2;
                }
            }
            targets.Clear();

            providedInfo_item_field.SetValue(providedInfo, providedInfo_item);
            providedInfo_reachableStartX_field.SetValue(providedInfo, providedInfo_reachableStartX);
            providedInfo_reachableEndX_field.SetValue(providedInfo, providedInfo_reachableEndX);
            providedInfo_reachableStartY_field.SetValue(providedInfo, providedInfo_reachableStartY);
            providedInfo_reachableEndY_field.SetValue(providedInfo, providedInfo_reachableEndY);
            providedInfo_mouse_field.SetValue(providedInfo, providedInfo_mouse);
            sCH_targets_field.SetValue(null, targets);
            //orig(providedInfo, ref focusedX, ref focusedY);
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
