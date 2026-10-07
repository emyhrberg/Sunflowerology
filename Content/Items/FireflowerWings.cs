using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Items
{
	[AutoloadEquip(EquipType.Wings)]
	public class FireflowerWings : ModItem
	{
		public override void SetStaticDefaults() {
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(80, 5, 1f);
		}

		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 20;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
		}

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 0.95f; // Falling glide speed
			ascentWhenRising = 0.05f; // Rising speed
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 1.15f;
			constantAscend = 0.135f;
		}
	}
}
