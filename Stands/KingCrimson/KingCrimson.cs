﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrarianBizzareAdventure.Helpers;
using TerrarianBizzareAdventure.Players;
using TerrarianBizzareAdventure.Projectiles;

namespace TerrarianBizzareAdventure.Stands.KingCrimson
{
    public class KingCrimson : Stand
    {
        public KingCrimson() : base("howDoesItWork", "King Crimson")
        {
            AuraColor = new Color(189, 0, 85);
        }

        private Vector2 _punchRushDirection;

        public override void AddAnimations()
        {
            #region Mandatory
            Animations.Add(ANIMATION_SUMMON, new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCSpawn"), 7, 4));
            Animations.Add(ANIMATION_IDLE, new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCIdle"), 5, 22));
            Animations.Add(ANIMATION_DESPAWN, new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCSpawn"), 7, 4));
            #endregion

            #region Punch
            Animations.Add("PUNCH_R", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCPunchRight"), 4, 5));
            Animations.Add("PUNCH_L", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCPunchLeft"), 4, 5));

            Animations.Add("PUNCH_RU", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCPunchRightU"), 4, 5));
            Animations.Add("PUNCH_LU", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCPunchLeftU"), 4, 5));

            Animations.Add("PUNCH_RD", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCPunchRightD"), 4, 5));
            Animations.Add("PUNCH_LD", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCPunchLeftD"), 4, 5));

            Animations["PUNCH_R"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            Animations["PUNCH_L"].SetNextAnimation(Animations[ANIMATION_IDLE]);

            Animations["PUNCH_RU"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            Animations["PUNCH_LU"].SetNextAnimation(Animations[ANIMATION_IDLE]);

            Animations["PUNCH_RD"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            Animations["PUNCH_LD"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            #endregion

            #region Donut
            Animations.Add("DONUT_PREP", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCDonutPrep"), 15, 4));
            Animations.Add("DONUT_IDLE", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCDonutIdle"), 7, 15, true));
            Animations.Add("DONUT_ATT", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCDonutCommit"), 6, 4));
            Animations.Add("DONUT_UNDO", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCDonutUndo"), 12, 4));

            Animations["DONUT_PREP"].SetNextAnimation(Animations["DONUT_IDLE"]);
            Animations["DONUT_ATT"].SetNextAnimation(Animations["DONUT_UNDO"]);
            Animations["DONUT_UNDO"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            #endregion

            #region Rush
            Animations.Add("RUSH_MID", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCRush"), 4, 4));
            Animations.Add("RUSH_UP", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCRushUp"), 4, 4));
            Animations.Add("RUSH_DOWN", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCRushDown"), 4, 4));

            Animations["RUSH_UP"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            Animations["RUSH_MID"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            Animations["RUSH_DOWN"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            #endregion

            Animations[ANIMATION_SUMMON].SetNextAnimation(Animations[ANIMATION_IDLE]);

            #region Cut
            Animations.Add("CUT_IDLE", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCCutIdle"), 5, 25));

            Animations.Add("CUT_PREP", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCCut"), 20, 3));
            Animations.Add("CUT_ATT", new SpriteAnimation(mod.GetTexture("Stands/KingCrimson/KCYeet"), 13, 3));

            Animations["CUT_ATT"].SetNextAnimation(Animations[ANIMATION_IDLE]);
            Animations["CUT_PREP"].SetNextAnimation(Animations["CUT_IDLE"]);
            Animations["CUT_IDLE"].SetNextAnimation(Animations["CUT_ATT"]);
            #endregion
        }

        public override void AI()
        {
            base.AI();

            projectile.width = (int)CurrentAnimation.FrameSize.X;
            projectile.height = (int)CurrentAnimation.FrameSize.Y;

            if (Animations.Count <= 0)
                return;

            IsFlipped = Owner.direction == 1;

            projectile.timeLeft = 200;
            projectile.friendly = true;

            if (Owner.whoAmI == Main.myPlayer)
            {
                /*if (TBAInputs.StandPose.JustPressed)
                    if (CurrentState == ANIMATION_IDLE)
                        IsTaunting = true;
                    else
                        IsTaunting = false;*/

                if (TBAInputs.SummonStand.JustPressed && CurrentState == ANIMATION_IDLE)
                    CurrentState = ANIMATION_DESPAWN;
            }

            Vector2 lerpPos = Vector2.Zero;

            int xOffset = IsTaunting || CurrentState.Contains("PUNCH") || CurrentState.Contains("ATT") || CurrentState == "DONUT_UNDO" || RushTimer > 0? 34 : -16;

            lerpPos = Owner.Center + new Vector2(xOffset * Owner.direction, -24 + Owner.gfxOffY);

            projectile.Center = Vector2.Lerp(projectile.Center, lerpPos, 0.26f);

            if (CurrentState == ANIMATION_SUMMON)
            {
                Opacity = 1;
            }

            #region Punching
            if (CurrentState == ANIMATION_IDLE)
            {
                if (PunchCounter < 3)
                {
                    if (TBAPlayer.Get(Owner).MouseOneTimeReset > 0)
                    {
                        if (TBAPlayer.Get(Owner).MouseOneTime < 15 && !Owner.controlUseItem)
                        {
                            Owner.direction = Main.MouseWorld.X < Owner.Center.X ? -1 : 1;

                            if (Main.MouseWorld.Y > Owner.Center.Y + 60)
                                CurrentState = "PUNCH_" + (Main.rand.NextBool() ? "R" : "L") + "D";
                            else if (Main.MouseWorld.Y < Owner.Center.Y - 60)
                                CurrentState = "PUNCH_" + (Main.rand.NextBool() ? "R" : "L") + "U";
                            else
                                CurrentState = "PUNCH_" + (Main.rand.NextBool() ? "R" : "L");

                            PunchCounter++;

                            PunchCounterReset = 28;

                            Projectile.NewProjectile(projectile.Center, VectorHelpers.DirectToMouse(projectile.Center, 22f), ModContent.ProjectileType<Punch>(), 120, 3.5f, Owner.whoAmI, projectile.whoAmI);

                        }

                        if (TBAPlayer.Get(Owner).MouseOneTime >= 15)
                        {
                            Owner.direction = Main.MouseWorld.X < Owner.Center.X ? -1 : 1;
                            CurrentState = "CUT_PREP";
                        }
                    }
                }
                else
                {
                    if (Main.MouseWorld.Y > Owner.Center.Y + 60)
                        CurrentState = "RUSH_DOWN";

                    else if (Main.MouseWorld.Y < Owner.Center.Y - 60)
                        CurrentState = "RUSH_UP";

                    else
                        CurrentState = "RUSH_MID";

                    RushTimer = 180;

                    _punchRushDirection = VectorHelpers.DirectToMouse(projectile.Center, 14f);

                    TBAPlayer.Get(Owner).AttackDirectionResetTimer = RushTimer;
                    TBAPlayer.Get(Owner).AttackDirection = Main.MouseWorld.X < projectile.Center.X ? -1 : 1;
                }
            }
            #endregion

            #region Rush
            if (RushTimer > 0)
            {
                if (RushTimer % 2 == 0)
                {
                    if (RushTimer > 12)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int projBack = Projectile.NewProjectile(projectile.Center, _punchRushDirection, ModContent.ProjectileType<KCRushBack>(), 80, 3.5f, Owner.whoAmI);

                            RushPunch rushBack = Main.projectile[projBack].modProjectile as RushPunch;
                            rushBack.ParentProjectile = projectile.whoAmI;
                        }

                        int projFront = Projectile.NewProjectile(projectile.Center, _punchRushDirection, ModContent.ProjectileType<KCRush>(), 80, 3.5f, Owner.whoAmI);

                        RushPunch rushFront = Main.projectile[projFront].modProjectile as RushPunch;
                        rushFront.ParentProjectile = projectile.whoAmI;
                    }
                    else
                    {
                        int projFront = Projectile.NewProjectile(projectile.Center, _punchRushDirection, ModContent.ProjectileType<KCRush>(), 80, 3.5f, Owner.whoAmI);

                        RushPunch rushFront = Main.projectile[projFront].modProjectile as RushPunch;
                        rushFront.ParentProjectile = projectile.whoAmI;
                        rushFront.IsFinalPunch = true;
                    }
                }

                RushTimer--;
            }

            Animations["RUSH_DOWN"].AutoLoop = RushTimer > 0;
            Animations["RUSH_UP"].AutoLoop = RushTimer > 0;
            Animations["RUSH_MID"].AutoLoop = RushTimer > 0;
            #endregion

            if (CurrentState.Contains("PUNCH") || CurrentState.Contains("ATT") || CurrentState.Contains("UNDO"))
                Owner.heldProj = projectile.whoAmI;

            #region Yeet attacc
            // If we do a YEET attack, damage is dealt by stand itself instead of a seperate projectile
            bool yeeting = CurrentState == "CUT_ATT" && CurrentAnimation.CurrentFrame > 3 && CurrentAnimation.CurrentFrame < 9;
            bool donuting = CurrentState == "DONUT_ATT" && CurrentAnimation.CurrentFrame > 2;
            projectile.damage = yeeting || donuting ? 400 : 0;

            Animations["CUT_IDLE"].AutoLoop = Owner.controlUseItem;
            
            if(CurrentState == "CUT_IDLE" && !Owner.controlUseItem)
            {
                CurrentAnimation.ResetAnimation();
                CurrentState = "CUT_ATT";
            }
            #endregion

            if(TBAPlayer.Get(Owner).MouseTwoTime > 15 && CurrentState == ANIMATION_IDLE)
            {
                CurrentState = "DONUT_PREP";
            }

            if(!Owner.controlUseTile && CurrentState == "DONUT_IDLE")
            {
                CurrentState = "DONUT_ATT";
            }

            Animations["DONUT_UNDO"].FrameSpeed = Animations["DONUT_UNDO"].CurrentFrame < 5 ? 9 : 5;



            if (CurrentState == ANIMATION_IDLE && CurrentAnimation.Finished)
                CurrentAnimation.ResetAnimation();

            if (CurrentState == ANIMATION_DESPAWN)
            {
                Opacity = (5 - CurrentAnimation.FrameRect.Y / (int)CurrentAnimation.FrameSize.Y) * 0.2f;

                if (CurrentAnimation.Finished)
                    KillStand();
            }

            if (PunchCounterReset > 0)
                PunchCounterReset--;
            else
                PunchCounter = 0;

        }

        public int PunchCounter { get; private set; }
        public int PunchCounterReset { get; private set; }
        public int RushTimer { get; private set; }
    }
}