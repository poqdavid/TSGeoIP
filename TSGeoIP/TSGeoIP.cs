using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using System.Threading;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;
using Terraria;
using TerrariaApi;
using TerrariaApi.Server;

namespace TSGeoIP
{
	[ApiVersion(1, 19)]
	public class TSGeoIP : TerrariaPlugin
	{
		// disable once FieldCanBeMadeReadOnly.Local
		private Dictionary<TSPlayer, string> MyPlayersData = new Dictionary<TSPlayer, string>();
		 
		public static string Data_Dir = @"TSGeoIP\";
		public LookupService GeoIP_LS;
		public static Settings iSettings;
        
		public override Version Version {
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}
 
		public override string Name {
			get { return "TSGeoIP Plugin"; }
		}
 
		public override string Author {
			get { return "POQDavid"; }
		}

		public TSGeoIP(Main game)
			: base(game)
		{
			Order = 1;
		}
		public static TerrariaPlugin that;
		public static void ConsoleLOG(string message)
		{
			TShock.Log.Write(message, TraceLevel.Info);
			
			ServerApi.LogWriter.PluginWriteLine(that, message, TraceLevel.Info);
		}

		public override void Initialize()
		{
			that = this;
			ConsoleLOG("Initializing TSGeoIP!");
			
			iSettings = new Settings();
			
			Settings.LoadSettings();
			string GeoIPDB = Data_Dir + "GeoIP.dat";
			GeoIP_LS = new LookupService(GeoIPDB, LookupService.GEOIP_STANDARD);
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			ServerApi.Hooks.ServerChat.Register(this, OnChat);
			ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreet);
			ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
		}
		

		public string GetPlayerFlag(TSPlayer PlayerTemp)
		{
			string temp = "Earth";
			string playerip = PlayerTemp.IP;
			string playername = PlayerTemp.Name;
		
			if (playerip.Contains("127.0.0.")) {
				temp = "Local";
			
			} else {
				if (playername == "POQDavid") {
					temp = "Mars";
				} else {
					
					if (playerip.Contains(".")) {
						try {
							Country c = GeoIP_LS.getCountry(playerip);
							temp = c.getCode();

						} catch (Exception) {

						}
					}
				
				}
			}
			return temp;
		}

        
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnNetGreet);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
				Settings.SaveSettting();
			}
			base.Dispose(disposing);
		}
        
		private void OnInitialize(EventArgs args)
		{
			Commands.ChatCommands.Add(new Command("tsgeoip", TSGeoIPCMD, "tsgeoip"));
		}

		public void OnLeave(LeaveEventArgs args)
		{
			if (MyPlayersData.ContainsKey(TShock.Players[args.Who])) {
		    
				MyPlayersData.Remove(TShock.Players[args.Who]);
			}
		}
		
		public void OnNetGreet(GreetPlayerEventArgs args)
		{
			if (!MyPlayersData.ContainsKey(TShock.Players[args.Who])) {
				MyPlayersData.Add(TShock.Players[args.Who], GetPlayerFlag(TShock.Players[args.Who]));
			} else {
				MyPlayersData[TShock.Players[args.Who]] = GetPlayerFlag(TShock.Players[args.Who]);
			}
		}
		
		private void TSGeoIPCMD(CommandArgs args)
		{
			if (args.Parameters.Count < 1) {
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax:");
				args.Player.SendErrorMessage("{0}tsgeoip reload_set", TShock.Config.CommandSpecifier);
				args.Player.SendErrorMessage("{0}tsgeoip save_set", TShock.Config.CommandSpecifier);
				args.Player.SendErrorMessage("{0}tsgeoip prefix true|false", TShock.Config.CommandSpecifier);
				args.Player.SendErrorMessage("{0}tsgeoip suffix true|false", TShock.Config.CommandSpecifier);
				args.Player.SendErrorMessage("{0}tsgeoip prefix_str \"({0}) \"", TShock.Config.CommandSpecifier);
				args.Player.SendErrorMessage("{0}tsgeoip suffix_str \" ({0})\"", TShock.Config.CommandSpecifier);
				return;
			}
			
			switch (args.Parameters[0].ToLower()) {
				case "reload_set":
					{
						Settings.LoadSettings();
					}
					return;
				case "save_set":
					{
						Settings.SaveSettting();
					}
					return;
				case "prefix":
					{
						try {
							if (args.Parameters[1].ToLower() == "false" || args.Parameters[1].ToLower() == "true" || args.Parameters.Count == 2) {
								iSettings.asPrefix = bool.Parse(args.Parameters[1].ToLower());
								Settings.SaveSettting();
							} else {
								args.Player.SendErrorMessage("Invalid syntax: {0}tsgeoip prefix true|false", TShock.Config.CommandSpecifier);
							}
						} catch (Exception) {
							args.Player.SendErrorMessage("Invalid syntax: {0}tsgeoip prefix true|false", TShock.Config.CommandSpecifier);
						}
					}
					return;
				case "suffix":
					{
						try {
							if (args.Parameters[1].ToLower() == "false" || args.Parameters[1].ToLower() == "true" || args.Parameters.Count == 2) {
								iSettings.asSuffix = bool.Parse(args.Parameters[1].ToLower());
								Settings.SaveSettting();
							} else {
								args.Player.SendErrorMessage("Invalid syntax: {0}tsgeoip suffix true|false", TShock.Config.CommandSpecifier);
							}
						} catch (Exception) {
							args.Player.SendErrorMessage("Invalid syntax: {0}tsgeoip suffix true|false", TShock.Config.CommandSpecifier);
						}
					}
					return;
				case "prefix_str":
					{
						if (args.Parameters.Count == 2 || args.Parameters[1].Contains("{0}")) {

							iSettings.PrefixString = args.Parameters[1];
							Settings.SaveSettting();
						} else {
							args.Player.SendErrorMessage("Invalid syntax: {0}tsgeoip prefix_str \"({0}) \"", TShock.Config.CommandSpecifier);
						}
					}
					return;
				case "suffix_str":
					{
						if (args.Parameters.Count == 2 || args.Parameters[1].Contains("{0}")) {

							iSettings.SuffixString = args.Parameters[1];
							Settings.SaveSettting();
						} else {
							args.Player.SendErrorMessage("Invalid syntax: {0}tsgeoip suffix_str \" ({0})\"", TShock.Config.CommandSpecifier);
						}
					}
					return;
				case "help":
					{
						args.Player.SendInfoMessage("{0}tsgeoip reload_set", TShock.Config.CommandSpecifier);
						args.Player.SendInfoMessage("{0}tsgeoip save_set", TShock.Config.CommandSpecifier);
						args.Player.SendInfoMessage("{0}tsgeoip prefix true|false", TShock.Config.CommandSpecifier);
						args.Player.SendInfoMessage("{0}tsgeoip suffix true|false", TShock.Config.CommandSpecifier);
						args.Player.SendInfoMessage("{0}tsgeoip prefix_str \"({0}) \"", TShock.Config.CommandSpecifier);
						args.Player.SendInfoMessage("{0}tsgeoip suffix_str \" ({0})\"", TShock.Config.CommandSpecifier);
					}
					return;
				default:
					{
						args.Player.SendErrorMessage("Invalid subcommand. Type {0}tsgeoip help for a list of valid commands.", TShock.Config.CommandSpecifier);
					}
					return;
			}
		}
        
		private void OnChat(ServerChatEventArgs args)
		{

			if (args.Handled)
				return;

			var tsplr = TShock.Players[args.Who];
			string tsplr_group_prefix = tsplr.Group.Prefix;
			string tsplr_group_suffix = tsplr.Group.Suffix;
			string tsplr_group_name = tsplr.Group.Name;	     
			string tsplr_name = tsplr.Name;
			
			byte tsplr_group_r = tsplr.Group.R;
			byte tsplr_group_g = tsplr.Group.G;
			byte tsplr_group_b = tsplr.Group.B;
			
			if (iSettings.asPrefix == true) {
				string temp1 = string.Format(iSettings.PrefixString, MyPlayersData[tsplr]);
				tsplr_group_prefix = tsplr.Group.Prefix.Replace("%TSGeoIP-CC-Prefix", temp1);
			} else {
				tsplr_group_prefix = tsplr.Group.Prefix.Replace("%TSGeoIP-CC-Prefix", "");
			}
			
			if (iSettings.asSuffix == true) {
				string temp2 = string.Format(iSettings.SuffixString, MyPlayersData[tsplr]);
				tsplr_group_suffix = tsplr.Group.Suffix.Replace("%TSGeoIP-CC-Suffix", temp2);
			} else {
				tsplr_group_suffix = tsplr.Group.Suffix.Replace("%TSGeoIP-CC-Suffix", "");
			}
			

			if (tsplr == null) {
				args.Handled = true;
				return;
			}

			if (args.Text.Length > 500) {
				TShock.Utils.Kick(tsplr, "Crash attempt via long chat packet.", true);
				args.Handled = true;
				return;
			}
			
			if ((!args.Text.StartsWith(TShock.Config.CommandSpecifier) && !args.Text.StartsWith(TShock.Config.CommandSilentSpecifier))) {
					if (!tsplr.Group.HasPermission(Permissions.canchat)) {
						args.Handled = true;
					} else if (tsplr.mute) {
						tsplr.SendErrorMessage("You are muted!");
						args.Handled = true;
					} else if (!TShock.Config.EnableChatAboveHeads) {
						var text = String.Format(TShock.Config.ChatFormat, tsplr_group_name, tsplr_group_prefix, tsplr_name, tsplr_group_suffix, args.Text);
						TShockAPI.Hooks.PlayerHooks.OnPlayerChat(tsplr, args.Text, ref text);
						TShock.Utils.Broadcast(text, tsplr_group_r, tsplr_group_g, tsplr_group_b);
						args.Handled = true;
					} else {
						Player ply = Main.player[args.Who];
						string name = ply.name;
						ply.name = String.Format(TShock.Config.ChatAboveHeadsFormat, tsplr_group_name, tsplr_group_prefix, tsplr_name, tsplr_group_suffix);
						NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, ply.name, args.Who, 0, 0, 0, 0);
						ply.name = name;
						var text = args.Text;
						TShockAPI.Hooks.PlayerHooks.OnPlayerChat(tsplr, args.Text, ref text);
						NetMessage.SendData((int)PacketTypes.ChatText, -1, args.Who, text, args.Who, tsplr_group_r, tsplr_group_g, tsplr_group_b);
						NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, name, args.Who, 0, 0, 0, 0);

						string msg = String.Format("<{0}> {1}", String.Format(TShock.Config.ChatAboveHeadsFormat, tsplr_group_name, tsplr_group_prefix, tsplr_name, tsplr_group_suffix), text);

						tsplr.SendMessage(msg, tsplr_group_r, tsplr_group_g, tsplr_group_b);

						TSPlayer.Server.SendMessage(msg, tsplr_group_r, tsplr_group_g, tsplr_group_b);
						TShock.Log.Info("Broadcast: {0}", msg);
						args.Handled = true;
					}
			}
		}
        
	}
}
