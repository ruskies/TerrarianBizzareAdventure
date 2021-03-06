﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using TerrarianBizzareAdventure.Drawing;
using TerrarianBizzareAdventure.Enums;
using TerrarianBizzareAdventure.Helpers;
using TerrarianBizzareAdventure.Players;
using TerrarianBizzareAdventure.Projectiles;
using TerrarianBizzareAdventure.Projectiles.Misc;
using TerrarianBizzareAdventure.TimeStop;

namespace TerrarianBizzareAdventure.Stands.StardustCrusaders.StarPlatinum
{
    public class StarPlatinumStand : TimeStoppingStand
    {
        private const string
            TEXPATH = "Stands/StardustCrusaders/StarPlatinum/",
            PUNCH = "SPPunch_",
            LEFTHAND = "_LeftHand",
            RIGHTHAND = "_RightHand";


        public StarPlatinumStand() : base("starPlatinum", "Star Platinum")
        {
            CallSoundPath = "Sounds/StarPlatinum/SP_Call";
            AuraColor = new Color(1f, 0f, 1f);//new Color(210, 101, 198);//new Color(203, 85, 195);
        }

        public override void AddCombos(List<StandCombo> combos)
        {
            combos.Add(new StandCombo("Punch", MouseClick.LeftClick.ToString()));
            combos.Add(new StandCombo("Jaw Breaker", MouseClick.LeftHold.ToString()));
            combos.Add(new StandCombo("Punch Barrage", MouseClick.LeftClick.ToString(), MouseClick.LeftClick.ToString(), MouseClick.LeftClick.ToString()));
            combos.Add(new StandCombo("Star Platinum: The World", TBAInputs.ContextAction.GetAssignedKeys()[0].ToString()));
        }

        public override void AddAnimations()
        {

            Animations.Add(ANIMATION_SUMMON, new SpriteAnimation(TEXPATH + "SPSummon", 10, 4));
            Animations.Add(ANIMATION_IDLE, new SpriteAnimation(TEXPATH + "SPIdle", 14, 4, true));

            Animations[ANIMATION_SUMMON].SetNextAnimation(Animations[ANIMATION_IDLE]);

            Animations.Add("MIDDLEPUNCH_LEFTHAND", new SpriteAnimation(TEXPATH + PUNCH + "Middle" + LEFTHAND, 3, 5, false, Animations[ANIMATION_IDLE]) );
            Animations.Add("MIDDLEPUNCH_RIGHTHAND", new SpriteAnimation(TEXPATH + PUNCH + "Middle" + RIGHTHAND, 3, 5, false, Animations[ANIMATION_IDLE]) );

            Animations.Add("DOWNPUNCH_LEFTHAND", new SpriteAnimation(TEXPATH + PUNCH + "Down" + LEFTHAND, 3, 5, false, Animations[ANIMATION_IDLE]) );
            Animations.Add("DOWNPUNCH_RIGHTHAND", new SpriteAnimation(TEXPATH + PUNCH + "Down" + RIGHTHAND, 3, 5, false, Animations[ANIMATION_IDLE]) );

            Animations.Add("UPPUNCH_LEFTHAND", new SpriteAnimation(TEXPATH + PUNCH + "Up" + LEFTHAND, 3, 5, false, Animations[ANIMATION_IDLE]) );
            Animations.Add("UPPUNCH_RIGHTHAND", new SpriteAnimation(TEXPATH + PUNCH + "Up" + RIGHTHAND, 3, 5, false, Animations[ANIMATION_IDLE]) );

            Animations.Add("POSE_TRANSITION", new SpriteAnimation(TEXPATH + "SPPose_Transition", 15, 4));
            Animations.Add("POSE_TRANSITION_REVERSE", new SpriteAnimation(TEXPATH + "SPPose_Transition", 15, 4, false, Animations[ANIMATION_IDLE]));
            Animations.Add("POSE_IDLE", new SpriteAnimation(TEXPATH + "SPPose_Idle", 11, 6, true, Animations[ANIMATION_IDLE]));

            Animations["POSE_TRANSITION"].SetNextAnimation(Animations["POSE_IDLE"]);

            Animations.Add("RUSH_UP", new SpriteAnimation(TEXPATH + "SPRush_Up", 4, 4));
            Animations.Add("RUSH_DOWN", new SpriteAnimation(TEXPATH + "SPRush_Down", 4, 4));
            Animations.Add("RUSH_MIDDLE", new SpriteAnimation(TEXPATH + "SPRush_Middle", 4, 4));

            Animations.Add("BLOCK_IDLE", new SpriteAnimation(TEXPATH + "SPBlockIdle", 7, 6, true));

            Animations.Add("BLOCK_TRANSITION", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPBlockTransition"), 11, 3, false, Animations["BLOCK_IDLE"]));

            Animations.Add("BLOCK_TRANSITION_REVERSE", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPBlockTransition"), 11, 3, false, Animations[ANIMATION_IDLE]));

            Animations["BLOCK_IDLE"].SetNextAnimation(Animations["BLOCK_TRANSITION_REVERSE"], true);

            Animations.Add(ANIMATION_DESPAWN, new SpriteAnimation(mod.GetTexture(TEXPATH + "SPDespawn"), 6, 4));

            Animations.Add(TIMESTOP_ANIMATION, new SpriteAnimation(mod.GetTexture(TEXPATH + "SPPose_Transition"), 15, 4));

            Animations.Add("DONUT_IDLE", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPDonutIdle"), 4, 6, true));
            Animations.Add("DONUT_PREP", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPDonutTransition"), 13, 5));
            Animations.Add("DONUT_PUNCH", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPDonutPunch"), 15, 3));
            Animations.Add("DONUT_PULL", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPDonutRetract"), 8, 5));

            Animations["DONUT_PULL"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            Animations["DONUT_PREP"].SetNextAnimation(Animations["DONUT_IDLE"]);
            Animations["DONUT_PUNCH"].SetNextAnimation(Animations["DONUT_PULL"]);

            Animations["RUSH_UP"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            Animations["RUSH_MIDDLE"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            Animations["RUSH_DOWN"].SetNextAnimation(Animations[ANIMATION_IDLE]);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            target.immune[projectile.owner] = 20;
            target.velocity = new Vector2(0, -12);
        }

        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        {
            target.velocity = new Vector2(0, -16);
        }

        public override void AI()
        {
            base.AI();

            Penetrate = -1;
            projectile.friendly = true;

            if (Animations.Count <= 0)
                return;

            if (CurrentState == ANIMATION_SUMMON)
            {
                if (CurrentAnimation.CurrentFrame < 3)
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/StarPlatinum/SP_Spawn"));

                Opacity = CurrentAnimation.FrameRect.Y / CurrentAnimation.FrameRect.Height * 0.25f;

                XPosOffset = -16;
                YPosOffset = -24;
            }

            if(InIdleState)
                XPosOffset = -16;

            #region Rush


            if (Animations.Count > 0)
            {
                Animations["RUSH_DOWN"].AutoLoop = RushTimer > 0;
                Animations["RUSH_UP"].AutoLoop = RushTimer > 0;
                Animations["RUSH_MIDDLE"].AutoLoop = RushTimer > 0;
            }

            #endregion

            TimeLeft = 200;

            // Runs on clients only
            if (Owner.whoAmI == Main.myPlayer)
            {
                if (TBAInputs.StandPose.JustPressed)
                    if (CurrentState == ANIMATION_IDLE)
                        IsTaunting = true;
                    else
                        IsTaunting = false;

                if (TBAInputs.SummonStand.JustPressed && CurrentState == ANIMATION_IDLE)
                    CurrentState = ANIMATION_DESPAWN;

                if (TBAInputs.ContextAction.JustPressed && InIdleState)
                {
                    TimeStop();
                }
            }

            if(IsPunching ||
                RushTimer > 0 || 
                CurrentState.Contains("POSE") || 
                CurrentState == "DONUT_PUNCH" ||
                CurrentState == "DONUT_PULL"
                || CurrentState == TIMESTOP_ANIMATION)
            {
                XPosOffset = 34;
            }


            if(CurrentState == "DONUT_PUNCH" || CurrentState == "DONUT_PULL" || CurrentState == TIMESTOP_ANIMATION)
                Owner.heldProj = projectile.whoAmI;


            if(CurrentState == "DONUT_PUNCH" && CurrentAnimation.CurrentFrame == 4)
               TBAMod.PlayVoiceLine("Sounds/StarPlatinum/Donut");


            if (CurrentState == "DONUT_PUNCH" && CurrentAnimation.CurrentFrame > 4)
                Damage = 350;
            else
                Damage = 0;


            if (CurrentState.Contains("BLOCK"))
            {
                Owner.heldProj = projectile.whoAmI;
                PositionOffset = Owner.Center + new Vector2(6 * Owner.direction, YPosOffset + Owner.gfxOffY);
            }
            else
            {
                PositionOffset = Owner.Center + new Vector2(XPosOffset * Owner.direction, YPosOffset + Owner.gfxOffY);
            }

            if (CurrentState.Contains("PUNCH"))
                Owner.heldProj = projectile.whoAmI;


            Center = Vector2.Lerp(projectile.Center, PositionOffset, 0.26f);


            if (IsTaunting)
            {
                if (!CurrentState.Contains("POSE"))
                {
                    CurrentState = "POSE_TRANSITION";
                    CurrentAnimation.ResetAnimation();
                }
            }

            if (!CurrentState.Contains("PUNCH"))
            {
                IsPunching = false;
            }


            #region Punch
            if(StopsItemUse)
            if (InIdleState && TBAPlayer.Get(Owner).MouseOneTimeReset > 0 && TBAPlayer.Get(Owner).MouseOneTime < 15 && !Owner.controlUseItem)
            {
                projectile.netUpdate = true;
                Punching(Main.rand.Next(2) == 0);
            }
            #endregion


            if (CurrentState == ANIMATION_DESPAWN)
            {
                Opacity = (5 - CurrentAnimation.FrameRect.Y / (int)CurrentAnimation.FrameSize.Y) * 0.2f;

                XPosOffset += 1;
                YPosOffset += 0.75f;

                if (CurrentAnimation.Finished)
                    KillStand();
            }

            if (CurrentState == ANIMATION_IDLE && Owner.controlDown)
            {
                CurrentState = "BLOCK_TRANSITION";
                CurrentAnimation.ResetAnimation();
            }

            IsFlipped = Owner.direction == 1;


            if (projectile.active)
            {
                Animations["POSE_IDLE"].AutoLoop = IsTaunting;
                Animations["BLOCK_IDLE"].AutoLoop = Owner.controlDown;
            }

            #region Time Stop

            if (TimeStopDelay > 1)
                TimeStopDelay--;
            else if (TimeStopDelay == 1)
            {
                projectile.netUpdate = true;
                if (!TimeStopManagement.TimeStopped)
                {
                    Projectile.NewProjectile(Owner.Center, Vector2.Zero, ModContent.ProjectileType<TimeStopVFX>(), 0, 0, Owner.whoAmI);

                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/StarPlatinum/SP_TimeStopSignal"));
                }

                TimeStopManagement.ToggleTimeStopIfStopper(TBAPlayer.Get(Owner), 7 * Constants.TICKS_PER_SECOND);
                TimeStopDelay--;
            }

            #endregion

            if (CurrentState == "POSE_IDLE" && !IsTaunting)
            {
                CurrentState = ANIMATION_IDLE;
            }

            if (StopsItemUse && TBAPlayer.Get(Owner).MouseOneTime >= 20 && InIdleState)
            {
                TBAPlayer.Get(Owner).CheckStaminaCost(15, true);
                CurrentState = "DONUT_PREP";
            }

            if (CurrentState == "DONUT_IDLE" && !Owner.controlUseItem)
            {
                CurrentAnimation.ResetAnimation();
                CurrentState = "DONUT_PUNCH";
            }
        }

        public void Punching(bool left)
        {
            if (PunchCounter < 2)
            {
                 TBAPlayer.Get(Owner).CheckStaminaCost(2);

                if (Main.MouseWorld.Y > Owner.Center.Y + 60)
                    CurrentState = left ? "DOWNPUNCH_LEFTHAND" : "DOWNPUNCH_RIGHTHAND";

                else if (Main.MouseWorld.Y < Owner.Center.Y - 60)
                    CurrentState = left ? "UPPUNCH_LEFTHAND" : "UPPUNCH_RIGHTHAND";

                else
                    CurrentState = left ? "MIDDLEPUNCH_LEFTHAND" : "MIDDLEPUNCH_RIGHTHAND";


                SpawnPunch();

                SetOwnerDirection();

                PunchCounter++;
                PunchCounterReset = 26;

                IsPunching = true;

                CurrentAnimation.ResetAnimation();
            }

            else
            {
                TBAPlayer.Get(Owner).CheckStaminaCost(16, true);

                TBAMod.PlayVoiceLine("Sounds/StarPlatinum/Ora");

                if (Main.MouseWorld.Y > Owner.Center.Y + 60)
                    CurrentState = "RUSH_DOWN";

                else if (Main.MouseWorld.Y < Owner.Center.Y - 60)
                    CurrentState = "RUSH_UP";

                else
                    CurrentState = "RUSH_MIDDLE";

                PunchCounter = 0;
                PunchCounterReset = 0;

                PunchRushDirection = VectorHelpers.DirectToMouse(projectile.Center, 18f);

                RushTimer = 180;

                int barrage = Projectile.NewProjectile(projectile.Center, PunchRushDirection, ModContent.ProjectileType<StarBarrage>(), 60, 0, Owner.whoAmI);

                if(Main.projectile[barrage].modProjectile is StarBarrage starBarrage)
                {
                    starBarrage.RushDirection = PunchRushDirection;
                    starBarrage.ParentProjectile = projectile.whoAmI;
                }


                SetOwnerDirection(180);

                CurrentAnimation.ResetAnimation();
            }
        }
        

        private void SpawnPunch()
        {
            Projectile.NewProjectile(projectile.Center, VectorHelpers.DirectToMouse(projectile.Center, 22f), ModContent.ProjectileType<Punch>(), 80, 3.5f, Owner.whoAmI, projectile.whoAmI);
        }

        private void SetOwnerDirection(int time = 5)
        {
            TBAPlayer.Get(Owner).AttackDirectionResetTimer = time;
            TBAPlayer.Get(Owner).AttackDirection = Main.MouseWorld.X < projectile.Center.X ? -1 : 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);

            writer.Write(IsTaunting);
            writer.Write(RushTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);

            IsTaunting = reader.ReadBoolean();
            RushTimer = reader.ReadInt32();
        }

        public override bool CanDie => !CurrentState.Contains("DONUT") && CurrentState != TIMESTOP_ANIMATION && !CurrentState.Contains("RUSH") && RushTimer <= 0;


        public bool IsPunching { get; private set; }
        public bool InPose { get; private set; }
    }
}
