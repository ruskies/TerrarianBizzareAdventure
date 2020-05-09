﻿using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WebmilioCommons.Projectiles;

namespace TerrarianBizzareAdventure.Stands.KingCrimson
{
    public class DonutPunch : StandardProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 48;

            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 8;

            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.ai[1] = -1;
        }


        public override void AI()
        {
            projectile.netUpdate = true;


            var playerDirection = Owner.direction;

            Vector2 offset = new Vector2(36 * playerDirection, AI1 > -1 ? -8 : 0);

            projectile.Center = ParentProjectile.Center + offset;


            if (DonutTarget != null)
            {
                for (int i = 0; i < 3; i++)
                    Dust.NewDust(projectile.Center - new Vector2(playerDirection == 1 ? 8 : 0, 0), 0, 0, DustID.Blood, 8 * Owner.direction, -2, 0, default(Color), 1.5f);


                if (DonutType == TargetType.NPC)
                {
                    NPC npc = DonutTarget as NPC;

                    bool shouldNotPull = npc.type == NPCID.WallofFlesh || npc.type == NPCID.WallofFleshEye;

                    if (shouldNotPull)
                        projectile.Center = DonutTarget.Center;
                    else
                        DonutTarget.Center = Center;
                }


                if (DonutType == TargetType.Player)
                {
                    DonutTarget.velocity = Vector2.Zero;
                    DonutTarget.Center = projectile.Center;
                }
            }
        }


        public override void SendExtraAI(BinaryWriter writer)
        {
            if (DonutType != TargetType.None)
                writer.Write((byte)DonutType);

            if (DonutTarget != null)
                writer.Write(DonutTarget.whoAmI);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            DonutType = (TargetType)reader.ReadByte();

            if (DonutType == TargetType.NPC)
                DonutTarget = Main.npc[reader.ReadInt32()];

            if (DonutType == TargetType.Player)
                DonutTarget = Main.player[reader.ReadInt32()];
        }


        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);


            DonutMissedCheckAndUpdate();


            projectile.damage = 600;


            if (target.life - damage > 0)
            {
                DonutTarget = target;
                DonutType = TargetType.NPC;

                AI1 = target.whoAmI;
            }
            else
                projectile.Kill();


            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Punch" + Main.rand.Next(1, 5)).WithVolume(.2f));
        }

        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        {
            base.ModifyHitPvp(target, ref damage, ref crit);


            DonutMissedCheckAndUpdate();


            Damage = 600;


            if (target.statLife - damage > 0)
            {
                DonutTarget = target;
                DonutType = TargetType.Player;

                AI1 = target.whoAmI;
            }
            else
                projectile.Kill();


            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Punch" + Main.rand.Next(1, 5)).WithVolume(.2f));
        }


        private void DonutMissedCheckAndUpdate()
        {
            if (!(ParentProjectile.modProjectile is KingCrimson kc))
                return;

            if (kc.HasMissedDonut)
                TimeLeft = 75;

            kc.HasMissedDonut = false;
        }


        public override bool? CanHitNPC(NPC target)
        {
            switch (AI1)
            {
                case -5:
                    return false;
                case -1:
                    return true;
            }


            if (target.whoAmI == (int)AI1 && TimeLeft <= 2)
                return true;


            return false;
        }

        public override bool CanHitPvp(Player target)
        {
            switch (AI1)
            {
                case -5:
                    return false;
                case -1:
                    return true;
            }


            if (target.whoAmI == (int)AI1 && TimeLeft <= 2)
                return true;


            return false;
        }


        public sealed override string Texture => "TerrarianBizzareAdventure/Textures/EmptyPixel";


        private Projectile ParentProjectile => Main.projectile[(int)AI0];

        public Entity DonutTarget { get; private set; }

        public TargetType DonutType { get; private set; }
    }

    public enum TargetType : byte
    {
        None,
        Player,
        NPC
    }
}
