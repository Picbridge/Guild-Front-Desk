//using UnityEngine;
//using GFD.Adventurer;
//using System;
//using Random = UnityEngine.Random;

//public class AdventurerInstance
//{
//    public AdventurerData data;

//    public AdventurerInstance()
//    {
//        data = new AdventurerData();
//        GenerateRandomAdventurer();
//    }

//    private void GenerateRandomAdventurer()
//    {
//        data.adventurerName = "Adventurer_" + Random.Range(1000, 9999);

//        data.gender = Gender.Female;//GetRandom<Gender>();
//        data.race = GetRandom<Race>();
//        data.perspective = Perspective.Side;//GetRandom<Perspective>();
//        data.adventurerClass = GetRandom<Class>();
//        data.personality = GetRandom<Personality>();
//        data.rank = GetRandom<Rank>();
//        data.background = GetRandom<Background>();
//        data.mood = GetRandom<Mood>();
//        data.injuryStatus = InjuryStatus.Healthy;
//        data.currentState = State.Idle;

//        data.strength = Random.Range(1, 20);
//        data.agility = Random.Range(1, 20);
//        data.intelligence = Random.Range(1, 20);
//        data.endurance = Random.Range(1, 20);

//        data.isSponsored = false;
//        data.questCount = 0;
//        data.currQuestID = -1;

//        // assign asset names based on class/race/gender
//        AssignVisuals();
//    }

//    private void AssignVisuals()
//    {
//        // Set base path-friendly names
//        data.bodySprite = GetRandomSpriteName("CharacterParts/" + data.perspective + "/Body/" + data.gender);
//        data.headSprite = GetRandomSpriteName("CharacterParts/" + data.perspective + "/Head");
//        data.hairSprite = GetRandomSpriteName("CharacterParts/" + data.perspective + "/Hair");
//        data.eyesSprite = GetRandomSpriteName("CharacterParts/" + data.perspective + "/Eyes");
//        data.mouthSprite = GetRandomSpriteName("CharacterParts/" + data.perspective + "/Mouth");

//        data.skinColor = GetSkinColorForRace(data.race);
//        data.hairColor = GetRandomHairColor();
//    }

//    private string GetRandomSpriteName(string resourcePath)
//    {
//        Sprite[] sprites = Resources.LoadAll<Sprite>(resourcePath);
//        if (sprites.Length == 0)
//        {
//            Debug.LogError($"No sprites found at path: {resourcePath}");
//            return string.Empty;
//        }
//        return sprites[Random.Range(0, sprites.Length)].name;
//    }


//    private Color GetSkinColorForRace(Race race)
//    {
//        switch (race)
//        {
//            case Race.Elf: return new Color(0.9f, 0.85f, 0.7f);
//            case Race.Orc: return new Color(0.4f, 0.6f, 0.3f);
//            case Race.Demon: return new Color(0.6f, 0.3f, 0.3f);
//            default: return new Color(1f, 0.9f, 0.8f);
//        }
//    }

//    private Color GetRandomHairColor()
//    {
//        Color[] palette = {
//            new Color(0.1f, 0.1f, 0.1f), // black
//            new Color(0.6f, 0.3f, 0.1f), // brown
//            new Color(0.9f, 0.9f, 0.2f), // blonde
//            new Color(0.4f, 0.7f, 0.1f)  // green
//        };
//        return palette[Random.Range(0, palette.Length)];
//    }

//    private T GetRandom<T>()
//    {
//        var values = System.Enum.GetValues(typeof(T));
//        return (T)values.GetValue(Random.Range(0, values.Length));
//    }

//    public string GetPrompt()
//    {
//        return $"Adventurer {data.adventurerName} is a {data.gender} {data.race} " +
//            $"{data.adventurerClass} with a {data.personality} personality. " +
//               $"Their rank is {data.rank} and they have a {data.background} background. " +
//               $"They feel {data.mood}.";
//    }
//}
