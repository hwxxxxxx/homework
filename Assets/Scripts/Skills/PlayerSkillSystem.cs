using UnityEngine;

public class PlayerSkillSystem : MonoBehaviour
{
    [SerializeField] private SkillBase equippedSkill;

    public SkillBase EquippedSkill => equippedSkill;

    public void TryHandleInput(GameInput gameInput)
    {
        if (gameInput == null || equippedSkill == null)
        {
            return;
        }

        if (!gameInput.IsSkillPressed())
        {
            return;
        }

        equippedSkill.TryActivate(gameObject);
    }
}
