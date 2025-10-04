using UnityEngine;
using UnityEngine.UI;

public class SelectableControllerUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button moveButton;
    public Button removeButton;
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Button revertButton;
    public Button applyButton;

    private SelectableItemController currentTarget;

    private void Start()
    {
        gameObject.SetActive(false); // hidden by default

        moveButton.onClick.AddListener(() => currentTarget?.ToggleMove());
        removeButton.onClick.AddListener(() => { currentTarget?.Remove(); Hide(); });
        rotateLeftButton.onClick.AddListener(() => currentTarget?.RotateLeft());
        rotateRightButton.onClick.AddListener(() => currentTarget?.RotateRight());
        revertButton.onClick.AddListener(() => currentTarget?.Revert());
        applyButton.onClick.AddListener(() => { currentTarget?.Apply(); Hide(); });
    }

    public void Show(SelectableItemController target)
    {
        currentTarget = target;
        gameObject.SetActive(true);
        target.Select();
    }

    public void Hide()
    {
        if (currentTarget != null)
        {
            currentTarget.Deselect();
            currentTarget = null;
        }
        gameObject.SetActive(false);
    }
}
