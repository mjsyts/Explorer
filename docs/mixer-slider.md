# Global Mixer Slider

`Assets/2DGamekit/Scripts/Audio/MixerSliderLink.cs`, is responsible for setting global volume levels. We need to replace the Unity audio with Wwise. RTPCs are ideal for handling this middleware side.

## Implementation

We'll add an RTPC member variable to the class:

```cs
    public AK.Wwise.RTPC rtpcParameter;
```