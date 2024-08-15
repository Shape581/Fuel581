using Life;
using ModKit.Helper;
using ModKit.Interfaces;
using System;
using UnityEngine;
using ModKit.Internal;
using Life.Naos;
using Mirror;
using Life.Network;
using Life.Network.Systems;
using Life.UI;
using Life.API;
using Life.Behaviours;
using Life.Config;
using Life.ServerCreationSystem;
using System.Runtime.InteropServices;


namespace Fuel581
{
    public class Fuel581 : ModKit.ModKit
    {
        private readonly MyEvents _events;

        public Fuel581(IGameAPI api) : base(api)
        {
            PluginInformations = new PluginInformations(AssemblyHelper.GetName(), "1.0.0", "Shape581");
            _events = new MyEvents(api);
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            _events.Init(Nova.server);
            ModKit.Internal.Logger.LogSuccess($"{PluginInformations.SourceName} v{PluginInformations.Version}", "initialisé");
        }

        public class MyEvents : ModKit.Helper.Events
        {
            private readonly System.Random _random;
            private const float MinFuelPrice = 1.0f;
            private const float MaxFuelPrice = 3.0f;

            public MyEvents(IGameAPI api) : base(api)
            {
                _random = new System.Random();
            }

            public override void OnHourPassed()
            {
                base.OnHourPassed();

                double randomDecimal = _random.NextDouble() * (MaxFuelPrice - MinFuelPrice) + MinFuelPrice;

                float newFuelPrice = (float)randomDecimal;

                Nova.server.config.roleplayConfig.fuelPrice = newFuelPrice;
                Nova.server.config.roleplayConfig.Save();
                Nova.server.config.roleplayConfig.Load();
            }
        }
    }
}