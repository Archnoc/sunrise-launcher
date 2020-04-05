using System.Threading.Tasks;

namespace sunrise_launcher
{
    public interface IManifestFactory
    {
        public Task<IManifest> Get(Server server);
    }

    public class ManifestFactory : IManifestFactory
    {
        const string manifiesta_v1 = "manifiesta-v1";
        const string tequila_xml = "tequila-xml";

        public async Task<IManifest> Get(Server server)
        {
            var schema = getSchema(server.ManifestURL);
            switch (schema)
            {
                case manifiesta_v1:
                    return await Manifiesta.Get(server);
                case tequila_xml:
                    return await TequilaXML.Get(server);
            }
            return null;
        }

        private string getSchema(string manifesturl)
        {
            if (manifesturl.ToLower().EndsWith(".xml"))
                return tequila_xml;
            else
                return manifiesta_v1; //todo; call endpoint for actual schema
        }
    }
}
