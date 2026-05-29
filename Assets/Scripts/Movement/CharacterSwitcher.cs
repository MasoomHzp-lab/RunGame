using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject maleVisual;
    [SerializeField] private GameObject femaleVisual;
    [SerializeField] private PlayerAnimationController animationController;

    private bool isFemale;

    private void Awake()
    {
        ApplyCharacter(false);
    }

    public void SelectMale()
    {
        ApplyCharacter(false);
    }

    public void SelectFemale()
    {
        ApplyCharacter(true);
    }

    public void ToggleCharacter()
    {
        ApplyCharacter(!isFemale);
    }

    private void ApplyCharacter(bool female)
    {
        isFemale = female;

        maleVisual.SetActive(!isFemale);
        femaleVisual.SetActive(isFemale);

        GameObject activeVisual = isFemale ? femaleVisual : maleVisual;
        Animator activeAnimator = activeVisual.GetComponentInChildren<Animator>(true);

        animationController.SetAnimator(activeAnimator);
    }
}