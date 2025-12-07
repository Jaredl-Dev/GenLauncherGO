using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenLauncherNet
{
    public static class FoldersHandler
    {
        private static string startPath = Directory.GetCurrentDirectory();

        public static void ApplyActionsToGameFolders(params Action<DirectoryInfo>[] actions)
        {
            ApplyActionsToGameFolder(new DirectoryInfo(startPath), actions);
        }

        public static void ApplyActionsToGameFolder(DirectoryInfo directoryInfo, params Action<DirectoryInfo>[] actions)
        {
            foreach (var directory in directoryInfo.GetDirectories())
            {
                try
                {
                    foreach (var action in actions)
                    {
                        action(directory);
                    }
                }
                catch
                {
                    //TODO logger
                }
            }

            foreach (var dirInfo in directoryInfo.GetDirectories())
                ApplyActionsToGameFolder(dirInfo, actions);
        }
    }
}