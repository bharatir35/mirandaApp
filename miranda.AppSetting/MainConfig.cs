using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using System.IO;

namespace miranda.AppSetting
{
    public class Settings
    {
        string _cfgSrc;
        JsonReader rdr;
        public Settings(string cfgSrc)
        {
            if (!System.IO.File.Exists(cfgSrc))
                return;
            try
            {
                rdr = new JsonTextReader(File.OpenText(cfgSrc));
                this._cfgSrc = cfgSrc;
                
            }
            catch
            {

            }





        }



    }
}
