﻿using Terraria;
using TerrarianBizzareAdventure.Projectiles;
using TerrarianBizzareAdventure.TimeStop;

namespace TerrarianBizzareAdventure.Stands.StarPlatinum
{
    public class StarPlatinumRush : RushPunch, IProjectileHasImmunityToTimeStop
    {
        public bool IsNativelyImmuneToTimeStop(Projectile projectile) => projectile.owner == TimeStopManagement.TimeStopper.player.whoAmI;


        public override string Texture => "TerrarianBizzareAdventure/Stands/StarPlatinum/StarFist";
    }
}
