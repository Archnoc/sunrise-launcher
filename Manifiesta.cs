using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sunrise_launcher
{
    public class Manifiesta : ManifestMetadata, IManifest
    {
        [JsonPropertyName("files")]
        public List<ManifestFile> Files { get; set; }

        public IEnumerable<ManifestFile> GetFiles()
        {
            return Files;
        }

        public int Count()
        {
            return Files.Count;
        }

        public ManifestMetadata GetMetadata()
        {
            return this;
        }

        private static HttpClient client = new HttpClient();
        public static async Task<Manifiesta> Get(Server server)
        {
            try
            {
                var response = await client.GetAsync(server.ManifestURL);
                if (response.IsSuccessStatusCode)
                {
                    var hash = SHA256.Create();
                    using (var reader = await response.Content.ReadAsStreamAsync())
                    using (var hashstream = new CryptoStream(reader, hash, CryptoStreamMode.Read))
                    {
                        var manifest = await JsonSerializer.DeserializeAsync<Manifiesta>(hashstream);
                        manifest.Hash = Hashing.ByteArrayToHex(hash.Hash);
                        return manifest;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception while retrieving manifest: {0}", ex.Message);
            }
            return null;
        }
    }
}
