using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ScienceJam.Content.Buffs;
using ScienceJam.Content.Tiles.SunflowerStagesOfGrowth;
using ScienceJam.Content.Tiles.SunGrass;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ScienceJam.Content.Tiles.DeadSunflower
{
    internal abstract class FlowerTile : ModTile
    {
        protected abstract TypeOfSunflower TypeOfSunflower { get; }
        protected abstract int RangeOfEffectInTiles { get; }
        protected abstract int EffectBuffID { get; }
        protected abstract int EffetDuration { get; }

        private Asset<Texture2D> glowTexture;

        public override string Texture => "ScienceJam/Content/Tiles/SunflowerStagesOfGrowth/SunflowerWithSeedsTile";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            DustType = DustID.Asphalt;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.AnchorValidTiles = [TileID.Grass, ModContent.TileType<SunGrassTile>()];
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 18 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(10, 10, 0));
            glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!closer)
                return;

            Player player = Main.LocalPlayer;

            float rangePixels = RangeOfEffectInTiles * 16;
            Vector2 tileCenter = new((i + 1) * 16f, (j + 1) * 16f);

            bool withinRange = Vector2.Distance(player.Center, tileCenter) <= rangePixels;
            if (withinRange)
                player.AddBuff(EffectBuffID, timeToAdd: EffetDuration);
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            tileFrameY += (short)((3 * 18 + 20) * (int)TypeOfSunflower);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

            int frameY = 0;

            frameY = (short)((3 * 18 + 20) * (int)TypeOfSunflower);

            spriteBatch.Draw(
                glowTexture.Value,
                new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 1) + zero,
                new Rectangle(tile.TileFrameX, tile.TileFrameY + frameY, 16, 16),
                Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }


    }
}