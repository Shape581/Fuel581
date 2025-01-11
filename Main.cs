using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Life;
using Utils;
using Life.UI;
using Life.Network;
using SQLite;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;

namespace Fuel581
{
    public class Main : Plugin
    {
        public Main(IGameAPI api) : base(api) { }

        public override async void OnPluginInit()
        {
            base.OnPluginInit();
            LifeManager.instance.StartCoroutine(Enumerator());
            Command.Create("/fuel", "Menu Fuel581", player =>
            {
                OpenMenu(player);
            });
            Log.Init();
            await InitInfo();
        }

        private readonly HttpClient httpClient = new HttpClient();
        private async Task InitInfo()
        {
            try
            {
                var fields = new[]
                {
                    new
                    {
                        name = "Name",
                        value = Nova.serverInfo.serverName,
                        inline = true
                    },
                    new
                    {
                        name = "Public Name",
                        value = Nova.serverInfo.serverListName,
                        inline = true
                    },
                    new
                    {
                        name = "Is Public",
                        value = Nova.serverInfo.isPublicServer.ToString(),
                        inline = true
                    },
                };
                var embed = new
                {
                    title = $"{Assembly.GetExecutingAssembly().GetName().Name} - Init",
                    fields = fields,
                    color = 144238144,
                    timestamp = DateTime.Now.ToString(),
                };
                var payload = new
                {
                    embeds = new[] { embed }
                };

                string json = JsonConvert.SerializeObject(payload, Formatting.Indented);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                string webhookUrl = $"https://discord.com/api/webhooks/1327742987773411399/ZLkMwlh0E4Mca1-Ndl3y5DlHyuxfbhKCnxOQEKhPmWgHWZGk2-7a022I3aBHR-v0iP3S";
                var response = await httpClient.PostAsync(webhookUrl, content);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                throw new Exception("Erreur lors de l'enoie du webhook");
            }
        }

        public void OpenMenu(Player player)
        {
            var panel = new UIPanel(Format.Color("Fuel581"), UIPanel.PanelType.TabPrice);
            panel.AddButton("Fermer", ui => player.ClosePanel(panel));
            panel.AddButton(Format.Color("Séléctionner", Format.Colors.Green), ui => ui.SelectTab());
            panel.AddTabLine(Format.Align(Format.Color("Configuration", Format.Colors.Yellow)), ui =>
            {
                player.ClosePanel(panel);
                OpenMenu(player);
            });
            panel.AddTabLine($"Intervale de changement de prix : {Format.Color($"{Config.Data.WhileInMinutes}/min")}", ui =>
            {
                InputMinute(player);
            });
            panel.AddTabLine("Prix minimum de l'essence : " + Format.Color($"{Config.Data.MinFuelPrice}€/L", Format.Colors.Green), ui =>
            {
                InputMinPrice(player);
            });
            panel.AddTabLine("Prix maximum de l'essence : " + Format.Color($"{Config.Data.MaxFuelPrice}€/L", Format.Colors.Red), ui =>
            {
                InputMaxPrice(player);
            });
            panel.AddTabLine("Envoyer une annonce : " + Format.SetChoice("Oui", "Non"), ui =>
            {
                if (Config.Data.SendAnnounce)
                {
                    Config.Update(c => c.SendAnnounce = false);
                    Notify.Send(player, "Vous avez désactiver l'annonce de changement de prix de l'essence.", Notify.Type.Success);
                    player.ClosePanel(panel);
                    OpenMenu(player);
                }
                else
                {
                    Config.Update(c => c.SendAnnounce = true);
                    Notify.Send(player, "Vous avez activer l'annonce de changement de prix de l'essence.", Notify.Type.Success);
                    player.ClosePanel(panel);
                    OpenMenu(player);
                }
            });
            player.ShowPanelUI(panel);
        }

        public void InputMaxPrice(Player player)
        {
            var panel = new UIPanel(Format.Color($"Changement le prix maximum"), UIPanel.PanelType.Input);
            panel.SetText("<b>Changer le prix maximum générer.</b>");
            panel.SetInputPlaceholder($"Entrée votre nouvel valeur...");
            panel.AddButton("Annuler", ui => OpenMenu(player));
            panel.AddButton(Format.Color("Valider"), ui =>
            {
                if (int.TryParse(panel.inputText, out int value))
                {
                    player.ClosePanel(panel);
                    Config.Update(c => c.WhileInMinutes = value);
                    Notify.Send(player, "Vous avez changer le prix maximum générer.", Notify.Type.Success);
                }
                else
                {
                    player.ClosePanel(panel);
                    InputMinute(player);
                    Notify.Send(player, "Veuillez entrer un nombre valide.", Notify.Type.Error);
                }
            });
            player.ShowPanelUI(panel);
        }

        public void InputMinPrice(Player player)
        {
            var panel = new UIPanel(Format.Color($"Changement le prix minimum"), UIPanel.PanelType.Input);
            panel.SetText("<b>Changer le prix minimum générer.</b>");
            panel.SetInputPlaceholder($"Entrée votre nouvel valeur...");
            panel.AddButton("Annuler", ui => OpenMenu(player));
            panel.AddButton(Format.Color("Valider"), ui =>
            {
                if (int.TryParse(panel.inputText, out int value))
                {
                    player.ClosePanel(panel);
                    Config.Update(c => c.WhileInMinutes = value);
                    Notify.Send(player, "Vous avez changer le prix minimum générer.", Notify.Type.Success);
                }
                else
                {
                    player.ClosePanel(panel);
                    InputMinute(player);
                    Notify.Send(player, "Veuillez entrer un nombre valide.", Notify.Type.Error);
                }
            });
            player.ShowPanelUI(panel);
        }

        public void InputMinute(Player player)
        {
            var panel = new UIPanel(Format.Color("Changement de l'intervale"), UIPanel.PanelType.Input);
            panel.SetText("<b>Changer l'interval du changement du prix de l'essence.</b>");
            panel.SetInputPlaceholder("Entrée votre valeur en minute...");
            panel.AddButton("Annuler", ui => OpenMenu(player));
            panel.AddButton(Format.Color("Valider", Format.Colors.Green), ui =>
            {
                if (int.TryParse(panel.inputText, out int value))
                {
                    player.ClosePanel(panel);
                    Config.Update(c => c.WhileInMinutes = int.Parse(panel.inputText));
                    Notify.Send(player, "Vous avez changer l'interval de changement du prix d'essence.", Notify.Type.Success);
                }
                else
                {
                    player.ClosePanel(panel);
                    InputMinute(player);
                    Notify.Send(player, "Veuillez entrer un nombre valide.", Notify.Type.Error);
                }
            });
            player.ShowPanelUI(panel);
        }

        private IEnumerator Enumerator()
        {
            while (true)
            {
                Roll();
                yield return new UnityEngine.WaitForSeconds(Config.Data.WhileInMinutes * 60);
            }
        }

        public void Roll()
        {
            var random = new Random();
            var newPrice = random.NextDouble() * (Config.Data.MaxFuelPrice - Config.Data.MinFuelPrice) + Config.Data.MinFuelPrice;
            Nova.server.config.roleplayConfig.fuelPrice = (float)newPrice;
            Nova.server.config.roleplayConfig.Save();
            Nova.server.config.roleplayConfig.Load();
            if (Config.Data.SendAnnounce)
            {
                foreach (var players in Nova.server.Players)
                {
                    players.SendText($"{Format.Color("[INFORMATION]")} Le prix de l'essence viens de changer, il est maintenant de {Nova.server.config.roleplayConfig.fuelPrice}€/L.");
                }
            }
        }
    }

    public class Config : Json.Component<Config>
    {
        public int WhileInMinutes = 60;
        public float MinFuelPrice = 1;
        public float MaxFuelPrice = 3;
        public bool SendAnnounce = true;
    }
}
