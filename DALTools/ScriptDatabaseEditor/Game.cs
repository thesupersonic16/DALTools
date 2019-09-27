#pragma warning disable CS0067
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDatabaseEditor
{
    public class Game : INotifyPropertyChanged
    {

        public string GamePath = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public GameLanguage GameLanguage { get; set; }
        public bool EnableResourceLoading { get; set; }

        public string LangPath => Path.Combine(GamePath, "Data", GetLangDirectoryName(GameLanguage));
        public string LangPathRel => Path.Combine("Data", GetLangDirectoryName(GameLanguage));

        public Game(string path, GameLanguage lang)
        {
            GamePath = path;
            GameLanguage = lang;
            EnableResourceLoading = !string.IsNullOrEmpty(path);
        }

        public static string GetLangDirectoryName(GameLanguage lang)
        {
            switch (lang)
            {
                case GameLanguage.English:
                    return "ENG";
                case GameLanguage.Japanese:
                    return "JPN";
                case GameLanguage.Chinese:
                    return "CHN";
                default:
                    return "Data";
            }
        }

    }
}
