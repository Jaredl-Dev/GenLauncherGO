using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenLauncherNet
{
    public class ReposModsData
    {
        public string LauncherVersion { get; set; }
        public string DownloadLink { get; set; }
        public string VulkanReposData { get; set; }

        public List<ModAddonsAndPatchesRawData> modDatas = new List<ModAddonsAndPatchesRawData>();

        public List<string> globalAddonsData = new List<string>();
        public List<string> originalGameAddons = new List<string>();
        public List<string> originalGamePatches = new List<string>();
        public List<ExecutablesRawData> executables = new List<ExecutablesRawData>();

        public List<AdvertisingData> AdvData = new List<AdvertisingData>();
    }

    public class ModAddonsAndPatchesRawData
    {
        public string ModName { get; set; }
        public string ModLink { get; set; }
        public List<string> ModPatches { get; set; }

        public List<string> ModAddons { get; set; }

        public ModAddonsAndPatchesRawData()
        {
            ModPatches = new List<string>();
            ModAddons = new List<string>();
        }
    }

    public class ExecutablesRawData
    {
        public string ModName { get; set; }
        public string ModLink { get; set; }
        public string DependencyName { get; set; }
    }

    public class AdvertisingData
    {
        public string ModName { get; set; }
        public string ModLink { get; set; }
        public List<string> ImagesData { get; set; }

        public AdvertisingData()
        {
            ImagesData = new List<string>();
        }
    }
}
