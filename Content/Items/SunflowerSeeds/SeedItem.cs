using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AssGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScienceJam.Content.Tiles.SunflowerStagesOfGrowth;
using StructureHelper.Content.GUI;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ScienceJam.Content.Items.SunflowerSeeds
{
    internal abstract class SeedItem : ModItem
    {
        public NatureData seedData = new();
        public TypeOfSunflower typeOfSunflower = TypeOfSunflower.Sunflower;
        bool updated = false;

        public override string Texture => "ScienceJam/Content/Items/SunflowerSeeds/SeedItem";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SproutTile>());
            Item.width = 20;
            Item.height = 26;
            Item.value = 1000;
        }

        public override bool CanStack(Item source)
        {
            foreach (var tag in NatureTags.AllTags)
            {
                if (seedData[tag] != ((SeedItem)source.ModItem).seedData[tag])
                {
                    return false; // If any tag does not match, do not stack
                }
            }
            return true;
        }

        public override void SaveData(TagCompound tag)
        {
            foreach (var seedTag in NatureTags.AllTags)
            {
                tag[seedTag] = seedData[seedTag];
            }
        }

        public override void LoadData(TagCompound tag)
        {
            foreach (var seedTag in NatureTags.AllTags)
            {
                try
                {
                    if (tag.TryGet(seedTag, out int val))
                    {
                        seedData[seedTag] = val;
                    }
                    else
                    {
                        seedData[seedTag] = 0;
                    }
                }
                catch
                {
                    seedData[seedTag] = 0; // If the tag is not found or fails, set it to 0
                }

            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (var tag in NatureTags.AllTags)
            {
                TooltipLine tooltip = new TooltipLine(Mod, "Info: ", $"{tag}: {seedData[tag]}") { OverrideColor = Color.Yellow };
                tooltips.Add(tooltip);
            }

        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Microsoft.Xna.Framework.Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            
            if(!updated)
            {
                typeOfSunflower = seedData.FindClosestTypeOfSunflower();
                updated = true;
            }
            frame.X = 22 * (int)typeOfSunflower;
            frame.Width = 20;
            spriteBatch.Draw(TextureAssets.Item[Item.type].Value, position, frame, drawColor, 0f, new Vector2(10, 13), 1, SpriteEffects.None, 0f);
            
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            
            if (!updated)
            {
                typeOfSunflower = seedData.FindClosestTypeOfSunflower();
                updated = true;
            }
            Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
            Vector2 drawOrigin = itemFrame.Size() / 2f;
            Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);
            itemFrame.X = 22 * (int)typeOfSunflower; // Adjust the X position based on the type of sunflower
            itemFrame.Width = 20; // Ensure the width is set correctly
            spriteBatch.Draw(TextureAssets.Item[Item.type].Value, drawPosition, itemFrame, lightColor, 0f, new Vector2(10, 13), 1, SpriteEffects.None, 0f);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);
        }
        public override bool? UseItem(Player player)
        {
            SproutEntity.transferData = seedData.Clone();
            return base.UseItem(player);
        }
    }
}
