using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WwiseAudioPlayer : MonoBehaviour
{
    public AK.Wwise.Event wwiseEvent;
    public bool debug = false;

    [System.Serializable]
    public struct TileOverride
    {
        public TileBase tile;
        public AK.Wwise.Switch wwiseSwitch;
    }

    [Header("Tile to Wwise Switch Mappings")]
    public TileOverride[] overrides;

    protected Dictionary<TileBase, AK.Wwise.Switch> m_LookupOverride;

    private void Awake()
    {
        // Initialize dictionary
        m_LookupOverride = new Dictionary<TileBase, AK.Wwise.Switch>();

        for (int i = 0; i < overrides.Length; i++)
        {
            if (overrides[i].tile == null || overrides[i].wwiseSwitch == null)
                continue;

            m_LookupOverride[overrides[i].tile] = overrides[i].wwiseSwitch;
        }
    }

    /// <summary>
    /// Plays the Wwise event. If a TileBase is given, it will set the corresponding Wwise switch first.
    /// </summary>
    public void Play(TileBase surface = null)
    {
        // Set the Wwise switch if applicable
        if (surface != null && m_LookupOverride.TryGetValue(surface, out var switchToSet))
        {
            switchToSet.SetValue(gameObject);
        }

        // Debug logging
        if (debug)
        {
            Debug.Log($"[WwiseAudioPlayer] Posting event: {wwiseEvent.Name}");
            if (surface != null && m_LookupOverride.ContainsKey(surface))
                Debug.Log($"[WwiseAudioPlayer] Switch set for surface: {surface.name}");
        }

        // Post the Wwise event
        wwiseEvent?.Post(gameObject);
    }

    /// <summary>
    /// Stops the Wwise event if it is playing.
    /// </summary>
    public void Stop()
    {
        // Stop by posting the same event with a Stop action
        if (wwiseEvent != null)
        {
            wwiseEvent.Stop(gameObject);
        }
    }
}