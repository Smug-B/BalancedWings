using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ModLoader;

namespace BalancedWings
{
	public class BalancedWings : Mod
	{
		public override void Load() => IL.Terraria.Player.Update += Player_Update1;

		private void Player_Update1(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStfld<Player>("fallStart2")))
			{
				Logger.Info("ILEdit failed to match target");
				return;
			}
			cursor.Index -= 7;
			for (int count = 0; count < 8; count++)
			{
				cursor.Remove(); // The greatest code. Definitely will not break if another mod dares add anything.
			}

			if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStloc(55)))
			{
				Logger.Info("Second ILEdit failed to match target");
				return;
			}
			cursor.Emit(OpCodes.Pop);
			cursor.Emit(OpCodes.Ldarg_0);
			cursor.EmitDelegate<Func<Player, int>>((player) =>
			{
				int playerY = (int)(player.position.Y / 16f);
				int fallStart;

				if (player.wings > 0) {
					fallStart = player.fallStart2;
				}
				else {
					fallStart = player.fallStart;
				}

				return playerY - fallStart;
			});

			if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<Player>("wingsLogic")))
			{
				Logger.Info("Third ILEdit failed to match target");
				return;
			}
			cursor.Emit(OpCodes.Pop);
			cursor.Emit(OpCodes.Ldc_I4, 0);

			if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdfld<Player>("mouseInterface")))
			{
				Logger.Info("Fourth ILEdit failed to match target");
				return;
			}
			cursor.Emit(OpCodes.Ldarg_0);
			cursor.EmitDelegate<Action<Player>>((player) =>
			{
				if (player.velocity.Y <= 0)
				{
					player.fallStart2 = (int)(player.position.Y / 16);
				}
			});
		}
	}

	public class BalancedWingsPlayer : ModPlayer
	{
		public override void PostUpdateMiscEffects()
		{
			if (player.wingsLogic > 0 && player.wingTime <= 0 && player.controlJump)
			{
				int playerY = (int)(player.position.Y / 16);
				int playerFallDistance = playerY - player.fallStart2;
				player.fallStart2 += (int)(playerFallDistance * 0.05f);
			}
		}
	}
}