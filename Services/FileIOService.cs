using LED_DDP_DRIVER.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace LED_DDP_DRIVER.Services
{
    public class FileIOService
    {
        private readonly string _folderPath;
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public FileIOService()
        {
            //Path: Documents\LEDDDPAUDIO\settings.json
            _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LEDDDPAUDIO");
            _filePath = Path.Combine(_folderPath, "settings.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        /// <summary>
        /// Loads the application settings from a JSON file. If the file does not exist or is empty, it creates a default settings file and returns the default settings.
        /// </summary>
        public AppConfig LoadSettings()
        {
            try
            {
                // Check if the settings file exists.
                if (!File.Exists(_filePath))
                {
                    return CreateDefaultSettings();
                }

                string jsonString = File.ReadAllText(_filePath);

                // If file is empty, treat it as if it doesn't exist and create default settings.
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return CreateDefaultSettings();
                }

                var config = JsonSerializer.Deserialize<AppConfig>(jsonString);
                return config ?? CreateDefaultSettings();
            }
            catch (Exception)
            {
                return new AppConfig();
            }
        }

        /// <summary>
        /// Creates file structure and uses defaults.
        /// </summary>
        private AppConfig CreateDefaultSettings()
        {
            var defaultConfig = new AppConfig();
            try
            {
                Directory.CreateDirectory(_folderPath);
                string jsonString = JsonSerializer.Serialize(defaultConfig, _jsonOptions);
                File.WriteAllText(_filePath, jsonString);
            }
            catch (Exception)
            {
                // Later log to console
            }
            return defaultConfig;
        }
    }
}
