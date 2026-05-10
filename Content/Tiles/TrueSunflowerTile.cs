using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Sunflowerology.Content.Dusts;
using Sunflowerology.Content.Items;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Sunflowerology.Content.Tiles
{
    internal class TrueSunflowerTile : ModTile
    {

        protected Asset<Texture2D> glowTexture = null;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileLighted[Type] = true;

            DustType = ModContent.DustType<TrueSunflowerKillTileDust>();
            

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 8;
            TileObjectData.newTile.FlattenAnchors = true;
            TileObjectData.newTile.Origin = new(0, 7);
            TileObjectData.newTile.AnchorBottom = new(Terraria.Enums.AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.RandomStyleRange = 2;
            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16, 18 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.AnchorValidTiles = NatureData.TilesToData.Keys.ToArray();
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(10, 10, 0));

            glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!closer)
                return;

            Player player = Main.LocalPlayer;

            player.AddBuff(ModContent.BuffType<Buffs.TrueSunflowerBuff>(), 30);

        }

        public override bool CanDrop(int i, int j)
        {
            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (glowTexture == null)
            {
                return;
            }
            Tile tile = Main.tile[i, j];
            var tileData = TileObjectData.GetTileData(Type, 0);
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

            spriteBatch.Draw(
                glowTexture.Value,
                new Vector2(i * 16 - (int)Main.screenPosition.X + tileData.DrawXOffset, j * 16 - (int)Main.screenPosition.Y + tileData.DrawYOffset) + zero,
                new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16),
                Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.3f;
            g = 0.3f;
            b = 0.1f;
        }
    }
}