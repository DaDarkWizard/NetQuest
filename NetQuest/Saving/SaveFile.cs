using NetQuest.World;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetQuest.Saving
{
    public class SaveFile
    {
        private string fileName = "";

        

        public void Save(GameWorld world, List<Player> players)
        {
            if(string.IsNullOrWhiteSpace(fileName))
            {
                throw new Exception("Unable to save when no fileName is provided.");
            }
            SaveStruct saveStruct = new()
            {
                Version = "0.0.1",
                World = world,
                Players = players
            };

            if (world is null)
            {
                return;
            }
            using FileStream file = File.Create($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/NetQuest/saves/{fileName}.world");
            using var writer = new StreamWriter(file);
            writer.Write(JsonConvert.SerializeObject(saveStruct));
        }

        public (GameWorld? world, List<Player>? players) Load()
        {
            using FileStream file = File.Create($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/NetQuest/saves/{fileName}.world");
            using var reader = new StreamReader(file);
            SaveStruct? results = JsonConvert.DeserializeObject<SaveStruct>(reader.ReadToEnd());
            if(results is null)
            {
                throw new Exception("Not a save file.");
            }
            return (results.World, results.Players);
        }

        public class SaveStruct
        {
            public string Version { get; set; } = "0.0.1";
            public GameWorld? World { get; set; }
            public List<Player>? Players { get; set; }
        }
    }
}
