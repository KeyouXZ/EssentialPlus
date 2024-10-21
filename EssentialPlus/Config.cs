using Newtonsoft.Json;

namespace EssentialsPlus;

public class Config
{
    [JsonProperty("Pvp Disable Command")]
    public string[] DisabledCommandsInPvp = new string[]
    {
"eback"
    };

    [JsonProperty("Back Position History")]
    public int BackPositionHistory = 10;

    [JsonProperty("MySql Host")]
    public string MySqlHost = "";

    [JsonProperty("MySql Database Name")]
    public string MySqlDbName = "";

    [JsonProperty("MySql Username")]
    public string MySqlUsername = "";

    [JsonProperty("MySql Password")]
    public string MySqlPassword = "";

    public void Write(string path)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
    public static Config Read(string path) { return File.Exists(path) ? JsonConvert.DeserializeObject<Config>(File.ReadAllText(path)) : new Config(); }
}