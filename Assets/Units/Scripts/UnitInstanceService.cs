using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class UnitInstanceService : GURUService
{
    [Header("References")]
    [SerializeField] private LocalData localData;
    public LocalData LocalData => localData;

    [Header("Collections")]
    [SerializeField] private List<UnitTemplate> initialUnits;
    [SerializeField] private UnitSpecies playerUnit;
    [SerializeField] private List<UnitSpecies> speciesCollection;
    [SerializeField] private List<Job> jobCollection;
    [SerializeField] private List<Skill> skillCollection;
    [SerializeField] private List<ColorPalette> colorPalettes;
    [SerializeField] private List<UnitInteraction> interactionCollection;

    [SerializeField] private List<UnitInstance> units;

    public List<UnitTemplate> InitialUnits => initialUnits;

    public List<UnitInstance> Instances => units;

    private const string UNIT_KEY = "units";

    public event UnityAction<float> OnXpChanged;

    public ColorPalette GetColorPaletteByElement(Element element)
    {
        return colorPalettes.Find(p => p?.Element == element);
    }

    protected override void OnInitialize()
    {
        Debug.Log("Initializing UnitInstanceService");
        units = new List<UnitInstance>();

        if (localData.JsonFile.ContainsKey(UNIT_KEY))
        {
            var unitsJson = localData.JsonFile[UNIT_KEY].ToString();

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new UnitSpeciesConverter(speciesCollection) }
            };

            var unitsData = JsonConvert.DeserializeObject<List<UnitData>>(unitsJson, settings);

            foreach (var unitData in unitsData)
            {
                // Species is now auto-resolved via UnitSpeciesConverter
                if (unitData.species == null)
                {
                    Debug.LogError($"Species could not be resolved for unit '{unitData.name}'.");
                    continue;
                }

                var matchingJob = jobCollection.Find(job => job.Id == unitData.job?.Id);

                Element element;
                if (!Enum.TryParse(unitData.element.ToString(), true, out element))
                {
                    element = Element.NONE;
                }

                // Assign runtime-only fields
                unitData.job = matchingJob;
                unitData.element = element;
                unitData.colorPalette = GetColorPaletteByElement(element);

                var unitInstance = new UnitInstance(unitData);


                var skills = new List<SkillInstance>();
                var skillSaveDict = unitData.skills?.ToDictionary(s => s.id, s => s) ?? new Dictionary<string, UnitData.SkillData>();

                foreach (var skill in skillCollection)
                {
                    float xp = 0f;
                    if (skillSaveDict.TryGetValue(skill.Id, out var skillSave))
                    {
                        xp = skillSave.xp;
                    }

                    SkillInstance skillInstance;
                    if (skill.Id.ToLower() == "dance")
                    {
                        skillInstance = new DanceSkillInstance(unitInstance, skill, xp);
                    }
                    else
                    {
                        skillInstance = new SkillInstance(unitInstance, skill, xp);
                    }
                    skills.Add(skillInstance);
                }

                // Check for skills in JSON that are not in the collection
                var loadedSkillIds = unitData.skills?.Select(s => s.id).Where(id => !string.IsNullOrEmpty(id)).ToList() ?? new List<string>();
                var collectionSkillIds = skillCollection.Select(s => s.Id).ToList();
                var missingSkills = loadedSkillIds.Except(collectionSkillIds);
                foreach (var missing in missingSkills)
                {
                    Debug.LogError($"Skill '{missing}' not found in skillCollection for unit '{unitData.name}'.");
                }

                unitInstance.InitializeSkills(skills);

                RegisterUnit(unitInstance, false);

                // assign original template to compare momeents
                var template = initialUnits.Find(unit => unit.Data.id == unitInstance.Id);
                if (template != null) unitInstance.SetTemplate(template);
            }
        }


        // adding initial units not already
        foreach (var initialUnit in initialUnits)
        {
            if (!units.Any(unit => unit.Id == initialUnit.Data.id))
            {
                var newUnitInstance = new UnitInstance(initialUnit.Data);
                RegisterUnit(newUnitInstance, false);
                newUnitInstance.SetTemplate(initialUnit);

                // Debug.Log($"Registered initial unit '{newUnitInstance.DisplayName}' with ID '{newUnitInstance.Id}'");
            }
        }

        // Rebuild friends relationships from saved data
        if (localData.JsonFile.ContainsKey(UNIT_KEY))
        {
            var unitsJson = localData.JsonFile[UNIT_KEY].ToString();

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new UnitSpeciesConverter(speciesCollection) }
            };

            var unitsData = JsonConvert.DeserializeObject<List<UnitData>>(unitsJson, settings);

            foreach (var unitData in unitsData)
            {
                var unit = units.Find(u => u.Id == unitData.id);
                if (unit == null) continue;

                foreach (var friendId in unitData.friends ?? new List<string>())
                {
                    if (string.IsNullOrEmpty(friendId)) continue;

                    var friendUnit = units.Find(u => u.Id == friendId);
                    if (friendUnit != null && !unit.Friends.Contains(friendUnit.Id))
                    {
                        unit.Friends.Add(friendUnit.Id);
                    }
                }
            }
        }

        // Maintain order by ID
        units = units.OrderBy(u => u.Id).ToList();

        // Update units with new moments from original assets
        foreach (var unit in units)
        {
            if (unit.Template != null)
            {
                foreach (var originalMoment in unit.Template.Moments)
                {
                    if (!unit.Moments.Any(m => m.Interaction == originalMoment.Interaction))
                    {
                        var newMoment = new UnitMoment(originalMoment.Interaction, false);
                        newMoment.OnMomentComplete += () => SaveData();
                        unit.Moments.Add(newMoment);
                    }
                }
            }
        }

        // Auto-fill missing skills from skill collection
        foreach (var unit in units)
        {
            var skillsList = unit.Skills != null ? unit.Skills.Values.ToList() : new List<SkillInstance>();
            var existingSkillIds = skillsList.Select(s => s.Skill.Id).ToHashSet();
            var missingSkills = skillCollection.Where(skill => !existingSkillIds.Contains(skill.Id)).ToList();

            if (missingSkills.Count > 0)
            {
                foreach (var skill in missingSkills)
                {
                    SkillInstance skillInstance;
                    if (skill.Id.ToLower() == "dance")
                    {
                        skillInstance = new DanceSkillInstance(unit, skill, 0);
                    }
                    else
                    {
                        skillInstance = new SkillInstance(unit, skill, 0);
                    }
                    skillsList.Add(skillInstance);
                }
                unit.InitializeSkills(skillsList);
            }
        }

        SaveData();
    }

    public delegate bool UnitQuery(UnitSpecies unit);

    public UnitInstance CreateUnit(UnitQuery query = null, bool isEnemy = false)
    {
        var (newUnit, newElement) = GenerateNewUnit(query);

        var matchingColorPalette = GetColorPaletteByElement(newElement);

        var displayName = GenerateDisplayName(newUnit.Id);
        var data = new UnitData
        {
            name = newUnit.Id,
            displayName = displayName,
            element = newElement,
            job = null,
            species = newUnit,
            colorPalette = matchingColorPalette,
            isEnemy = isEnemy
        };

        var newUnitInstance = new UnitInstance(data);

        var skills = new List<SkillInstance>();

        foreach (var skill in skillCollection)
        {
            SkillInstance skillInstance;
            if (skill.Id.ToLower() == "dance")
            {
                skillInstance = new DanceSkillInstance(newUnitInstance, skill, 0);
            }
            else
            {
                skillInstance = new SkillInstance(newUnitInstance, skill, 0);
            }
            skills.Add(skillInstance);
        }

        newUnitInstance.InitializeSkills(skills);
        RegisterUnit(newUnitInstance);
        return newUnitInstance;
    }

    public UnitInstance CreateUnitFromTemplate(UnitTemplate template)
    {
        if (template == null || template.Data == null)
        {
            Debug.LogError("Cannot create unit from null template or template with null data");
            return null;
        }

        var data = template.Data;
        // Keep the template's ID (or generate one if empty)
        var newUnitInstance = new UnitInstance(data);

        // Initialize skills
        var skills = new List<SkillInstance>();
        foreach (var skill in skillCollection)
        {
            SkillInstance skillInstance;
            if (skill.Id.ToLower() == "dance")
            {
                skillInstance = new DanceSkillInstance(newUnitInstance, skill, 0);
            }
            else
            {
                skillInstance = new SkillInstance(newUnitInstance, skill, 0);
            }
            skills.Add(skillInstance);
        }

        newUnitInstance.InitializeSkills(skills);

        // Set the template reference
        newUnitInstance.SetTemplate(template);

        // Add template moments if any
        if (template.Moments != null && template.Moments.Count > 0)
        {
            foreach (var moment in template.Moments)
            {
                newUnitInstance.Moments.Add(moment);
            }
        }

        RegisterUnit(newUnitInstance);
        return newUnitInstance;
    }

    private string GenerateDisplayName(string baseName)
    {
        var titles = new[] { "Mysterious", "Party", "DJ", "Crazy", "Wild", "Cool", "Happy", "Sad", "Fun", "Silly" };
        var names = new[] { "Sal", "Dan", "Cindy", "Bob", "Alice", "Tom", "Jerry", "Mickey", "Luna", "Rex", "Bella", "Max", "Lily", "Charlie", "Daisy" };
        var title = titles[UnityEngine.Random.Range(0, titles.Length)];
        var name = names[UnityEngine.Random.Range(0, names.Length)];
        return $"{title} {name}";
    }

    public UnitInstance CopyUnit(UnitInstance instance, bool saveData = true, bool register = true)
    {
        var data = instance.Data;
        data.id = null; // Generate new Id for copy
        var copiedUnit = new UnitInstance(data);

        // Copy skills with their XP values from original unit
        var skills = new List<SkillInstance>();
        if (instance.Skills != null && instance.Skills.Count > 0)
        {
            foreach (var skillEntry in instance.Skills)
            {
                var skill = skillEntry.Key;
                var originalSkillInstance = skillEntry.Value;
                SkillInstance skillInstance;
                if (skill.Id.ToLower() == "dance")
                {
                    skillInstance = new DanceSkillInstance(copiedUnit, skill, originalSkillInstance.XP);
                }
                else
                {
                    skillInstance = new SkillInstance(copiedUnit, skill, originalSkillInstance.XP);
                }
                skills.Add(skillInstance);
            }
        }

        copiedUnit.InitializeSkills(skills);

        if (register)
        {
            return RegisterUnit(copiedUnit, saveData);
        }

        return copiedUnit;
    }

    public (UnitSpecies unit, Element element) GenerateNewUnit(UnitQuery predicate)
    {
        // Skip the player's own unit
        var availableUnits = speciesCollection
            .Where(unit => unit != playerUnit)
            .ToList();

        if (predicate != null)
        {
            availableUnits = availableUnits.Where(unit => predicate(unit)).ToList();
        }

        // Safety check: if no available units, fallback to player unit
        if (availableUnits.Count == 0)
            availableUnits.Add(playerUnit);

        // Step 1: pick a unit the instance hasn't seen yet
        var unseenUnits = availableUnits
            .Where(u => !Instances.Any(ui => ui.Species == u))
            .ToList();

        if (unseenUnits.Count > 0)
        {
            var chosenUnit = unseenUnits[UnityEngine.Random.Range(0, unseenUnits.Count)];
            TryPickUnseenElementForUnit(chosenUnit, out Element chosenElement);
            return (chosenUnit, chosenElement);
        }

        // Step 2: all units have been seen → pick a unit that still has unseen elements
        var availablePairs = new List<(UnitSpecies, Element)>();
        foreach (var u in availableUnits)
        {
            if (TryPickUnseenElementForUnit(u, out Element e))
                availablePairs.Add((u, e));
        }

        if (availablePairs.Count > 0)
            return availablePairs[UnityEngine.Random.Range(0, availablePairs.Count)];

        // Step 3: fallback → any unit and any element
        var fallbackUnit = availableUnits[UnityEngine.Random.Range(0, availableUnits.Count)];
        var allElements = (Element[])System.Enum.GetValues(typeof(Element));
        var fallbackElement = allElements[UnityEngine.Random.Range(0, allElements.Length)];

        return (fallbackUnit, fallbackElement);
    }

    private bool TryPickUnseenElementForUnit(UnitSpecies unit, out Element element)
    {
        var usedElements = Instances
            .Where(ui => ui.Species == unit)
            .Select(ui => ui.Element)
            .ToHashSet();

        var allElements = (Element[])System.Enum.GetValues(typeof(Element));
        var unseenElements = allElements
            .Where(e => !usedElements.Contains(e) && e != Element.NONE)
            .ToList();

        if (unseenElements.Count > 0)
        {
            element = unseenElements[UnityEngine.Random.Range(0, unseenElements.Count)];
            return true;
        }

        // fallback: all elements used → pick any
        element = allElements[UnityEngine.Random.Range(0, allElements.Length)];
        return false;
    }

    public bool TryGetFriend(UnitInstance unit, out UnitInstance friend, List<UnitInstance> blacklist)
    {
        friend = null;

        var introduceNewFriend = unit.Friends.Count switch
        {
            0 => 1f,
            1 => 0.66f,
            2 => 0.33f,
            _ => 0f,
        };

        if (unit.Friends.Count < 3 && UnityEngine.Random.value < introduceNewFriend)
        {
            friend = CreateNewFriend(unit);
        }
        else
        {
            var availableFriends = unit.Friends.Where(friendId => !blacklist.Any(unit => unit.Id == friendId)).ToList();
            if (availableFriends.Count > 0)
            {
                var targetFriend = availableFriends[UnityEngine.Random.Range(0, availableFriends.Count)];
                friend = units.Find(unit => unit.Id == targetFriend);
            }
        }

        return friend != null;
    }

    public UnitInstance CreateNewFriend(UnitInstance unit)
    {
        var friend = CreateUnit();
        unit.Friends.Add(friend.Id);
        friend.Friends.Add(unit.Id);
        return friend;
    }

    public UnitInstance RegisterUnit(UnitInstance unit, bool saveData = true)
    {
        // Check if an instance with the same Id already exists
        var existing = units.FirstOrDefault(u => u.Id == unit.Id);
        if (existing != null)
        {
            return existing; // Return the already-registered instance
        }

        float sum = 0f;
        float lastSaveTime = 0f;

        unit.OnXpChanged += value =>
        {
            sum += value;

            if (sum > 10f || Time.time - lastSaveTime > 30f)
            {
                sum = 0;
                lastSaveTime = Time.time;
                SaveData();
            }

            OnXpChanged?.Invoke(value);
        };

        units.Add(unit);

        if (saveData) SaveData();

        return unit;
    }

    public void DeleteUnit(UnitInstance unit)
    {
        units.Remove(unit);

        // Remove this unit from all other units' friend lists
        foreach (var otherUnit in units)
        {
            otherUnit.Friends.Remove(unit.Id);
        }

        SaveData();
    }

    public void SaveData()
    {
        if (localData == null)
        {
            Debug.LogError("LocalData is not assigned in UnitInstanceService");
            return;
        }
        if (localData.JsonFile == null) localData.Initialize();

        // Filter out enemies - they should not be persisted
        var unitsData = units.Where(unit => !unit.Data.isEnemy).Select(unit => unit.Data).ToList();

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new List<JsonConverter> { new UnitSpeciesConverter(speciesCollection) }
        };

        var json = JsonConvert.SerializeObject(unitsData, settings);
        localData.SaveData(UNIT_KEY, JToken.Parse(json));
    }
}
