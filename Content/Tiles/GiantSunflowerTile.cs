using global::ScienceJam.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ScienceJam.Content.Tiles
{
    internal class GiantSunflowerTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Mod.Logger.Info("GiantSunflowerTile - SetStaticDefaults");

            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            DustType = ModContent.DustType<SunGrassSparkle>();

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileID.Grass };
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 18 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(10, 10, 0));
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Mod.Logger.Info($"PreDraw called for {i},{j}");
            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (!(tile.TileFrameX == 0 && tile.TileFrameY / 18 == 3))
                return;
            Mod.Logger.Info($"PostDraw (anchor) at {i},{j}  frameX:{tile.TileFrameX}  frameY:{tile.TileFrameY}");
            Texture2D tex = ModContent.Request<Texture2D>(
                "ScienceJam/Content/Tiles/GiantSunflowerTile").Value;
            const int frameW = 18, frameH = 18, pad = 1;
            const int srcW = frameW * 3 + pad * 2;
            const int srcH = frameH * 4 + pad * 3;
            int styleGroupWidth = (frameW + pad) * 2;
            int style = tile.TileFrameX / styleGroupWidth;
            Rectangle src = new Rectangle(style * (srcW + pad), 0, srcW, srcH);
            Vector2 topLeft = new Vector2(i * 16, j * 16 - 57);
            Vector2 offScr = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 pos = topLeft - Main.screenPosition + offScr;
            spriteBatch.Draw(
                tex,
                pos,
                src,
                Lighting.GetColor(i, j),
                0f,
                Vector2.Zero,
                2f,
                SpriteEffects.None,
                0f);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Mod.Logger.Info($"KillMultiTile at {i},{j}");
            Item.NewItem(
                new EntitySource_TileBreak(i, j),
                new Rectangle(i * 16, j * 16, 32, 32),
                ModContent.ItemType<Items.Placeable.SunGrassSeed>());
        }
    }
}
