namespace TSGeoIP
{
	// Import statements are placed here
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading;
	using MaxMind.GeoIP;
	using MaxMind.GeoIP2;
	using Newtonsoft;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Terraria;
	using TerrariaApi;
	using TerrariaApi.Server;
	using TShockAPI;
	using TShockAPI.DB;
	using TShockAPI.Hooks;

	/// <summary>
	/// This program enables the use of GeoIP and GeoIP2 API
	/// For TShock
	/// <list type="bullet">
	/// <item>
	/// <term>Author</term>
	/// <description>POQDavid</description>
	/// </item>
	/// </list>
	/// </summary>
	[ApiVersion(1, 21)]
	public class TSGeoIP : TerrariaPlugin
	{

		/// <summary>
		/// This is simply where plugin stores data and loads them from.
		/// </summary>
		/// <returns>directory for the plugin's data and settings.</returns>
		public static string Data_Dir = @"TSGeoIP\";
		
		/// <summary>
		/// LookupService of GeoIP API in plugin.
		/// </summary>
		public LookupService GeoIP_LS;
		
		/// <summary>
		/// DatabaseReader of GeoIP2 API in plugin.
		/// </summary>
		public DatabaseReader GeoIP2_DBR;
		
		/// <summary>
		/// This is a static member of the Settings.
		/// </summary>
		public static Settings iSettings;
		
		// disable once FieldCanBeMadeReadOnly.Local
		/// <summary>
		/// To store player data.
		/// </summary>
		private Dictionary<int, string> myPlayersData = new Dictionary<int, string>();
        
		/// <summary>
		/// Plugin's version.
		/// </summary>
		public override Version Version {
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}
 
		/// <summary>
		/// Plugin's name.
		/// </summary>
		public override string Name {
			get { return "TSGeoIP Plugin"; }
		}
		
		/// <summary>
		/// Plugin's author or POQDavid in this case lol.
		/// </summary>
		public override string Author {
			get { return "POQDavid"; }
		}

		/// <summary>
		/// The main program.
		/// </summary>
		/// <param name="game">Well it's clear what it is.</param>
		public TSGeoIP(Main game)
			: base(game)
		{
			this.Order = 1;
		}
		
		/// <summary>
		/// A static member of the plugin.
		/// </summary>
		public static TerrariaPlugin that;
		
		/// <summary>
		/// A simple method to write in both console and log file.
		/// </summary>
		/// <param name="message">LOG message.</param>
		public static void ConsoleLOG(string message)
		{
			TShock.Log.Write(message, TraceLevel.Info);
			ServerApi.LogWriter.PluginWriteLine(that, message, TraceLevel.Info);
		}
		
		/// <summary>
		/// A simple method to write in both console and log file
		/// with an extra option to set TraceLevel.
		/// </summary>
		/// <param name="message">LOG message.</param>
		/// <param name="tl">LOG TraceLevel.</param>
		public static void ConsoleLOG(string message, TraceLevel tl)
		{
			TShock.Log.Write(message, tl);
			ServerApi.LogWriter.PluginWriteLine(that, message, tl);
		}
		
		/// <summary>
		/// Method to Initialize plugin's code.
		/// </summary>
		public override void Initialize()
		{
			that = this;
			ConsoleLOG("Initializing TSGeoIP!");
			
			if (!Directory.Exists(Data_Dir)) {
				ConsoleLOG("Didn't found TSGeoIP folder!");
				Directory.CreateDirectory(Data_Dir);
				ConsoleLOG("Created TSGeoIP folder!");
			} else {
				ConsoleLOG("Found TSGeoIP folder!");
			}
			
			iSettings = new Settings();
			
			Settings.LoadSettings();
			
			string geoIPDB = Data_Dir + "GeoIP.dat";
			string geoIP2DB = Data_Dir + "GeoLite2-City.mmdb";
			
			if (iSettings.GeoIP_API.ToLower() == "geoip") {
				if (File.Exists(geoIPDB)) {
					this.GeoIP_LS = new LookupService(geoIPDB, LookupService.GEOIP_STANDARD);
				} else {
					ConsoleLOG("There is no GeoIP.dat", TraceLevel.Error);
					this.GeoIP_LS = null;
				}
			}
			
			if (iSettings.GeoIP_API.ToLower() == "geoip2") {
				if (File.Exists(geoIP2DB)) {
					
					this.GeoIP2_DBR = new DatabaseReader(geoIP2DB);
					  
				} else {
					ConsoleLOG("There is no GeoLite2-City.mmdb", TraceLevel.Error);
					this.GeoIP_LS = null;
				}
			}
			
			ServerApi.Hooks.GameInitialize.Register(this, this.OnInitialize);
			ServerApi.Hooks.ServerJoin.Register(this, this.OnJoin);
			ServerApi.Hooks.NetGreetPlayer.Register(this, this.OnNetGreet);
			ServerApi.Hooks.ServerChat.Register(this, this.OnChat);
			ServerApi.Hooks.ServerLeave.Register(this, this.OnLeave);
		}
		
		/// <summary>
		/// Method to get player's country ISO code.
		/// </summary>
		/// <param name="playerTemp">Gets the player object.</param>
		/// <returns>Country ISO Code.</returns>
		public string GetPlayerFlag(TSPlayer playerTemp)
		{
			string temp = "Earth";
			string playerip = playerTemp.IP;
			string playername = playerTemp.Name;
		
			if (playerip.Contains("127.0.0.")) {
				temp = "Local";
			
			} else {
				if (playername == "POQDavid") {
					temp = "Mars";
				} else {
					
					if (playerip.Contains(".")) {
						try {
							
							if (this.GeoIP_LS != null) {
								Country c = this.GeoIP_LS.getCountry(playerip);
								temp = c.getCode();
							}
							
							if (this.GeoIP2_DBR != null) {
								var cDB = this.GeoIP2_DBR.City(playerip);
								temp = cDB.Country.IsoCode;
							}

						} catch (Exception ex) {
							ConsoleLOG("Error on getting ip location of player: " + playername, TraceLevel.Error);
							ConsoleLOG(ex.Message, TraceLevel.Error);
						}
					}
				
				}
			}
			return temp.ToLower();
		}
		
		/// <summary>
		/// Method to dispose things needed.
		/// </summary>
		/// <param name="disposing">To dispose or not.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				ServerApi.Hooks.GameInitialize.Deregister(this, this.OnInitialize);
				ServerApi.Hooks.ServerJoin.Deregister(this, this.OnJoin);
				ServerApi.Hooks.NetGreetPlayer.Deregister(this, this.OnNetGreet);
				ServerApi.Hooks.ServerChat.Deregister(this, this.OnChat);
				ServerApi.Hooks.ServerLeave.Deregister(this, this.OnLeave);
				Settings.SaveSettting();
			}
			base.Dispose(disposing);
		}
        
		/// <summary>
		/// Things to do OnInitialize.
		/// </summary>
		/// <param name="args">Containing event data.</param>
		private void OnInitialize(EventArgs args)
		{
			Commands.ChatCommands.Add(new Command("tsgeoip.admin.commands", this.TSGeoIPCMD, "tsgeoip"));
		}

		/// <summary>
		/// This event happens every time a player leaves the server.
		/// </summary>
		/// <param name="args">Containing event data.</param>
		private void OnLeave(LeaveEventArgs args)
		{
			if (this.myPlayersData.ContainsKey(TShock.Players[args.Who].Index)) {
		    
				this.myPlayersData.Remove(TShock.Players[args.Who].Index);
			}
		}
		
		/// <summary>
		/// This event happens every time a player joins the server.
		/// </summary>
		/// <param name="args">Containing event data.</param>
		private void OnJoin(JoinEventArgs args)
		{
			var tsplr = TShock.Players[args.Who];     
			string tsplr_name = tsplr.Name;
			string tsplr_user_name = tsplr.User.Name;
			
			if (iSettings.AKC_List.Contains(this.GetPlayerFlag(tsplr)) & (!iSettings.AKC_White_List.Contains(tsplr_name))) {
				TShock.Utils.Kick(tsplr, "You have been kicked because of region limit", true);
				ConsoleLOG("User: " + tsplr_user_name + " Country Code: " + this.GetPlayerFlag(tsplr) + " Reason: Was kick because of region limit");
			} else {
				//TODO Try to use this "args.Player.RealPlayer"
				if (!this.myPlayersData.ContainsKey(tsplr.Index)) {
					this.myPlayersData.Add(tsplr.Index, this.GetPlayerFlag(tsplr));
				} else {
					this.myPlayersData[tsplr.Index] = this.GetPlayerFlag(tsplr);
				}
			}	
		}
		
		/// <summary>
		/// This event happens every time player really joins the server.
		/// </summary>
		/// <param name="args">Containing event data.</param>
		private void OnNetGreet(GreetPlayerEventArgs args)
		{

		}
		
		/// <summary>
		/// This method is for handling the plugin's commands.
		/// </summary>
		/// <param name="args">Containing event data.</param>
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
				args.Player.SendErrorMessage("{0}tsgeoip akl <add/remove> <country code>", TShock.Config.CommandSpecifier);
				args.Player.SendErrorMessage("{0}tsgeoip akl_list", TShock.Config.CommandSpecifier);
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
								iSettings.AsPrefix = bool.Parse(args.Parameters[1].ToLower());
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
								iSettings.AsSuffix = bool.Parse(args.Parameters[1].ToLower());
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
				case "akl_list":
					{
						args.Player.SendInfoMessage("**AKL LIST**");
						foreach (string ccode in TSGeoIP.iSettings.AKC_List) {
							args.Player.SendInfoMessage("  " + ccode);
						}
						args.Player.SendInfoMessage("**AKL LIST**");
									 
					}
					return;
				case "akl":
					{
						if (args.Parameters.Count != 3) {
							args.Player.SendErrorMessage("Invalid syntax: {0}tsgeoip akl <add/remove> <country code>", TShock.Config.CommandSpecifier);
							return;
						}

						switch (args.Parameters[1].ToLower()) {
							case "add":
								{
									TSGeoIP.iSettings.AKC_List.Add(args.Parameters[2].ToLower());
									args.Player.SendInfoMessage("Added the country code to the list!");
									Settings.SaveSettting();
								}
								return;
							case "remove":
								{
									TSGeoIP.iSettings.AKC_List.Remove(args.Parameters[2].ToLower());
									args.Player.SendInfoMessage("Removed the country code from the list!");
									Settings.SaveSettting();
								}
								return;
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
						args.Player.SendInfoMessage("{0}tsgeoip akl <add/remove> <country code>", TShock.Config.CommandSpecifier);
						args.Player.SendInfoMessage("{0}tsgeoip akl_list", TShock.Config.CommandSpecifier);
					}
					return;
				default:
					{
						args.Player.SendErrorMessage("Invalid subcommand. Type {0}tsgeoip help for a list of valid commands.", TShock.Config.CommandSpecifier);
					}
					return;
			}
		}
        
		/// <summary>
		/// This method is for handling prefix and suffix in chat.
		/// </summary>
		/// <param name="args">Containing event data.</param>
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
			
			if (iSettings.AsPrefix == true) {
				string temp1 = string.Format(iSettings.PrefixString, this.myPlayersData[tsplr.Index]);
				tsplr_group_prefix = tsplr.Group.Prefix.Replace("%TSGeoIP-CC-Prefix", temp1);
			} else {
				tsplr_group_prefix = tsplr.Group.Prefix.Replace("%TSGeoIP-CC-Prefix", string.Empty);
			}
			
			if (iSettings.AsSuffix == true) {
				string temp2 = string.Format(iSettings.SuffixString, this.myPlayersData[tsplr.Index]);
				tsplr_group_suffix = tsplr.Group.Suffix.Replace("%TSGeoIP-CC-Suffix", temp2);
			} else {
				tsplr_group_suffix = tsplr.Group.Suffix.Replace("%TSGeoIP-CC-Suffix", string.Empty);
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
					var text = string.Format(TShock.Config.ChatFormat, tsplr_group_name, tsplr_group_prefix, tsplr_name, tsplr_group_suffix, args.Text);
					TShockAPI.Hooks.PlayerHooks.OnPlayerChat(tsplr, args.Text, ref text);
					TShock.Utils.Broadcast(text, tsplr_group_r, tsplr_group_g, tsplr_group_b);
					args.Handled = true;
				} else {
					Player ply = Main.player[args.Who];
					string name = ply.name;
					ply.name = string.Format(TShock.Config.ChatAboveHeadsFormat, tsplr_group_name, tsplr_group_prefix, tsplr_name, tsplr_group_suffix);
					NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, ply.name, args.Who, 0, 0, 0, 0);
					ply.name = name;
					var text = args.Text;
					TShockAPI.Hooks.PlayerHooks.OnPlayerChat(tsplr, args.Text, ref text);
					NetMessage.SendData((int)PacketTypes.ChatText, -1, args.Who, text, args.Who, tsplr_group_r, tsplr_group_g, tsplr_group_b);
					NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, name, args.Who, 0, 0, 0, 0);

					string msg = string.Format("<{0}> {1}", string.Format(TShock.Config.ChatAboveHeadsFormat, tsplr_group_name, tsplr_group_prefix, tsplr_name, tsplr_group_suffix), text);

					tsplr.SendMessage(msg, tsplr_group_r, tsplr_group_g, tsplr_group_b);

					TSPlayer.Server.SendMessage(msg, tsplr_group_r, tsplr_group_g, tsplr_group_b);
					TShock.Log.Info("Broadcast: {0}", msg);
					args.Handled = true;
				}
			}
		}
        
	}
}
