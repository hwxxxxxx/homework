using UnityEngine;

public class BaseSceneBinding : MonoBehaviour
{
    [SerializeField] private BaseSceneUIController baseSceneUiController;

    public BaseSceneUIController BaseSceneUiController => baseSceneUiController;
}
