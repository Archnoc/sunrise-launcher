﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace sunrise_launcher
{
    public class Manifest : ManifestMetadata
    {
        [JsonPropertyName("files")]
        public List<ManifestFile> Files { get; set; }
    }

    public class ManifestMetadata
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("launch_options")]
        public List<LaunchOption> LaunchOptions { get; set; }

        public bool Verify()
        {
            if (LaunchOptions.Count == 0)
            {
                Console.WriteLine("need at least one launch config");
                return false;
            }

            return LaunchOptions.All(x => x.Verify());
        }
    }

    public class LaunchOption
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("path")]
        public string LaunchPath { get; set; }
        [JsonPropertyName("env")]
        public string Env { get; set; }
        [JsonPropertyName("args")]
        public string Args { get; set; }

        public bool Verify()
        {
            if (Path.IsPathFullyQualified(LaunchPath))
            {
                Console.WriteLine("launch path is fully qualified");
                return false;
            }

            return true;
        }
    }

    public class ManifestFile
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("size")]
        public long Size { get; set; }
        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; }
        [JsonPropertyName("md5")]
        public string MD5 { get; set; }
        [JsonPropertyName("sources")]
        public List<FileSource> Sources { get; set; }

        public bool Verify()
        {
            if (Path.Contains(".."))
            {
                Console.WriteLine("illegal sequence in manifest file path: '..' in {0}", Path);
                return false;
            }
                
            if (Path.Contains("~"))
            {
                Console.WriteLine("illegal sequence in manifest file path: '~' in {0}", Path);
                return false;
            }

            if (Path.Contains("$"))
            {
                Console.WriteLine("illegal sequence in manifest file path: '$' in {0}", Path);
                return false;
            }

            if (Path.Contains("%"))
            {
                Console.WriteLine("illegal sequence in manifest file path: '%' in {0}", Path);
                return false;
            }

            if (System.IO.Path.IsPathFullyQualified(Path))
            {
                Console.WriteLine("file path is fully qualified: {0}", Path);
                return false;
            }

            if (Path.Contains("main.js"))
            {
                Console.WriteLine("illegal sequence in manifest file path: 'main.js' in {0}", Path);
                return false;
            }

            if (Path.Contains("main.qml"))
            {
                Console.WriteLine("illegal sequence in manifest file path: 'main.qml' in {0}", Path);
                return false;
            }

            if (Path.Contains("sunrise-launcher"))
            {
                Console.WriteLine("illegal sequence in manifest file path: 'sunrise-launcher' in {0}", Path);
                return false;
            }

            return Sources.All(x => x.Verify());
        }
    }

    public class FileSource
    {
        [JsonPropertyName("url")]
        public string URL { get; set; }

        public bool Verify()
        {
            if (URL.ToLower().Contains("file:"))
            {
                Console.WriteLine("illegal sequence in manifest source url: 'file:' in source {0}", URL);
                return false;
            }

            return true;
        }
    }
}
