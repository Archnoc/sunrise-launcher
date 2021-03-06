﻿using Qml.Net;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace sunrise_launcher
{
    public class Server
    {
        [JsonPropertyName("manifest_url")]
        [NotifySignal("manifestUrlChanged")]
        public string ManifestURL { get; set; }
        [JsonPropertyName("install_path")]
        public string InstallPath { get; set; }
        [JsonPropertyName("metadata")]
        public ManifestMetadata Metadata { get; set; }
        [JsonPropertyName("launch")]
        [NotifySignal("launchChanged")]
        public string Launch { get; set; }
        [JsonIgnore]
        public State State { get; set; }
        [JsonIgnore]
        public string Error { get; set; }
        [JsonIgnore]
        public string TaskName { get; set; }
        [JsonIgnore]
        public int TaskDone { get; set; }
        [JsonIgnore]
        public int TaskCount { get; set; }
    }

    public enum State
    {
        Unchecked = 0,
        Ready = 1,
        Updating = 2,
        Error = 3
    }
}