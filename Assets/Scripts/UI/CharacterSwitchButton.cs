using UnityEngine;
using UnityEngine.UI;

public class CharacterSwitchButton : MonoBehaviour
{
    [Header("Character Switcher")]
    [SerializeField] private CharacterSwitcher characterSwitcher;

    [Header("Button UI")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite maleIcon;
    [SerializeField] private Sprite femaleIcon;

    private bool isFemale;

    private void Start()
    {
        UpdateIcon();
    }

    public void OnClick()
    {
        isFemale = !isFemale;

        if (isFemale)
            characterSwitcher.SelectFemale();
        else
            characterSwitcher.SelectMale();

        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (buttonImage == null)
            return;

        buttonImage.sprite = isFemale ? femaleIcon : maleIcon;
    }
}