# Global Mixer Slider

`Assets/2DGamekit/Scripts/Audio/MixerSliderLink.cs`, is responsible for setting global volume levels. We need to replace the Unity audio with Wwise. RTPCs are ideal for handling this middleware side.

## Implementation

First, we'll replace the Unity `AudioMixer` with `AK.Wwise.RTPC`:

```cs
    public AK.Wwise.RTPC rtpcParameter;
```

Unity used attenuation values in dB, but RTPCs in Wwise default to a range of 0-100. You *can* change the RTPCs in Wwise to mimic the original Unity implementation, but Wwise is my Single Source Of Truth for audio in this project. I changed the member variable names to something more generalized, so if you want to stay more faithful to the Unity implementation you'll need to keep this in mind:

```cs
        public float maxValue = 100.0f;
        public float minValue = 0.0f;
```

The internal logic is the same for the rest of the class: `Slider` should fetch the current internal mix value it represents on `Awake()` and set the value on `SliderValueChange()`.

### `Awake()`

```cs
        void Awake()
        {
            m_Slider = GetComponent<Slider>();

            // Get current RTPC value
            float currentValue = rtpcParameter.GetGlobalValue();
            
            // Normalize to 0-1 for slider
            m_Slider.value = (currentValue - minValue) / (maxValue - minValue);

            m_Slider.onValueChanged.AddListener(SliderValueChange);
        }
```

### `SliderChangeValue(...)`

```cs
        void SliderValueChange(float normalizedValue)
        {
            // Convert slider value (0-1) to RTPC range
            float rtpcValue = minValue + normalizedValue * (maxValue - minValue);
            
            // Set global RTPC value
            rtpcParameter.SetGlobalValue(rtpcValue);
        }
```