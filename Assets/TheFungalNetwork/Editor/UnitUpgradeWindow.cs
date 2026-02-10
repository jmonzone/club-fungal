#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class UnitUpgradeWindow : EditorWindow
    {
        private UnitInstance unit;
        private NetworkRun networkRun;
        private Vector2 scrollPosition;
        private Skill selectedSkill;
        private int fishCostPerLevel = 5; // Fish cost to boost a skill by one level

        public static void ShowWindow(UnitInstance unit, NetworkRun networkRun)
        {
            var window = GetWindow<UnitUpgradeWindow>("Unit Upgrade");
            window.unit = unit;
            window.networkRun = networkRun;
            window.minSize = new Vector2(450, 500);
            window.Show();
        }

        private void OnGUI()
        {
            if (unit == null)
            {
                EditorGUILayout.HelpBox("No unit instance selected", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);
            DrawSkills();
            EditorGUILayout.Space(10);
            DrawUpgradeSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            // Background box
            var headerRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = new Color(0.2f, 0.3f, 0.4f);

            // Unit name with color
            var nameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };
            EditorGUILayout.LabelField(unit.DisplayName ?? "Unknown Unit", nameStyle, GUILayout.Height(35));

            // Species/Type with icon
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (unit.Species != null)
            {
                var speciesStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.7f, 0.85f, 1f) }
                };

                // Show sprite if available
                if (unit.Species.Sprite != null)
                {
                    var spriteRect = GUILayoutUtility.GetRect(40, 40, GUILayout.Width(40));
                    GUI.DrawTexture(spriteRect, unit.Species.Sprite.texture, ScaleMode.ScaleToFit);
                }

                EditorGUILayout.LabelField($"Species: {unit.Species.name}", speciesStyle, GUILayout.Height(20));

                if (unit.Species.Type != null)
                {
                    var typeStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = new Color(0.6f, 0.75f, 0.9f) }
                    };
                    EditorGUILayout.LabelField($"Type: {unit.Species.Type.name}", typeStyle, GUILayout.Height(16));
                }
            }
            else
            {
                EditorGUILayout.LabelField("Species: Unknown", GUILayout.Height(20));
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Job
            if (unit.Job != null)
            {
                var jobStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(1f, 0.9f, 0.6f) }
                };
                EditorGUILayout.LabelField($"Job: {unit.Job.name}", jobStyle, GUILayout.Height(16));
            }

            // Energy bar
            if (networkRun?.Settings?.useEnergySystem ?? true)
            {
                EditorGUILayout.Space(5);
                var energyRect = EditorGUILayout.GetControlRect(false, 20);
                var energyProgress = unit.Energy / 100f;

                // Energy bar background
                EditorGUI.DrawRect(energyRect, new Color(0.2f, 0.2f, 0.2f));

                // Energy bar fill
                var fillRect = new Rect(energyRect.x, energyRect.y, energyRect.width * energyProgress, energyRect.height);
                var energyColor = energyProgress > 0.5f ? new Color(0.3f, 1f, 0.3f) :
                                  energyProgress > 0.25f ? new Color(1f, 0.8f, 0.3f) :
                                  new Color(1f, 0.3f, 0.3f);
                EditorGUI.DrawRect(fillRect, energyColor);

                // Energy text
                var energyTextStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                    normal = { textColor = Color.white }
                };
                EditorGUI.LabelField(energyRect, $"Energy: {unit.Energy:F0}%", energyTextStyle);
            }

            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }

        private void DrawSkills()
        {
            var skillsHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                normal = { textColor = new Color(0.7f, 0.9f, 1f) }
            };
            EditorGUILayout.LabelField("Skills", skillsHeaderStyle);
            EditorGUILayout.Space(5);

            if (unit.Skills == null || unit.Skills.Count == 0)
            {
                EditorGUILayout.HelpBox("No skills found", MessageType.Info);
                return;
            }

            // Draw only enabled skills
            foreach (var skillKvp in unit.Skills)
            {
                var skill = skillKvp.Key;
                var skillInstance = skillKvp.Value;

                // Check if skill is enabled in settings
                bool isEnabled = networkRun?.Settings?.IsSkillEnabled(skill) ?? true;

                // Only draw if enabled
                if (isEnabled)
                {
                    DrawSkillCard(skill, skillInstance);
                    EditorGUILayout.Space(3);
                }
            }
        }

        private void DrawSkillCard(Skill skill, SkillInstance skillInstance)
        {
            var cardRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Skill name and level
            EditorGUILayout.BeginHorizontal();

            // Skill icon
            if (skill.Sprite != null)
            {
                var iconRect = GUILayoutUtility.GetRect(30, 30, GUILayout.Width(30));
                GUI.DrawTexture(iconRect, skill.Sprite.texture, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.BeginVertical();

            // Skill name
            var skillNameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.9f, 0.9f, 1f) }
            };
            EditorGUILayout.LabelField($"{skill.Id}", skillNameStyle);

            // Level
            var levelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.7f, 1f, 0.7f) }
            };
            EditorGUILayout.LabelField($"Level {skillInstance.Level}", levelStyle);

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Select button for upgrade
            GUI.backgroundColor = selectedSkill == skill ? new Color(0.5f, 1f, 0.5f) : new Color(0.7f, 0.85f, 1f);
            if (GUILayout.Button("Select", GUILayout.Width(70), GUILayout.Height(30)))
            {
                selectedSkill = skill;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // XP Progress bar
            var currentLevel = skillInstance.Level;
            var currentXP = skillInstance.XP;
            var xpForCurrentLevel = SkillInstance.GetXPFromLevel(currentLevel);
            var xpForNextLevel = SkillInstance.GetXPFromLevel(currentLevel + 1);
            var xpIntoLevel = currentXP - xpForCurrentLevel;
            var xpNeededForLevel = xpForNextLevel - xpForCurrentLevel;
            var progressValue = xpIntoLevel / xpNeededForLevel;

            var progressRect = EditorGUILayout.GetControlRect(false, 16);

            // Progress bar background
            EditorGUI.DrawRect(progressRect, new Color(0.2f, 0.2f, 0.25f));

            // Progress bar fill
            var fillRect = new Rect(progressRect.x, progressRect.y, progressRect.width * progressValue, progressRect.height);
            var progressColor = new Color(0.3f, 0.7f, 1f);
            EditorGUI.DrawRect(fillRect, progressColor);

            // XP text
            var xpTextStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                normal = { textColor = Color.white }
            };
            EditorGUI.LabelField(progressRect, $"XP: {xpIntoLevel:F0} / {xpNeededForLevel:F0}", xpTextStyle);

            EditorGUILayout.EndVertical();
        }

        private void DrawUpgradeSection()
        {
            var upgradeHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                normal = { textColor = new Color(1f, 0.9f, 0.6f) }
            };
            EditorGUILayout.LabelField("Skill Upgrades", upgradeHeaderStyle);
            EditorGUILayout.Space(5);

            if (networkRun == null || networkRun.Inventory == null)
            {
                EditorGUILayout.HelpBox("No network run available", MessageType.Warning);
                return;
            }

            var fishItem = networkRun.Settings?.fishItem;
            if (fishItem == null)
            {
                EditorGUILayout.HelpBox("No fish item configured in settings", MessageType.Warning);
                return;
            }

            var fishCount = networkRun.Inventory.GetItemCount(fishItem);

            // Global inventory display
            var inventoryRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = new Color(0.3f, 0.35f, 0.4f);

            var inventoryStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };
            EditorGUILayout.LabelField($"🐟 Global Fish: {fishCount}", inventoryStyle);

            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            // Upgrade options
            if (selectedSkill == null)
            {
                EditorGUILayout.HelpBox("Select a skill above to upgrade", MessageType.Info);
                return;
            }

            // Check if selected skill is enabled
            bool isSkillEnabled = networkRun?.Settings?.IsSkillEnabled(selectedSkill) ?? true;
            if (!isSkillEnabled)
            {
                EditorGUILayout.HelpBox($"Skill '{selectedSkill.Id}' is disabled in Network Run Settings", MessageType.Warning);
                selectedSkill = null; // Clear selection
                return;
            }

            if (!unit.Skills.TryGetValue(selectedSkill, out var selectedSkillInstance))
            {
                EditorGUILayout.HelpBox("Selected skill not found", MessageType.Error);
                return;
            }

            var upgradeBoxRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = new Color(0.25f, 0.3f, 0.35f);

            var selectedSkillStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.9f, 1f, 0.7f) }
            };
            EditorGUILayout.LabelField($"Upgrading: {selectedSkill.Id} (Lv.{selectedSkillInstance.Level})", selectedSkillStyle);

            EditorGUILayout.Space(5);

            // Upgrade options with different costs
            DrawUpgradeOption("Small Boost", 1, 100f, fishItem, fishCount);
            DrawUpgradeOption("Medium Boost", 3, 350f, fishItem, fishCount);
            DrawUpgradeOption("Large Boost", 5, 650f, fishItem, fishCount);
            DrawUpgradeOption("Huge Boost", 10, 1500f, fishItem, fishCount);

            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }

        private void DrawUpgradeOption(string label, int fishCost, float xpGain, ItemTemplate fishItem, int availableFish)
        {
            EditorGUILayout.BeginHorizontal();

            var canAfford = availableFish >= fishCost;

            // Info text
            var infoStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = canAfford ? new Color(0.85f, 0.95f, 1f) : new Color(0.6f, 0.6f, 0.6f) }
            };
            EditorGUILayout.LabelField($"{label} (+{xpGain:F0} XP)", infoStyle, GUILayout.Width(180));
            EditorGUILayout.LabelField($"Cost: {fishCost} 🐟", infoStyle, GUILayout.Width(100));

            GUI.enabled = canAfford;
            GUI.backgroundColor = canAfford ? new Color(0.5f, 1f, 0.5f) : new Color(0.3f, 0.3f, 0.3f);

            if (GUILayout.Button("Upgrade", GUILayout.Width(80), GUILayout.Height(22)))
            {
                ApplyUpgrade(fishItem, fishCost, xpGain);
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }

        private void ApplyUpgrade(ItemTemplate fishItem, int fishCost, float xpGain)
        {
            if (selectedSkill == null || networkRun == null) return;

            if (!unit.Skills.TryGetValue(selectedSkill, out var skillInstance)) return;

            // Remove fish from global inventory
            for (int i = 0; i < fishCost; i++)
            {
                networkRun.Inventory.RemoveItem(fishItem, 1);
            }

            // Add XP to skill
            skillInstance.IncreaseSkillXP(unit, xpGain);

            Debug.Log($"Upgraded {selectedSkill.Id} with {fishCost} fish, gained {xpGain} XP");

            UnityEditor.AssetDatabase.SaveAssets();
            Repaint();
        }
    }
}
#endif
