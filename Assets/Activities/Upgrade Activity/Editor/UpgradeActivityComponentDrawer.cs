#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class UpgradeActivityComponentDrawer : ActivityComponentDrawer<UpgradeActivityComponent>
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();

        public override List<CardDrawerDisplayItem> GetDisplayItems(ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            if (component is UpgradeActivityComponent upgradeComponent && upgradeComponent.UpgradeInstances != null && upgradeComponent.UpgradeInstances.Count > 0)
            {
                return new List<CardDrawerDisplayItem>
                {
                    new UpgradesGridDisplayItem(upgradeComponent.UpgradeInstances, currentRun)
                };
            }
            return null;
        }

        protected override void DrawTypedUnitCard(UnitInstance unit, ActivityInstance activity, UpgradeActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            _cardDrawer.Draw(
                unit,
                () =>
                {
                    // Remove button (debug mode)
                    if (currentRun?.Settings?.debugMode ?? false)
                    {
                        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                        if (GUILayout.Button("Remove", GUILayout.Height(18), GUILayout.Width(90)))
                        {
                            activity.RemoveUnit(unit);
                            UnityEditor.AssetDatabase.SaveAssets();
                            onChanged?.Invoke();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                },
                currentRun?.Settings);
        }
    }

    public class UpgradesGridDisplayItem : CardDrawerDisplayItem
    {
        public UpgradesGridDisplayItem(List<UpgradeInstance> upgradeInstances, NetworkRun currentRun)
        {
            condition = () => true;
            color = new Color(0.9f, 0.95f, 1f);
            drawAction = () =>
            {
                EditorGUILayout.BeginHorizontal();
                
                foreach (var upgradeInstance in upgradeInstances)
                {
                    if (upgradeInstance?.Template != null)
                    {
                        DrawUpgradeItem(upgradeInstance, currentRun);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            };
        }
        
        private void DrawUpgradeItem(UpgradeInstance upgradeInstance, NetworkRun currentRun)
        {
            var upgrade = upgradeInstance.Template;
            
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            
            // Get icon based on upgrade type
            Texture icon = null;
            if (upgrade is ResourceCollectionUpgrade resourceUpgrade && resourceUpgrade.Resource?.Sprite?.texture != null)
            {
                icon = resourceUpgrade.Resource.Sprite.texture;
            }
            else if (upgrade is SkillSpeedUpgrade skillSpeedUpgrade && skillSpeedUpgrade.Skill?.Sprite?.texture != null)
            {
                icon = skillSpeedUpgrade.Skill.Sprite.texture;
            }
            
            // Build subtitle with upgrade effect
            string subtitle = upgrade.Description;
            if (upgrade is ResourceCollectionUpgrade resUpgrade)
            {
                var resourceName = resUpgrade.Resource?.DisplayName ?? "Unknown";
                subtitle = $"+{resUpgrade.IncreaseAmount} {resourceName} collection";
            }
            else if (upgrade is SkillSpeedUpgrade skillUpgrade)
            {
                var skillName = skillUpgrade.Skill?.name ?? "Unknown";
                var bonusPercent = skillUpgrade.SpeedBonusMultiplier * 100f;
                subtitle = $"+{bonusPercent:F0}% {skillName} speed";
            }
            
            // Build display items
            var displayItems = new List<CardDrawerDisplayItem>();
            
            if (!upgradeInstance.IsPurchased)
            {
                // Purchase button with price
                displayItems.Add(new CardDrawerDisplayItem
                {
                    condition = () => true,
                    color = Color.white,
                    drawAction = () =>
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        // Check if player can afford (logic now in UpgradeInstance)
                        bool canAfford = upgradeInstance.CanAfford(currentRun?.Inventory);
                        
                        // Build button content with icon and text
                        string priceText = "Purchase";
                        Texture costIcon = null;
                        
                        if (upgradeInstance.Price != null && upgradeInstance.Price.Count > 0)
                        {
                            var priceParts = new List<string>();
                            foreach (var cost in upgradeInstance.Price)
                            {
                                if (cost?.Item != null)
                                {
                                    priceParts.Add($"{cost.Amount}x");
                                    if (costIcon == null && cost.Item.Sprite?.texture != null)
                                    {
                                        costIcon = cost.Item.Sprite.texture;
                                    }
                                }
                            }
                            if (priceParts.Count > 0)
                            {
                                priceText = string.Join(" ", priceParts);
                            }
                        }
                        
                        GUI.enabled = canAfford;
                        GUI.backgroundColor = canAfford ? new Color(0.6f, 0.8f, 1f) : Color.gray;
                        var buttonContent = new GUIContent(priceText, costIcon);
                        if (GUILayout.Button(buttonContent, GUILayout.Height(24), GUILayout.Width(80)))
                        {
                            upgradeInstance.Purchase(currentRun, null);
                        }
                        
                        GUI.backgroundColor = Color.white;
                        GUI.enabled = true;
                        
                        EditorGUILayout.EndHorizontal();
                    }
                });
            }
            else
            {
                // Purchased status
                displayItems.Add(new CardDrawerDisplayItem
                {
                    condition = () => true,
                    color = Color.green,
                    drawAction = () =>
                    {
                        var purchasedStyle = new GUIStyle(EditorStyles.label)
                        {
                            fontSize = 10,
                            normal = { textColor = Color.green }
                        };
                        EditorGUILayout.LabelField("✓ Purchased", purchasedStyle);
                    }
                });
            }
            
            Color backgroundColor = upgradeInstance.IsPurchased ? new Color(0.8f, 0.9f, 0.8f) : new Color(0.92f, 0.96f, 1f);
            
            ItemDrawer.DrawItem(
                icon: icon,
                displayName: upgrade.UpgradeName,
                subtitle: subtitle,
                backgroundColor: backgroundColor,
                shortcuts: null,
                menuItems: null,
                displayItems: displayItems
            );
            
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
