﻿using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using TerrarianBizzareAdventure.Players;
using TerrarianBizzareAdventure.Projectiles;
using TerrarianBizzareAdventure.TimeStop;
using WebmilioCommons.Managers;
using System.Linq;
using TerrarianBizzareAdventure.Drawing;
using WebmilioCommons.Projectiles;

namespace TerrarianBizzareAdventure.Stands
{
    public abstract class Stand : StandardProjectile, IHasUnlocalizedName, IProjectileHasImmunityToTimeStop
    {
        public const string
            ANIMATION_SUMMON = "SUMMON",
            ANIMATION_DESPAWN = "DESPAWN",
            ANIMATION_IDLE = "IDLE";


        protected Stand(string unlocalizedName, string name)
        {
            UnlocalizedName = "stand." + unlocalizedName;
            StandName = name;

            CurrentState = ANIMATION_SUMMON; // first animation *must* be have a key of "SUMMMON"

            Animations = new Dictionary<string, SpriteAnimation>();

            StandCombos = new List<StandCombo>();
        }


        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(StandName);
        }

        public override void SetDefaults()
        {
            projectile.ignoreWater = true;
            projectile.aiStyle = -1;
            projectile.penetrate = -1; // stand istelf should almost never hit anything by its own, this serves as fool proof though
            projectile.tileCollide = false; // tile collisions should be done manually
        }


        public virtual void AddCombos(List<StandCombo> combos) { }


        public abstract void AddAnimations();


        public override bool PreAI()
        {
            projectile.timeLeft = 200;


            if(CurrentState != LastState)
            {
                projectile.netUpdate = true;
            }

            if(!ReverseOffset)
            {
                if (DrawOffset < 5.0f)
                    DrawOffset += 0.1f;
                else
                    ReverseOffset = true;
            }
            else
            {
                if (DrawOffset > 0.0f)
                    DrawOffset -= 0.1f;
                else
                    ReverseOffset = false;
            }

            if (DrawRotation < MathHelper.Pi)
                DrawRotation += 0.07f;
            else
                DrawRotation = 0.0f;


            LastState = CurrentState;

            if (TBAPlayer.Get(Owner).ActiveStandProjectile != projectile.modProjectile)
                KillStand();

            if (Main.dedServ)
                return false;

            if (TBAPlayer.Get(Owner).Stamina <= 0)
                ShouldDie = true;

            if (!HasSetAnimations)
            {
                AddAnimations();

                if (Animations.Count >= 1)
                {
                    Width = (int)Animations[CurrentState].FrameSize.X;
                    Height = (int)Animations[CurrentState].FrameSize.Y;
                }

                HasSetAnimations = true;
            }

            if(ShouldDie && CanDie && CurrentState != ANIMATION_DESPAWN)
            {
                CurrentState = ANIMATION_DESPAWN;
            }

            if (Animations.Count >= 1 && !TimeStopManagement.projectileStates.ContainsKey(projectile))
            {
                Animations[CurrentState].Update();

                SpriteAnimation nextAnimation = CurrentAnimation.NextAnimation;

                bool reverseAnimation = CurrentAnimation.ReverseNextAnimation;

                if (CurrentAnimation.Finished && nextAnimation != null && Animations.ContainsValue(nextAnimation))
                {
                    CurrentAnimation.ResetAnimation(CurrentAnimation.ReversePlayback);
                    CurrentState = Animations.Where(x => x.Value == nextAnimation).Select(x => x.Key).First();
                    CurrentAnimation.ResetAnimation(reverseAnimation);
                }
            }


            if (Owner.dead || !Owner.active)
                KillStand();


            return true;
        }


        public override bool? CanCutTiles()
        {
            return false;
        }


        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(CurrentState);
            writer.Write(IsFlipped);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            CurrentState = reader.ReadString();

            IsFlipped = reader.ReadBoolean();
        }


        // Getting rid of vanilla drawing
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            TBAPlayer tbaPlayer = TBAPlayer.Get(Main.LocalPlayer);

            // Stand shouldn't be drawn in 2 scenarios:
            // 1) We aren't a stand user, so we can't see stands
            // 2) Someone fucked shit up and forgot to fill Animations smh;

            if (!tbaPlayer.StandUser || Animations.Count <= 0)
                return;

            SpriteEffects spriteEffects = IsFlipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (DrawStandAura)
            {
                spriteBatch.Draw(Animations[CurrentState].SpriteSheet, projectile.Center + new Vector2(DrawOffset, 0).RotatedBy(DrawRotation) - Main.screenPosition, Animations[CurrentState].FrameRect, AuraColor * 0.25f * Opacity, projectile.rotation, Animations[CurrentState].DrawOrigin, 1f, spriteEffects, 1f);
                spriteBatch.Draw(Animations[CurrentState].SpriteSheet, projectile.Center + new Vector2(-DrawOffset, 0).RotatedBy(DrawRotation) - Main.screenPosition, Animations[CurrentState].FrameRect, AuraColor * 0.25f * Opacity, projectile.rotation, Animations[CurrentState].DrawOrigin, 1f, spriteEffects, 1f);
                spriteBatch.Draw(Animations[CurrentState].SpriteSheet, projectile.Center + new Vector2(0, DrawOffset).RotatedBy(DrawRotation) - Main.screenPosition, Animations[CurrentState].FrameRect, AuraColor * 0.25f * Opacity, projectile.rotation, Animations[CurrentState].DrawOrigin, 1f, spriteEffects, 1f);
                spriteBatch.Draw(Animations[CurrentState].SpriteSheet, projectile.Center + new Vector2(0, -DrawOffset).RotatedBy(DrawRotation) - Main.screenPosition, Animations[CurrentState].FrameRect, AuraColor * 0.25f * Opacity, projectile.rotation, Animations[CurrentState].DrawOrigin, 1f, spriteEffects, 1f);
            }

            spriteBatch.Draw(Animations[CurrentState].SpriteSheet, projectile.Center - Main.screenPosition, Animations[CurrentState].FrameRect, Color.White * Opacity, projectile.rotation, Animations[CurrentState].DrawOrigin, 1f, spriteEffects, 1f);
        }

        public virtual void KillStand()
        {
            if (CanDie || Owner.dead || !Owner.active)
            {
                projectile.Kill();
                TBAPlayer.Get(Owner).KillStand();

                Animations.Clear();
            }
        }
		
		public override bool PreKill(int timeLeft)
		{
            TBAPlayer.Get(Owner).KillStand();
			
			return base.PreKill(timeLeft);
		}



        public bool IsNativelyImmuneToTimeStop() => true;

        public virtual bool CanAcquire(TBAPlayer tbaPlayer) => true;
        public virtual bool CanUse(TBAPlayer tbaPlayer) => CanAcquire(tbaPlayer);


        public string UnlocalizedName { get; }
        public string StandName { get; }



        public Dictionary<string, SpriteAnimation> Animations { get; }


        public bool IsFlipped { get; set; }
        public bool IsTaunting { get; set; }

        public bool HasSetAnimations { get; private set; }

        public string CurrentState { get; set; }
        public SpriteAnimation CurrentAnimation => Animations.Count > 0 ? Animations[CurrentState] : null;

        public Color AuraColor { get; set; }

        // should be used to force stand into certain animation
        public bool ShouldDie { get; set; }

        // should be used to check whether stand should try to commit NullReferenceException on its existance or not
        // depending on circumstances. I.e. TW
        public virtual bool CanDie => true;


        // Automaticly supplies all future stands with a transparent texture so it won't ever draw
        // Even if it gets past PreDraw somehow
        public sealed override string Texture => "TerrarianBizzareAdventure/Textures/EmptyPixel";


        public float Opacity { get; set; }

        public string CallSoundPath { get; set; }

        public string LastState { get; private set; }

        public float DrawOffset { get; private set; }

        public bool ReverseOffset { get; private set; }

        public List<StandCombo> StandCombos { get; private set; }

        public float DrawRotation { get; private set; }

        public Vector2 PositionOffset { get; set; }

        public float XPosOffset { get; set; }
        public float YPosOffset { get; set; }

        public static bool DrawStandAura { get; set; }
		
		public bool IsDespawning => CurrentState == ANIMATION_DESPAWN;

        public bool IsSpawning => CurrentState == ANIMATION_SUMMON;

        public virtual bool StopsItemUse => false;

        public bool InIdleState
        {
            get => CurrentState == ANIMATION_IDLE;
            set
            {
                if(value == true)
                    CurrentState = ANIMATION_IDLE;
            }
        }
    }
}
