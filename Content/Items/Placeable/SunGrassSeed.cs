
using ScienceJam.Common.Systems;
using ScienceJam.Content.Tiles;
using ScienceJam.Content.Tiles.SunGrass;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScienceJam.Content.Items.Placeable
{
    public class SunGrassSeed : ModItem
    {
        static int tileOnWhatPlace;
        static int tileToPlace;
        public override void SetStaticDefaults()
        {
            tileOnWhatPlace = TileID.Grass;
            tileToPlace = ModContent.TileType<SunGrassTile>();
        }

        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 100;
            Item.DefaultToPlaceableTile(-1);
            Item.useAnimation = Item.useTime;
            Item.createTile = -1;
            Item.width = 22;
            Item.height = 18;
            SmartCursorSystem.GrassTileRelationship.Add(
                new(Type, tileOnWhatPlace, tileToPlace));
            ItemID.Sets.GrassSeeds[Type] = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe(10)
                .AddIngredient(ItemID.DirtBlock)
                .Register();
        }

        public override bool? UseItem(Player player)
        {
            int i = Player.tileTargetX;
            int j = Player.tileTargetY;

            Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);

            if (tile.HasTile && !tile.IsActuated)
            {
                if (tile.TileType == tileOnWhatPlace && SJUtils.WithinPlacementRange(player, i, j))
                {
                    Main.tile[i, j].TileType = (ushort)tileToPlace;

                    SoundEngine.PlaySound(SoundID.Dig, player.Center);
                    WorldGen.TileFrame(i, j);
                    WorldGen.DiamondTileFrame(i, j);
                    NetMessage.SendTileSquare(-1, i, j, 1);
                    return true;
                }
            }
            return false;
        }


    }
}
