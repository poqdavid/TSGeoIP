using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TShockAPI;

namespace TSGeoIP
{
	/// <summary>
	/// Description of Settings.
	/// </summary>
	public class Settings
	{
		private bool _asPrefix = true;
		public bool asPrefix { get { return _asPrefix; } set { _asPrefix = value; } }
		
		private bool _asSuffix = false;
		public bool asSuffix { get { return _asSuffix; } set { _asSuffix = value; } }
		

		private string _PrefixString = "({0}) ";
		public string PrefixString {
			get { return _PrefixString; }
			set {
				if (value != null) {
					_PrefixString = value;
				}
			}
		}
		
		private string _SuffixString = " ({0})";
		public string SuffixString {
			get { return _SuffixString; }
			set {
				if (value != null) {
					_SuffixString = value;
				}
			}
		}

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

		public static void SaveSettting()
		{
			JsonSerializerSettings s = new JsonSerializerSettings();
			s.NullValueHandling = NullValueHandling.Ignore;
			s.ObjectCreationHandling = ObjectCreationHandling.Replace; // without this, you end up with duplicates.
		
			File.WriteAllText(TSGeoIP.Data_Dir + "TSGeoIP.json", JsonConvert.SerializeObject(TSGeoIP.iSettings, Formatting.Indented, s));
		}
 
		public static void LoadSettings()
		{
			
				
			try {
				string json_string = File.ReadAllText(TSGeoIP.Data_Dir + "TSGeoIP.json");
				if (IsJSONValid(json_string)) {
					JsonSerializerSettings s = new JsonSerializerSettings();
					s.NullValueHandling = NullValueHandling.Ignore;
					s.ObjectCreationHandling = ObjectCreationHandling.Replace; // without this, you end up with duplicates.
				
     
					TSGeoIP.iSettings = JsonConvert.DeserializeObject<Settings>(json_string, s);
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
