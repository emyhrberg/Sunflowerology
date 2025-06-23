using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ScienceJam.Content.Items.Placeable;
using ScienceJam.Content.Tiles.SunGrass;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScienceJam.Content.Tiles.SunflowerTree
{
    public class SunflowerTree : ModTree
    {
        private static Asset<Texture2D> texture;
        private static Asset<Texture2D> branchesTexture;
        private static Asset<Texture2D> topsTexture;

        public override TreePaintingSettings TreeShaderSettings => new()
        {
            UseSpecialGroups = true,
            SpecialGroupMinimalHueValue = 11f / 72f,
            SpecialGroupMaximumHueValue = 0.25f,
            SpecialGroupMinimumSaturationValue = 0.88f,
            SpecialGroupMaximumSaturationValue = 1f
        };

        public override void SetStaticDefaults()
        {
            // important!!
            GrowsOnTileId = [TileID.Grass, ModContent.TileType<SunGrassTile>()];

            // textures, also important!!
            texture = ModContent.Request<Texture2D>("ScienceJam/Content/Tiles/SunflowerTree/Tiles_5");
            branchesTexture = ModContent.Request<Texture2D>("ScienceJam/Content/Tiles/SunflowerTree/Tree_Branches_1");
            topsTexture = ModContent.Request<Texture2D>("ScienceJam/Content/Tiles/SunflowerTree/Tree_Tops_1");
        }

        // This is the primary texture for the trunk. Branches and foliage use different settings.
        public override Asset<Texture2D> GetTexture() => texture;

        public override int SaplingGrowthType(ref int style)
        {
            style = 0;
            //return ModContent.TileType<Plants.ExampleSapling>();
            return ModContent.TileType<SunflowerSapling>();
        }

        public override void SetTreeFoliageSettings(Tile tile, ref int xoff, 
            ref int treeFrame, ref int floorY, ref int topW, ref int topH)
        {
            topW = 82;   // 246 / 3 columns = 82
            topH = 82;   // frame height
        }

        public override Asset<Texture2D> GetBranchTextures() => branchesTexture;
        public override Asset<Texture2D> GetTopTextures() => topsTexture;

        public override int DropWood()
        {
            return ModContent.ItemType<SunflowerSeedItem>();
        }

        public override bool CanDropAcorn() => true;
        public override int CreateDust() => 7; // default dust for when tree is destroyed

        public override bool Shake(int x, int y, ref bool createLeaves)
        {
            //Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, ModContent.ItemType<Items.Placeable.ExampleBlock>());
            return false;
        }

        public override int TreeLeaf()
        {
            //return ModContent.GoreType<ExampleTreeLeaf>();
            return 0;
        }
    }
}