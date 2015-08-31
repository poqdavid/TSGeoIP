# TSGeoIP
This is a GeoIP plugin for TShock  

----------

**Well using this plugin is very simple:**  

1. You simply unzip the plugin in to your server's folder  
2. Then you just have to download GeoIP and GeoIP2 Databases  
3. Extract the databases in ```TSGeoIP``` folder  
[GeoIP City Database](http://geolite.maxmind.com/download/geoip/database/GeoLiteCity.dat.gz)  
[GeoIP2 City Database](http://geolite.maxmind.com/download/geoip/database/GeoLite2-City.mmdb.gz)

**Things you can do with this plugin:**

1. You can set Country Code as a prefix or suffix for each player by adding %TSGeoIP-CC-Prefix and %TSGeoIP-CC-Suffix to the user's Prefix and Suffix.
2. You can setup a region limit for your server.
3. You can use the whitelist to allow players to avoid the region limit.


**List of commands:**

    /tsgeoip reload_set
    /tsgeoip save_set
    /tsgeoip prefix true|false
    /tsgeoip suffix true|false
    /tsgeoip prefix_str "({0}) "
    /tsgeoip suffix_str " ({0})"
    /tsgeoip akl <add/remove> <country code>
    /tsgeoip akl_list
    /tsgeoip aklw <add/remove> <player name>
    /tsgeoip aklw_list
