using System.Threading.Tasks;

namespace sunrise_launcher
{
    public interface IManifestFactory
    {
        public IManifest Get(string manifesturl);
    }

    public class ManifestFactory : IManifestFactory
    {
        const string sunrise_api = "sunrise-api";
        const string sunrise_json = "sunrise-json";
        const string tequila_xml = "tequila-xml";

        public IManifest Get(string manifesturl)
        {
            var schema = getSchema(manifesturl);
            switch (schema)
            {
                case sunrise_api:
                    return new SunriseApi(manifesturl);
                case sunrise_json:
                    return new SunriseJson(manifesturl);
                case tequila_xml:
                    return new TequilaXML(manifesturl);
            }
            return null;
        }

        private string getSchema(string manifesturl)
        {
            if (manifesturl.ToLower().EndsWith(".xml"))
                return tequila_xml;
            else if (manifesturl.ToLower().EndsWith(".json"))
                return sunrise_json;
            else
                return sunrise_api;
        }
    }
}
