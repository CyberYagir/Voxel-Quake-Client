using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Content.Scripts.Menu.UI
{
    public class UIOptionsSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text value;
        [SerializeField] private string format = "F0";

        public void Init(float value, int min, int max, Action<float> onValueChanged)
        {
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            UpdateValue();

            slider.onValueChanged.AddListener(delegate
            {
                onValueChanged?.Invoke(slider.value); 
                UpdateValue();
            });
        }


        public void UpdateValue()
        {
            value.text = slider.value.ToString(format);
        }
    }
}
