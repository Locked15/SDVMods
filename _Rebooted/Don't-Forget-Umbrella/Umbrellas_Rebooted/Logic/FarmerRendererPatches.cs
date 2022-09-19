using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;

namespace Umbrellas_Rebooted.Logic
{
	public class FarmerRendererPatches
	{
		private static IMonitor Monitor;

		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}

		public static void draw_Postfix(ref FarmerRenderer __instance, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect,
								Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who,
								Texture2D ___baseTexture, Vector2 ___positionOffset)
		{
			try
			{
				if (ModEntry.drawUmbrella)
				{

					if (ModEntry.isMaleFarmer)
					{
						switch (Game1.player.FarmerSprite.currentFrame)
						{ 
							// Standing back.
							case 12:
								b.Draw(ModEntry.umbrellaOverlayTextureBack, position + origin + ___positionOffset + who.armOffset + new Vector2(0, -4 * scale), new Rectangle(0, 0, 16, 16), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 5.0E-05f);
								break;

							case 13:
							case 14:
							case 22:
							case 23:
								b.Draw(ModEntry.umbrellaOverlayTextureBack, position + origin + ___positionOffset + who.armOffset, new Rectangle(0, 0, 16, 16), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 5.0E-05f);
								break;

							case 113:
								b.Draw(ModEntry.umbrellaOverlayTextureBack, position + origin + ___positionOffset + who.armOffset + new Vector2(0, -20 * scale), new Rectangle(0, 0, 16, 16), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 5.0E-05f);
								break;

							case 117:
								b.Draw(ModEntry.umbrellaOverlayTextureSide, position + origin + ___positionOffset + who.armOffset + new Vector2(0, -20 * scale), new Rectangle(0, 0, 16, 16), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 5.0E-05f);
								break;

							case 107:
								b.Draw(ModEntry.umbrellaOverlayTextureBack, position + origin + ___positionOffset + who.armOffset + new Vector2(0, -24 * scale), new Rectangle(0, 0, 16, 16), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 5.0E-05f);
								break;
						}
					}
					else
					{
						switch (Game1.player.FarmerSprite.currentFrame)
						{ 
							// Standing back.
							case 12:
								b.Draw(ModEntry.umbrellaOverlayTextureBack, position + origin + ___positionOffset + who.armOffset, new Rectangle(0, 0, 16, 16), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 5.0E-05f);
								break;

							case 13:
							case 14:
							case 22:
							case 23:
								b.Draw(ModEntry.umbrellaOverlayTextureBack, position + origin + ___positionOffset + who.armOffset + new Vector2(0, 4 * scale), new Rectangle(0, 0, 16, 16), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 5.0E-05f);
								break;

							case 113:
								b.Draw(ModEntry.umbrellaOverlayTextureBack, position + origin + ___positionOffset + who.armOffset + new Vector2(0, -16 * scale), new Rectangle(0, 0, 16, 16), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 5.0E-05f);
								break;

							case 117:
								b.Draw(ModEntry.umbrellaOverlayTextureSide, position + origin + ___positionOffset + who.armOffset + new Vector2(0, -16 * scale), new Rectangle(0, 0, 16, 16), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 5.0E-05f);
								break;

							case 107:
								b.Draw(ModEntry.umbrellaOverlayTextureBack, position + origin + ___positionOffset + who.armOffset + new Vector2(0, -20 * scale), new Rectangle(0, 0, 16, 16), overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 5.0E-05f);
								break;
						}
					}
				}
			}

			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(draw_Postfix)}:\n{ex}", LogLevel.Error);
			}
		}
	}
}
