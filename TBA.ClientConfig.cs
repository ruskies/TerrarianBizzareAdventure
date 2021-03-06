﻿using System.ComponentModel;
using Terraria.ModLoader.Config;
using TerrarianBizzareAdventure.Stands;

namespace TerrarianBizzareAdventure
{
    public sealed class TBAConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [Header("Visuals")]

        [Label("Enable Stand Aura")]
        [DefaultValue(false)]
        public bool DrawStandAura { get; set; }

        [Header("Other")]

        [Label("Enable Voice Lines")]
        [DefaultValue(false)]
        public bool EnableVA { get; set; }

        public override void OnChanged()
        {
            Stand.DrawStandAura = DrawStandAura;

            TBAMod.Instance.VoiceLinesEnabled = EnableVA;
        }

        public override void OnLoaded()
        {
            Stand.DrawStandAura = DrawStandAura;

            TBAMod.Instance.VoiceLinesEnabled = EnableVA;
        }
    }
}
