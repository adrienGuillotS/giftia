using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace PSQuickDesigner
{
    internal class PresetDetailsHelper

    {
        public const string _PRESET_FILE_NAME = @"MRU New Doc Sizes.json";

        public static List<Preset> GetSavedPresets()
        {
            try
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new PresetConverter());

                string path = GetLocalPresetFolderPath();

                List<string> files = new List<string>();
                if (Directory.Exists(path))
                {
                    files = GetPresetFiles(path);
                }

                //if (files == null || files.Count == 0)
                //{
                //    files = GetPresetFiles(GetAdobePresetFolderPath());
                //}

                foreach (string presetFile in files)
                {
                    //Root root = JsonConvert.DeserializeObject<Root>(File.ReadAllText(presetFile));
                    Root root = JsonConvert.DeserializeObject<Root>(File.ReadAllText(presetFile), settings);
                    return root.Presets;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
        public static bool SavePresets(Root root)
        {
            try
            {
                var settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                settings.Converters.Add(new PresetConverter());

                string path = GetLocalPresetFolderPath();


                Directory.CreateDirectory(path);

                string presetFile = System.IO.Path.Combine(path, _PRESET_FILE_NAME);

                string json = JsonConvert.SerializeObject(root, settings);

                File.WriteAllText(presetFile, json, Encoding.Default);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }
        public static string GetLocalPresetFolderPath()
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string path = System.IO.Path.Combine(userFolderPath, "AppData", "Local", "PsQuickDesigner");

            return path;
        }
        public static string GetAdobePresetFolderPath()
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string adobeFolderPath = System.IO.Path.Combine(userFolderPath, "AppData", "Roaming", "Adobe");
            return adobeFolderPath;
        }
        public static List<string> GetPresetFiles(string path)
        {
            List<string> files = new List<string>();
            try
            {

                // Search for the file in the Adobe folder
                files = Directory.GetFiles(path, _PRESET_FILE_NAME, SearchOption.AllDirectories)?.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return files;
        }
    }
}
