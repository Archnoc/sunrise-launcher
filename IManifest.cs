using System.Collections.Generic;

namespace sunrise_launcher
{
    public interface IManifest
    {
        public ManifestMetadata GetMetadata();

        public IEnumerable<ManifestFile> GetFiles();

        public int Count();
    }
}
