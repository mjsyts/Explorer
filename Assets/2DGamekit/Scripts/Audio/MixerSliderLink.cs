using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gamekit2D
{
    [RequireComponent(typeof(Slider))]
    public class WwiseMixerSliderLink : MonoBehaviour
    {
        public AK.Wwise.RTPC rtpcParameter;

        public float maxValue = 100.0f;
        public float minValue = 0.0f;

        protected Slider m_Slider;

        void Awake()
        {
            m_Slider = GetComponent<Slider>();

            // Get current RTPC value
            float currentValue = rtpcParameter.GetGlobalValue();
            
            // Normalize to 0-1 for slider
            m_Slider.value = (currentValue - minValue) / (maxValue - minValue);

            m_Slider.onValueChanged.AddListener(SliderValueChange);
        }

        void SliderValueChange(float normalizedValue)
        {
            // Convert slider value (0-1) to RTPC range
            float rtpcValue = minValue + normalizedValue * (maxValue - minValue);
            
            // Set global RTPC value
            rtpcParameter.SetGlobalValue(rtpcValue);
        }
    }
}