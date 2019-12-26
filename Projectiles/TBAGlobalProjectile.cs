﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TerrarianBizzareAdventure.Stands;
using TerrarianBizzareAdventure.TimeSkip;
using TerrarianBizzareAdventure.TimeStop;

namespace TerrarianBizzareAdventure.Projectiles
{
    public sealed class TBAGlobalProjectile : GlobalProjectile
    {
        public override void SetDefaults(Projectile projectile)
        {
            TimeSkipStates = new List<TimeSkipState>();
        }

        public override bool PreAI(Projectile projectile)
        {
            int tickLimit = TimeStopManagement.TimeStopped && projectile.owner == TimeStopManagement.TimeStopper.player.whoAmI ? 10 : 1;
            IsStopped = TimeStopManagement.TimeStopped && !(projectile.modProjectile is IProjectileHasImmunityToTimeStop iisitts && iisitts.IsNativelyImmuneToTimeStop(projectile)) && RanForTicks > tickLimit && (!(projectile.modProjectile is Stand) && projectile.owner == TimeStopManagement.TimeStopper.player.whoAmI);

            var IsTimeSkipped = TimeSkipManager.IsTimeSkipped;

            if (IsTimeSkipped)
            {
                if (TimeSkipManager.TimeSkippedFor % 4 == 0)
                {
                    TimeSkipStates.Add
                        (
                            new TimeSkipState(projectile.Center, projectile.scale, projectile.rotation, new Rectangle(0, projectile.frame, 0, 0), projectile.direction)
                        );
                }
            }

            if (TimeSkipStates.Count > 12 || (!IsTimeSkipped && TimeSkipStates.Count > 0))
                TimeSkipStates.RemoveAt(0);

            if (IsStopped)
            {
                if (!TimeStopManagement.projectileStates.ContainsKey(projectile))
                    TimeStopManagement.RegisterStoppedProjectile(projectile);

                TimeStopManagement.projectileStates[projectile].PreAI(projectile);

                projectile.frameCounter = 0;

                return false;
            }
            else
            {
                RanForTicks++;
                return true;
            }
        }

        public override bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
        {
            if (TimeSkipManager.IsTimeSkipped)
                lightColor = Color.Red;

            return base.PreDraw(projectile, spriteBatch, lightColor);
        }

        public override void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
        {
            base.PostDraw(projectile, spriteBatch, lightColor);
            if (TimeSkipManager.IsTimeSkipped)
            {
                Texture2D texture = Main.projectileTexture[projectile.type];
                int frameCount = Main.projFrames[projectile.type];
                int frameHeight = texture.Height / frameCount;

                Vector2 drawOrig = new Vector2(texture.Width * 0.5f, (texture.Height / frameCount) * 0.5f);

                for (int i = TimeSkipStates.Count - 1; i > 0; i--)
                {
                    SpriteEffects spriteEffects = TimeSkipStates[i].Direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    spriteBatch.Draw(texture, TimeSkipStates[i].Position - Main.screenPosition, new Rectangle(0, TimeSkipStates[i].Frame.Y * frameHeight, texture.Width, frameHeight), (i == 1 ? lightColor : Color.Red * 0.5f), TimeSkipStates[i].Rotation, drawOrig, TimeSkipStates[i].Scale, spriteEffects, 1f);
                }
            }
        }

        public override bool ShouldUpdatePosition(Projectile projectile) => !IsStopped;


        public bool IsStopped { get; private set; }

        public int RanForTicks { get; private set; }

        public override bool InstancePerEntity => true;
        public override bool CloneNewInstances => true;


        public List<TimeSkipState> TimeSkipStates { get; private set; }
    }
}