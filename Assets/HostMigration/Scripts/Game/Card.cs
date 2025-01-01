using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Card : MonoBehaviour
{
    private TMPro.TMP_Text _nameText;
    private TMPro.TMP_Text _descriptionText;
    private Image _cardBackground;
    private Button _button;

    private void Start()
    {
        // Get references
        _nameText = transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
        _descriptionText = transform.GetChild(1).GetComponent<TMPro.TMP_Text>();
        _cardBackground = transform.GetComponent<Image>();
        _button = transform.GetComponent<Button>();
    }

    //public void SetupCardVisual(string name, string desc, Color color)
    //{
    //    _nameText.text = name;
    //    _descriptionText.text = desc;
    //    _cardBackground.color = color;
    //}

    public void SetupCardVisual(IBooster card, Color color)
    {
        _nameText.text = card.Name;
        _descriptionText.text = card.Description;
        _cardBackground.color = color;
        _button.interactable = true;
    }

    public void DisableCardClick()
    {
        _button.interactable = false;
    }

    public void OnCardClicked()
    {
        // Debug log to show the card was clicked
        Debug.Log($"Card clicked: {_nameText.text}");
    }
}
