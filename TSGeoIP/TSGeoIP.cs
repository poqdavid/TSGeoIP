using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Terraria;
using TerrariaApi;
using TerrariaApi.Server;
using TShockAPI;
using System.Threading;


namespace TSGeoIP
{
    [ApiVersion(1, 19)]
    public class TSGeoIP : TerrariaPlugin
    {
        public override Version version
        {
            get { return new Version("1.0"); }
        }
 
        public override string Name
        {
            get { return "TSGeoIP Plugin"; }
        }
 
        public override string Author
        {
            get { return "POQDavid"; }
        }

        public TSGeoIP(Main game) : base(game)
        {
            Order -= 0;
        }
 
        public override void Initialize()
        {

        }
 
        protected override void Dispose(bool disposing)
        {
            base.Dispose( disposing );
        }
    }
}
