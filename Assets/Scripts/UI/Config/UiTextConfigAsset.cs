using UnityEngine;

[CreateAssetMenu(menuName = "Game/UI/Text Config", fileName = "UiTextConfig")]
public class UiTextConfigAsset : ScriptableObject
{
    [SerializeField] private string battleInteractionLabel;
    [SerializeField] private string upgradeInteractionLabel;
    [SerializeField, TextArea(2, 4)] private string tutorialMessage;
    [SerializeField] private string battleStatusLockedTemplate;
    [SerializeField] private string battleStatusSelectedTemplate;
    [SerializeField] private string battleStatusSelectUnlockedHint;
    [SerializeField] private string unlockStatusAlreadyUnlockedTemplate;
    [SerializeField] private string unlockStatusUnlockedTemplate;
    [SerializeField] private string unlockStatusFailedTemplate;
    [SerializeField] private string levelButtonUnlockedSuffix;
    [SerializeField] private string levelButtonLockedSuffix;
    [SerializeField] private string unlockButtonUnlockedTemplate;
    [SerializeField] private string unlockButtonActionTemplate;
    [SerializeField] private string hudHealthTemplate;
    [SerializeField] private string hudSkillReadyText;
    [SerializeField] private string hudSkillCooldownTemplate;
    [SerializeField] private string interactionPromptIdle;
    [SerializeField] private string interactionPromptWithTargetTemplate;

    public string BattleInteractionLabel => battleInteractionLabel;
    public string UpgradeInteractionLabel => upgradeInteractionLabel;
    public string TutorialMessage => tutorialMessage;
    public string BattleStatusLockedTemplate => battleStatusLockedTemplate;
    public string BattleStatusSelectedTemplate => battleStatusSelectedTemplate;
    public string BattleStatusSelectUnlockedHint => battleStatusSelectUnlockedHint;
    public string UnlockStatusAlreadyUnlockedTemplate => unlockStatusAlreadyUnlockedTemplate;
    public string UnlockStatusUnlockedTemplate => unlockStatusUnlockedTemplate;
    public string UnlockStatusFailedTemplate => unlockStatusFailedTemplate;
    public string LevelButtonUnlockedSuffix => levelButtonUnlockedSuffix;
    public string LevelButtonLockedSuffix => levelButtonLockedSuffix;
    public string UnlockButtonUnlockedTemplate => unlockButtonUnlockedTemplate;
    public string UnlockButtonActionTemplate => unlockButtonActionTemplate;
    public string HudHealthTemplate => hudHealthTemplate;
    public string HudSkillReadyText => hudSkillReadyText;
    public string HudSkillCooldownTemplate => hudSkillCooldownTemplate;
    public string InteractionPromptIdle => interactionPromptIdle;
    public string InteractionPromptWithTargetTemplate => interactionPromptWithTargetTemplate;
}
