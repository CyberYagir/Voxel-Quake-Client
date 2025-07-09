using System;
using TMPro;
using UnityEngine;

namespace Content.Scripts.Menu.UI
{
    public class UIOptionsString : MonoBehaviour
    {
        [SerializeField] private TMP_InputField text;

        public void Init(string value, Action<string> onValueChanged)
        {
            text.text = value;

            text.onValueChanged.AddListener(delegate
            {
                onValueChanged?.Invoke(text.text);
            });
        }
    }
}
