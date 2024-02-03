using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DramaMask.UI;

public class FillableMeterUI : MonoBehaviour
{
    protected string Name = "FillableBar";
    private Image _image;
    public List<Image> RelatedVisuals;

    private bool _visible = true;
    public bool Visible
    {
        get => _visible;
        set
        {
            if (value == _visible) return;

            _visible = value;
            RelatedVisuals.ForEach(i => i.enabled = value);
        }
    }

    protected void Awake() => _image = GetComponent<Image>();
    public void UpdatePercentage(float percent)
    {
        if (_image.fillAmount == percent) return;

        _image.fillAmount = percent;
    }
}