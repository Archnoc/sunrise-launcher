using System.Threading.Tasks;

namespace sunrise_launcher
{
    public interface IManifestFactory
    {
        public IManifest Get(Server server);
    }

    public class ManifestFactory : IManifestFactory
    {
        const string manifiesta_v1 = "manifiesta-v1";
        const string tequila_xml = "tequila-xml";

        public IManifest Get(Server server)
        {
            var schema = getSchema(server.ManifestURL);
            switch (schema)
            {
                case manifiesta_v1:
                    return new Manifiesta(server.ManifestURL);
                case tequila_xml:
                    return new TequilaXML(server.ManifestURL);
            }
            return null;
        }

        private string getSchema(string manifesturl)
        {
            if (manifesturl.ToLower().EndsWith(".xml"))
                return tequila_xml;
            else
                return manifiesta_v1;
        }
    }
}
