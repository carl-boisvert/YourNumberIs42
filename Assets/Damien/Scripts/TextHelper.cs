using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextHelper : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] private float _minFontSize;
    [SerializeField, Range(0f, 100f)] private float _maxFontSize;
    
    private TextMeshProUGUI _text = null;

    private void Awake() {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void SetToMin() {
        _text.fontSize = _minFontSize;
    }

    public void SetToMax() {
        _text.fontSize = _maxFontSize;
    }
}
