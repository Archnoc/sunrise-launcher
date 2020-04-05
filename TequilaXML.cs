using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace sunrise_launcher
{
    [XmlRoot("manifest")]
    public class TequilaXML : IManifest
    {
        [XmlElement("label")]
        public string Label { get; set; }
        [XmlArray("profiles")]
        [XmlArrayItem("launch")]
        public List<TequilaProfile> Profiles { get; set; }
        [XmlArray("filelist")]
        [XmlArrayItem("file")]
        public List<TequilaFile> FileList { get; set; }

        [XmlIgnore]
        public string Hash { get; set; }

        private static HttpClient client = new HttpClient();
        public static async Task<TequilaXML> Get(Server server)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(TequilaXML));
                var response = await client.GetAsync(server.ManifestURL);
                if (response.IsSuccessStatusCode)
                {
                    var hash = SHA256.Create();
                    using (var reader = await response.Content.ReadAsStreamAsync())
                    using (var hashstream = new CryptoStream(reader, hash, CryptoStreamMode.Read))
                    {
                        var manifest = (TequilaXML)serializer.Deserialize(hashstream);
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

        public ManifestMetadata GetMetadata()
        {
            if (Profiles.Count == 0)
                return null;

            var metadata = new ManifestMetadata();
            metadata.Hash = Hash;
            metadata.Title = Profiles[0].Value;
            metadata.LaunchPath = Profiles[0].Exec;
            metadata.LaunchArgs = Profiles[0].Params;
            return metadata;
        }

        public IEnumerable<ManifestFile> GetFiles()
        {
            var files = new List<ManifestFile>();
            foreach(var tequilaFile in FileList)
            {
                var file = new ManifestFile();
                file.MD5 = tequilaFile.MD5.ToLower();
                file.Path = tequilaFile.Name;
                file.Size = tequilaFile.Size;
                file.Sources = new List<FileSource>();
                foreach(var url in tequilaFile.URL)
                {
                    var source = new FileSource();
                    source.URL = url;
                    file.Sources.Add(source);
                }
                files.Add(file);
            }
            return files;
        }

        public int Count()
        {
            return FileList.Count;
        }
    }

    public class TequilaProfile
    {
        [XmlAttribute("params")]
        public string Params { get; set; }
        [XmlAttribute("exec")]
        public string Exec { get; set; }
        [XmlText]
        public string Value { get; set; }
    }

    public class TequilaFile
    {
        [XmlAttribute("md5")]
        public string MD5 { get; set; }
        [XmlAttribute("size")]
        public long Size { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("url")]
        public List<string> URL { get; set; }
    }
}
