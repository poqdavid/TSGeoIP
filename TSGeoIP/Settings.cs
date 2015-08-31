namespace TSGeoIP
{
	// Import statements are placed here
	using System;
	using System.ComponentModel;
	using System.IO;
	using Newtonsoft;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using TShockAPI;

	/// <summary>
	/// And this part is for the settings :)
	/// <list type="bullet">
	/// <item>
	/// <term>Author</term>
	/// <description>POQDavid</description>
	/// </item>
	/// </list>
	/// </summary>
	public class Settings
	{
		///<summary>
		/// Default value for AsPrefix.
		///</summary>
		private bool defaultAsPrefix = true;
		
		///<summary>
		/// Default value for AsSuffix.
		///</summary>
		private bool defaultAsSuffix = false;
		
		///<summary>
		/// Default value for PrefixString.
		///</summary>
		private string defaultPrefixString = "({0}) ";
		
		///<summary>
		/// Default value for SuffixString.
		///</summary>
		private string defaultSuffixString = " ({0})";
		
		///<summary>
		/// Default value for GeoIP_API.
		///</summary>
		private string defaultGeoIPAPI = "GeoIP";
		
		///<summary>
		/// Default value for AKC_List.
		///</summary>
		private System.Collections.Generic.List<string> defaultAKCList = new System.Collections.Generic.List<string> { };
		
		///<summary>
		/// Default value for AKC_White_List.
		///</summary>
		private System.Collections.Generic.List<string> defaultAKCWList = new System.Collections.Generic.List<string> { };
		
		///<summary>
		/// Gets or sets the asPrefix property.
		///</summary>
		[JsonProperty("asPrefix")]
		[DefaultValue(true)]
		public bool AsPrefix { get { return this.defaultAsPrefix; } set { this.defaultAsPrefix = value; } }

		///<summary>
		/// Gets or sets the asSuffix property.
		///</summary>
		[JsonProperty("asSuffix")]
		[DefaultValue(false)]
		public bool AsSuffix { get { return this.defaultAsSuffix; } set { this.defaultAsSuffix = value; } }
		
		///<summary>
		/// Gets or sets the PrefixString property.
		///</summary>
		[JsonProperty("PrefixString")]
		[DefaultValue("({0}) ")]
		public string PrefixString { get { return this.defaultPrefixString; } set { this.defaultPrefixString = value; } }
		
		///<summary>
		/// Gets or sets the SuffixString property.
		///</summary>
		[JsonProperty("SuffixString")]
		[DefaultValue(" ({0})")]
		public string SuffixString { get { return this.defaultSuffixString; } set { this.defaultSuffixString = value; } }
		
		///<summary>
		/// Gets or sets the GeoIP_API property.
		///</summary>
		[JsonProperty("GeoIP_API")]
		[DefaultValue("GeoIP")]
		public string GeoIP_API { get { return this.defaultGeoIPAPI; } set { this.defaultGeoIPAPI = value; } }

		///<summary>
		/// Gets or sets the AutoKickList property.
		///</summary>
		[JsonProperty("AutoKickList")]
		public System.Collections.Generic.List<string> AKC_List { get { return this.defaultAKCList; } set { this.defaultAKCList = value; } }
		
		///<summary>
		/// Gets or sets the AutoKickWhiteList property.
		///</summary>
		[JsonProperty("AutoKickWhiteList")]
		public System.Collections.Generic.List<string> AKC_White_List { get { return this.defaultAKCWList; } set { this.defaultAKCWList = value; } }
		
		/// <summary>
		/// Given the JSON string, validates if it's a correct
		/// JSON string.
		/// </summary>
		/// <param name="json_string">JSON string to validate.</param>
		/// <returns>true or false.</returns>
		public static bool IsJSONValid(string json_string)
		{
			try {
				JToken.Parse(json_string);
				return true;
			} catch (JsonReaderException) {
				TSGeoIP.ConsoleLOG("The json file wasn't valid!");
				return false;
			}
		}
		
		/// <summary>
		/// Saves the plugin settings in TSGeoIP\TSGeoIP.json.
		/// </summary>
		public static void SaveSettting()
		{
			var s = new JsonSerializerSettings();
			s.ObjectCreationHandling = ObjectCreationHandling.Replace; // without this, you end up with duplicates.
		
			File.WriteAllText(TSGeoIP.Data_Dir + "TSGeoIP.json", JsonConvert.SerializeObject(TSGeoIP.iSettings, Formatting.Indented, s));
		}
		
		/// <summary>
		/// Loads the plugin settings from TSGeoIP\TSGeoIP.json.
		/// </summary>
		public static void LoadSettings()
		{
			try {
				string json_string = File.ReadAllText(TSGeoIP.Data_Dir + "TSGeoIP.json");
				if (IsJSONValid(json_string)) {
					var s = new JsonSerializerSettings();
					s.NullValueHandling = NullValueHandling.Ignore;
					s.ObjectCreationHandling = ObjectCreationHandling.Replace; // without this, you end up with duplicates.
                              
					TSGeoIP.iSettings = JsonConvert.DeserializeObject<Settings>(json_string, s);
					//TSGeoIP.ConsoleLOG(TSGeoIP.iSettings.PrefixString);
				} else {
					SaveSettting();
					TSGeoIP.ConsoleLOG("Created the new settings!");
					LoadSettings();
					TSGeoIP.ConsoleLOG("Loaded the new settings!");
				}
			} catch (Exception) {
				TSGeoIP.ConsoleLOG("No setting found!");
				SaveSettting();
				TSGeoIP.ConsoleLOG("Created the settings!");
				LoadSettings();
				TSGeoIP.ConsoleLOG("Loaded the settings!");
			}
		}
	}
}
