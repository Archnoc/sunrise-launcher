using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sunrise_launcher
{
    public class ServerFile
    {
        [JsonPropertyName("servers")]
        public List<Server> Servers { get; set; }
        [JsonPropertyName("selected")]
        public string Selected { get; set; }

        public ServerFile()
        {
            Servers = new List<Server>();
        }

        public static ServerFile Load(string path)
        {
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ServerFile>(json);
        }

        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(path, json);
        }
    }
}
