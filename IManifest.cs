using System;
using System.Collections.Generic;
using System.Text;

namespace sunrise_launcher
{
    public interface IManifest
    {
        public ManifestMetadata GetMetadata();

        public IEnumerable<ManifestFile> GetFiles();

        public int Count();
    }
}
