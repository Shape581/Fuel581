using Life;
using Life.Network;
using Newtonsoft.Json;
using RTG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Fuel581
{
    public class Main : Plugin
    {
        public string directoryPath;
        public string configPath;
        public Config config;
        private System.Random random = new System.Random();
        private Coroutine coroutine;

        public Main(IGameAPI api) : base(api) { }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            directoryPath = Path.Combine(pluginsPath, Assembly.GetExecutingAssembly().GetName().Name);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            configPath = Path.Combine(directoryPath, "config.json");
            if (!File.Exists(configPath))
            {
                config = new Config();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            if (coroutine == null)
                coroutine = Nova.man.StartCoroutine(Loop());
            new SChatCommand("/randomfuelprice", "Régenère le prix de l'essence", "/randomfuelprice", (player, args) =>
            {
                Roll();
                player.Notify("Fuel581", "Le Prix de l'essence a été régeneré.", NotificationManager.Type.Success);
            }).Register();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Fuel581 initalise !");
            Console.ResetColor();
        }

        private IEnumerator Loop()
        {
            while (true)
            {
                Roll();
                yield return new UnityEngine.WaitForSeconds(config.minuteLoopInterval * 60);
            }
        }

        public void Roll()
        {
            var newPrice = random.NextDouble() * (config.maxPrice - config.minPrice) + config.minPrice;
            Nova.server.config.roleplayConfig.fuelPrice = (float)newPrice;
            Nova.server.config.roleplayConfig.Save();
            Nova.server.config.roleplayConfig.Load();
            if (config.sendMessage)
                Nova.server.Players.Where(obj => obj.isSpawned).ToList().ForEach(player => player.SendText($"<color={LifeServer.COLOR_BLUE}>L'essence coute désormais {newPrice:F2}€.</color>"));
        }
    }
}
