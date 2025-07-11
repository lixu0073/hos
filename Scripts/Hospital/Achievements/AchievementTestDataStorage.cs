using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

class AchievementTestDataStorage
{
    string path = "C:/Users/jcieszyn/Documents/hospital/achievements.zapis";
    Dictionary<string, string> data = new Dictionary<string, string>();

    // Metoda zapisuje do path zserializowane achievementList
    public void TestSaveAchievementList(List<Achievement> achievementList)
    {
        foreach (Achievement ai in achievementList)
        {
            if (!data.ContainsKey(ai.id))
                data.Add(ai.id, ai.prepareDataToSave());
            else
                data[ai.id] = ai.prepareDataToSave();
        }

        if (!File.Exists(path))
        {
            var p = File.Create(path);
            p.Close();
        }
        XmlSerializer serializer = new XmlSerializer(typeof(List<KeyValuePair<string, string>>));
        TextWriter writer = new StreamWriter(path, false);
        serializer.Serialize(writer, data.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)).ToList());
        writer.Close();
    }

    // Metoda pobiera z path dane i tworzy liste obiektów typu Achievement ! BEZ ACHIEVEMENT INFO !
    public List<Achievement> TestLoadAchievementList()
    {
        List<Achievement> testStorage = new List<Achievement>();

        var stream = File.Open(path, FileMode.Open);

        XmlSerializer serializer = new XmlSerializer(typeof(List<KeyValuePair<string, string>>));
        data = new Dictionary<string, string>();
        foreach (var p in (List<KeyValuePair<string, string>>)serializer.Deserialize(stream))
            data.Add(p.Key, p.Value);

        stream.Close();

        foreach (var item in data)
        {
            Achievement newAchievement = new Achievement();
        
            newAchievement.initializeFromSave(item.Value);

            testStorage.Add(newAchievement);
        }

        return testStorage;
    }

	public void SaveAchievements(){
		
	}
}