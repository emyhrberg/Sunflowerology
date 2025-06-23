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
            GrowsOnTileId = [ModContent.TileType<SunGrassTile>()];

            // textures, also important!!
            texture = ModContent.Request<Texture2D>("ScienceJam/Content/Tiles/SunflowerTree/Tiles_5", AssetRequestMode.AsyncLoad);
            branchesTexture = ModContent.Request<Texture2D>("ScienceJam/Content/Tiles/SunflowerTree/Tree_Branches_1", AssetRequestMode.AsyncLoad);
            topsTexture = ModContent.Request<Texture2D>("ScienceJam/Content/Tiles/SunflowerTree/Tree_Tops_1", AssetRequestMode.AsyncLoad);

        }

        public override int SaplingGrowthType(ref int style)
        {
            style = 0;
            //return ModContent.TileType<Plants.ExampleSapling>();
            return ModContent.TileType<SunflowerSapling>();
        }

        public override Asset<Texture2D> GetBranchTextures()
        {
            return branchesTexture;
        }

        // Token: 0x06000293 RID: 659 RVA: 0x0000EDA4 File Offset: 0x0000CFA4
        public override Asset<Texture2D> GetTexture()
        {
            return texture;
        }

        // Token: 0x06000294 RID: 660 RVA: 0x0000EDAC File Offset: 0x0000CFAC
        public override Asset<Texture2D> GetTopTextures()
        {
            return topsTexture;
        }
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

        public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
        {
        }
    }
}