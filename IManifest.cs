using System.Collections.Generic;
using System.Threading.Tasks;

namespace sunrise_launcher
{
    public interface IManifest
    {
        public Task<ManifestMetadata> GetMetadataAsync();

        public Task<IList<ManifestFile>> GetFilesAsync();
    }
}
