﻿using System;
using System.Linq;
using System.Reflection;
using TerrarianBizzareAdventure.Players;
using WebmilioCommons.Managers;

namespace TerrarianBizzareAdventure.Stands
{
    public sealed class StandManager : SingletonManager<StandManager, Stand>
    {
        public override void DefaultInitialize()
        {
            Assembly myAssembly = Assembly.GetAssembly(typeof(Stand));

            foreach (Type type in myAssembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Stand))))
                Add(Activator.CreateInstance(type) as Stand);

            base.DefaultInitialize();
        }


        public Stand GetRandom(TBAPlayer tbaPlayer)
        {
            Stand stand = null;

            while (stand == null || !stand.CanAcquire(tbaPlayer))
                stand = GetRandom();

            return stand;
        }
    }
}