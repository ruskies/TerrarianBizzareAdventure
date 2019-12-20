﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrarianBizzareAdventure.Helpers;
using TerrarianBizzareAdventure.Players;
using TerrarianBizzareAdventure.Projectiles;
using TerrarianBizzareAdventure.Projectiles.Misc;
using TerrarianBizzareAdventure.TimeStop;
using System.Linq;

namespace TerrarianBizzareAdventure.Stands.StarPlatinum
{
    public class StarPlatinumStand : Stand
    {
        private const string
            TEXPATH = "Stands/StarPlatinum/",
            PUNCH = "SPPunch_",
            LEFTHAND = "_LeftHand",
            RIGHTHAND = "_RightHand";

        private bool _leftMouseButtonLastState;

        private Vector2 _punchRushDirection;


        public StarPlatinumStand() : base("starPlatinum", "Star Platinum")
        {
            CallSoundPath = "Sounds/SP_Call";
            AuraColor = new Color(1f, 0f, 1f);//new Color(210, 101, 198);//new Color(203, 85, 195);
        }


        public override void AddAnimations()
        {
            Animations.Add(ANIMATION_SUMMON, new SpriteAnimation(mod.GetTexture(TEXPATH + "SPSummon"), 10, 4));
            Animations.Add(ANIMATION_IDLE, new SpriteAnimation(mod.GetTexture(TEXPATH + "SPIdle"), 14, 4, true));

            Animations[ANIMATION_SUMMON].SetNextAnimation(Animations[ANIMATION_IDLE]);

            Animations.Add("MIDDLEPUNCH_LEFTHAND", new SpriteAnimation(mod.GetTexture(TEXPATH + PUNCH + "Middle" + LEFTHAND), 3, 5, false, Animations[ANIMATION_IDLE]) );
            Animations.Add("MIDDLEPUNCH_RIGHTHAND", new SpriteAnimation(mod.GetTexture(TEXPATH + PUNCH + "Middle" + RIGHTHAND), 3, 5, false, Animations[ANIMATION_IDLE]) );

            Animations.Add("DOWNPUNCH_LEFTHAND", new SpriteAnimation(mod.GetTexture(TEXPATH + PUNCH + "Down" + LEFTHAND), 3, 5, false, Animations[ANIMATION_IDLE]) );
            Animations.Add("DOWNPUNCH_RIGHTHAND", new SpriteAnimation(mod.GetTexture(TEXPATH + PUNCH + "Down" + RIGHTHAND), 3, 5, false, Animations[ANIMATION_IDLE]) );

            Animations.Add("UPPUNCH_LEFTHAND", new SpriteAnimation(mod.GetTexture(TEXPATH + PUNCH + "Up" + LEFTHAND), 3, 5, false, Animations[ANIMATION_IDLE]) );
            Animations.Add("UPPUNCH_RIGHTHAND", new SpriteAnimation(mod.GetTexture(TEXPATH + PUNCH + "Up" + RIGHTHAND), 3, 5, false, Animations[ANIMATION_IDLE]) );

            Animations.Add("POSE_TRANSITION", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPPose_Transition"), 15, 4));
            Animations.Add("POSE_TRANSITION_REVERSE", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPPose_Transition"), 15, 4, false, Animations[ANIMATION_IDLE]));
            Animations.Add("POSE_IDLE", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPPose_Idle"), 11, 6, true, Animations[ANIMATION_IDLE]));

            Animations["POSE_TRANSITION"].SetNextAnimation(Animations["POSE_IDLE"]);

            Animations.Add("RUSH_UP", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPRush_Up"), 4, 4));
            Animations.Add("RUSH_DOWN", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPRush_Down"), 4, 4));
            Animations.Add("RUSH_MIDDLE", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPRush_Middle"), 4, 4));

            Animations.Add("BLOCK_IDLE", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPBlockIdle"), 7, 6, true));

            Animations.Add("BLOCK_TRANSITION", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPBlockTransition"), 11, 3, false, Animations["BLOCK_IDLE"]));

            Animations.Add("BLOCK_TRANSITION_REVERSE", new SpriteAnimation(mod.GetTexture(TEXPATH + "SPBlockTransition"), 11, 3, false, Animations[ANIMATION_IDLE]));

            Animations["BLOCK_IDLE"].SetNextAnimation(Animations["BLOCK_TRANSITION_REVERSE"], true);

            Animations.Add(ANIMATION_DESPAWN, new SpriteAnimation(mod.GetTexture(TEXPATH + "SPDespawn"), 6, 4));
        }


        public override void AI()
        {
            base.AI();

            if (Animations.Count <= 0)
                return;

            if(CurrentState == ANIMATION_SUMMON && CurrentAnimation.CurrentFrame < 3)
                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/SP_Spawn"));

            #region Rush
            if (PunchCounterReset > 0)
                PunchCounterReset--;
            else
                PunchCounter = 0;

            if (RushTimer > 1)
            {
                if (CurrentAnimation.Finished)
                    CurrentAnimation.ResetAnimation();

                if (RushTimer % 2 == 0)
                {
                    if (RushTimer > 12)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int projBack = Projectile.NewProjectile(projectile.Center, _punchRushDirection, ModContent.ProjectileType<StarPlatinumRushBack>(), 80, 3.5f, Owner.whoAmI);

                            RushPunch rushBack = Main.projectile[projBack].modProjectile as RushPunch;
                            rushBack.ParentProjectile = projectile.whoAmI;
                        }

                        int projFront = Projectile.NewProjectile(projectile.Center, _punchRushDirection, ModContent.ProjectileType<StarPlatinumRush>(), 80, 3.5f, Owner.whoAmI);

                        RushPunch rushFront = Main.projectile[projFront].modProjectile as RushPunch;
                        rushFront.ParentProjectile = projectile.whoAmI;
                    }
                    else
                    {
                        int projFront = Projectile.NewProjectile(projectile.Center, _punchRushDirection, ModContent.ProjectileType<StarPlatinumRush>(), 80, 3.5f, Owner.whoAmI);

                        RushPunch rushFront = Main.projectile[projFront].modProjectile as RushPunch;
                        rushFront.ParentProjectile = projectile.whoAmI;
                        rushFront.IsFinalPunch = true;
                    }
                }
                RushTimer--;
            }
            else
            {
                if (RushTimer > 0 && CurrentAnimation.Finished)
                {
                    RushTimer--;
                    CurrentState = ANIMATION_IDLE;
                }
            }
            #endregion

            projectile.timeLeft = 200;

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

                if (TBAInputs.ContextAction.JustPressed && CurrentState.Contains("POSE"))
                {
                    if (!TimeStopManagement.TimeStopped)
                        Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/SP_TimeStopCall"));
                    CurrentState = ANIMATION_IDLE;
                    IsTaunting = false;
                    TimeStopDelay = 25;
                }
            }

            Vector2 lerpPos = Vector2.Zero;

            int xOffset = IsPunching || RushTimer > 0 || CurrentState.Contains("POSE") ? 34 : -16;

            if (CurrentState.Contains("BLOCK"))
            {
                Owner.heldProj = projectile.whoAmI;
                lerpPos = Owner.Center + new Vector2(6 * Owner.direction, -24 + Owner.gfxOffY);
            }
            else
            {
                lerpPos = Owner.Center + new Vector2(xOffset * Owner.direction, -24 + Owner.gfxOffY);
            }

            if (CurrentState.Contains("PUNCH"))
                Owner.heldProj = projectile.whoAmI;

            projectile.Center = Vector2.Lerp(projectile.Center, lerpPos, 0.26f);

            if (IsTaunting)
            {
                if (!CurrentState.Contains("POSE"))
                {
                    CurrentState = "POSE_TRANSITION";
                    CurrentAnimation.ResetAnimation();
                }
            }

            if (CurrentState == ANIMATION_SUMMON)
            {
                Opacity = CurrentAnimation.FrameRect.Y / CurrentAnimation.FrameRect.Height * 0.25f;
            }

            if(!CurrentState.Contains("PUNCH"))
            {
                IsPunching = false;
            }

            #region Punch
            if (CurrentState == ANIMATION_IDLE && Owner.controlUseItem && !_leftMouseButtonLastState && !IsPunching && !IsTaunting && RushTimer <= 0)
            {
                if (PunchCounter < 3)
                {
                    if (Main.MouseWorld.Y > Owner.Center.Y + 60)
                        CurrentState = Main.rand.NextBool() ? "DOWNPUNCH_LEFTHAND" : "DOWNPUNCH_RIGHTHAND";

                    else if (Main.MouseWorld.Y < Owner.Center.Y - 60)
                        CurrentState = Main.rand.NextBool() ? "UPPUNCH_LEFTHAND" : "UPPUNCH_RIGHTHAND";

                    else
                        CurrentState = Main.rand.NextBool() ? "MIDDLEPUNCH_LEFTHAND" : "MIDDLEPUNCH_RIGHTHAND";

                    SpawnPunch();

                    SetOwnerDirection();

                    PunchCounter++;

                    PunchCounterReset = 24;

                    IsPunching = true;

                    CurrentAnimation.ResetAnimation();
                }

                else
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Ora"));

                    if (Main.MouseWorld.Y > Owner.Center.Y + 60)
                        CurrentState = "RUSH_DOWN";

                    else if (Main.MouseWorld.Y < Owner.Center.Y - 60)
                        CurrentState = "RUSH_UP";

                    else
                        CurrentState = "RUSH_MIDDLE";

                    PunchCounter = 0;

                    PunchCounterReset = 0;

                    _punchRushDirection = VectorHelpers.DirectToMouse(projectile.Center, 14f);

                    RushTimer = 180;

                    SetOwnerDirection(180);

                    CurrentAnimation.ResetAnimation();
                }
            }
            #endregion

            if (CurrentState == ANIMATION_DESPAWN)
            {
                Opacity = (5 - CurrentAnimation.FrameRect.Y / (int)CurrentAnimation.FrameSize.Y) * 0.2f;

                if (CurrentAnimation.Finished)
                    KillStand();
            }

            if(CurrentState == ANIMATION_IDLE && Owner.controlDown)
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

            _leftMouseButtonLastState = Owner.controlUseItem;

            #region Time Stop

            if (TimeStopDelay > 1)
                TimeStopDelay--;
            else if (TimeStopDelay == 1)
            {
                if (!TimeStopManagement.TimeStopped)
                {
                    Projectile.NewProjectile(Owner.Center, Vector2.Zero, ModContent.ProjectileType<TimeStopVFX>(), 0, 0, Owner.whoAmI);

                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/SP_TimeStopSignal"));
                }
                TimeStopManagement.ToggleTimeStopIfStopper(TBAPlayer.Get(Owner), 5 * Constants.TICKS_PER_SECOND);
                TimeStopDelay--;
            }

            #endregion

            if (CurrentState == "POSE_IDLE" && !IsTaunting)
            {
                CurrentState = ANIMATION_IDLE;
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


        public bool IsPunching { get; private set; }
        public bool InPose { get; private set; }

        public int PunchCounter { get; private set; }
        public int PunchCounterReset { get; private set; }
        public int RushTimer { get; private set; }

        public int TimeStopDelay { get; private set; }
    }
}