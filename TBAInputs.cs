﻿using Terraria.ModLoader;

namespace TerrarianBizzareAdventure
{
    public static class TBAInputs
    {
        public static void Load(Mod mod)
        {
            SummonStand = mod.RegisterHotKey("Summon Stand", "Z");
            StandPose = mod.RegisterHotKey("Pose", "X");
            ContextAction = mod.RegisterHotKey("Context Action", "C");

            ExtraAction01 = mod.RegisterHotKey("Extra Action 01", null);
            ExtraAction02 = mod.RegisterHotKey("Extra Action 02", null);

            /*if(VoiceRecognitionSystem.SuccesfulBoot)
                VoiceRec = mod.RegisterHotKey("Voice controls", "V");
                */
            OpenCollection = mod.RegisterHotKey("Stand album", "[");
        }

        public static void Unload()
        {
            SummonStand = null;
            StandPose = null;
            ContextAction = null;

            ExtraAction01 = null;
            ExtraAction02 = null;

            OpenCollection = null;
        }


        public static ModHotKey SummonStand { get; private set; }
        public static ModHotKey StandPose { get; private set; }
        public static ModHotKey ContextAction { get; private set; }

        public static ModHotKey ExtraAction01 { get; private set; }
        public static ModHotKey ExtraAction02 { get; private set; }

        public static ModHotKey VoiceRec { get; private set; }

        public static ModHotKey OpenCollection { get; private set; }
    }
}
