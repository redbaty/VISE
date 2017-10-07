using Akavache;
using System.Collections.Generic;
using VISE.Models;

namespace VISE
{
    internal static class Globals
    {
        public static ComputerModel CurrentComputerModel { get; set; }
        public static List<ComputerModel> ComputerModels { get; set; } = new List<ComputerModel>();

        public static void SaveComputerModels()
        {
            BlobCache.UserAccount.InsertObject(Keys.Fixedmodels, ComputerModels);
        }

        public static class Keys
        {
            public const string Fixedmodels = "fixedModels";
        }
    }
}