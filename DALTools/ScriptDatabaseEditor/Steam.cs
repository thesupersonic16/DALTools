﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDatabaseEditor
{
    // Taken from https://github.com/thesupersonic16/HedgeModManager
    public static class Steam
    {
        public static string SteamLocation;

        public static void Init()
        {
            // Gets Steam's registry subkey
            var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Valve\\Steam");
            // If null then try the 64-bit subkey
            if (key == null)
                key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Valve\\Steam");
            // Checks if the key and value exists
            if (key != null && key.GetValue("InstallPath") is string steamPath)
                SteamLocation = steamPath;
        }

        public static string GetCachedProfileImageURLFromSID64(string SID64)
        {
            return Path.Combine(SteamLocation, "config/avatarcache/", SID64 + ".png");
        }

        public static List<SteamGame> SearchForGames(string preference = null)
        {
            var paths = new List<string>();
            var games = new List<SteamGame>();

            string vdfLocation = Path.Combine(SteamLocation, "steamapps\\libraryfolders.vdf");
            Dictionary<string, object> vdf = null;
            try
            {
                vdf = SteamVDF.Load(vdfLocation);
            }
            catch
            {
            }

            // Default Common Path
            paths.Add(Path.Combine(SteamLocation, "steamapps\\common"));

            // Adds all the custom libraries
            var container = SteamVDF.GetContainer(vdf, "LibraryFolders");
            if (container != null)
            {
                foreach (var library in container)
                {
                    if (int.TryParse(library.Key, out int index))
                    {
                        if (library.Value is Dictionary<string, object> libraryInfo)
                            paths.Add(Path.Combine(libraryInfo["path"] as string ?? string.Empty, "steamapps\\common"));
                        else
                            paths.Add(Path.Combine(library.Value as string ?? string.Empty, "steamapps\\common"));
                    }
                }
            }

            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    string dalPath = Path.Combine(path, "DATE A LIVE Rio Reincarnation\\DATE A LIVE RIO-REINCARNATION.exe");
                    if (File.Exists(dalPath))
                        games.Add(new SteamGame("DATE A LIVE: Rio Reincarnation", dalPath, "1047440"));
                }
            }

            if (preference != null)
                return games.OrderBy(x => x.GameName != preference).ToList();
            else return games;
        }
    }


    public static class SteamVDF
    {
        // Methods
        public static string GetProperty(
            Dictionary<string, object> containers, string name)
        {
            foreach (var value in containers)
            {
                if (value.Key == name)
                {
                    return value.Value as string;
                }
            }

            return null;
        }

        public static Dictionary<string, object> GetContainer(
            Dictionary<string, object> containers, string name)
        {
            foreach (var value in containers)
            {
                if (string.Compare(value.Key, name, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return value.Value as Dictionary<string, object>;
                }
            }

            return null;
        }

        public static Dictionary<string, object> Load(string filePath)
        {
            using (var fs = File.OpenRead(filePath))
            {
                return Load(fs);
            }
        }

        public static Dictionary<string, object> Load(Stream fileStream)
        {
            var defs = new Dictionary<string, object>();
            var reader = new StreamReader(fileStream, true);

            string line, str = "", nm = "";
            bool doReadString = false;
            char c;

            return ReadContainers() ?? new Dictionary<string, object>();

            // Sub-Methods
            Dictionary<string, object> ReadContainers()
            {
                List<Dictionary<string, object>> containers = new List<Dictionary<string, object>>();
                containers.Add(new Dictionary<string, object>());
                string name = "";
                nm = str = "";

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    doReadString = false;

                    for (int i = 0; i < line.Length; ++i)
                    {
                        c = line[i];
                        if (c == '"')
                        {
                            doReadString = !doReadString;

                            if (doReadString)
                            {
                                continue;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(nm))
                                {
                                    nm = str;
                                    str = "";
                                }
                                else
                                {
                                    if (containers.Count != 0)
                                        containers.Last().Add(nm, str);
                                    nm = str = "";
                                }
                            }
                        }
                        else if (c == '{')
                        {
                            var container = new Dictionary<string, object>();
                            containers.Last().Add(nm, container);
                            containers.Add(container);
                            name = nm;
                            nm = "";
                        }
                        else if (c == '}')
                        {
                            if (containers.Count != 0)
                            {
                                var container = containers.Last();
                                containers.Remove(container);
                            }
                            else
                                throw new Exception("Invalid VDF format!");
                        }
                        else if (doReadString)
                        {
                            str += c;
                        }
                    }
                }
                return containers.FirstOrDefault();
            }
        }
    }

    public class SteamGame
    {
        public string GameName { get; set; }
        public string GameID { get; set; }
        public string ExeName { get; set; }
        public string RootDirectory { get; set; }
        public string ExeDirectory { get { return Path.Combine(RootDirectory, ExeName); } }

        public SteamGame(string gameName, string exe, string gameID)
        {
            GameName = gameName;
            RootDirectory = Path.GetDirectoryName(exe);
            ExeName = Path.GetFileName(exe);
            GameID = gameID;
        }

        public void StartGame()
        {
            Process.Start($"steam://rungameid/{GameID}");
        }

        public override string ToString()
        {
            return $"SteamGame {{SteamID: {GameID}, GameName: {GameName}, EXE: {ExeDirectory}}}";
        }
    }
}
