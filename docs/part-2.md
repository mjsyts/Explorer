# Part 2: Rerouting Audio to Wwise

## What RandomAudioPlayer Does

2DGameKit handles all of its SFX through a single class, `RandomAudioPlayer`. Each sound lives on its own child `GameObject` under a `SoundSources` folder — each one carrying both an `AudioSource` and the `RandomAudioPlayer` script as components. When something needs to play a sound — a footstep, a bullet, a hit — it calls `PlayRandomSound()`, which picks a random clip from an array and fires it via the `AudioSource`:

```cs
public void PlayRandomSound(TileBase surface = null)
{
    AudioClip[] source = clips;

    AudioClip[] temp;
    if (surface != null && m_LookupOverride.TryGetValue(surface, out temp))
        source = temp;

    int choice = Random.Range(0, source.Length);

    if(randomizePitch)
        m_Source.pitch = Random.Range(1.0f - pitchRange, 1.0f + pitchRange);

    m_Source.PlayOneShot(source[choice]);
}
```

The one interesting bit is the surface override. Footsteps use a `TileOverride` struct to map surface tiles to different clip arrays, so the sound changes depending on what the player is standing on. That's the only piece of logic here that we actually need to keep.

## What Stays on the Unity Side

From the Unity side, all we need is the ability to post a Wwise event and set switches for things like surfaces. That's it.

## The Replacement: WwiseAudioPlayer

Here's the `WwiseAudioPlayer` class that will replace the 2DGameKit implementation:

```cs
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
```

The TileOverride struct is the same idea as before — TileBase maps to a switch instead of a clip array. Everything else is handled in Wwise.

## Wiring It Up

### To get this working in `PlayerCharacter`, we need to:

### 1. Swap the five `RandomAudioPlayer` references to `WwiseAudioPlayer`

```cs
        public WwiseAudioPlayer footstepAudioPlayer;
        public WwiseAudioPlayer landingAudioPlayer;
        public WwiseAudioPlayer hurtAudioPlayer;
        public WwiseAudioPlayer meleeAttackAudioPlayer;
        public WwiseAudioPlayer rangedAttackAudioPlayer;
```

### 2. Change `PlayRandomSound()` calls to `Play()`

#### A. `SpawnBullet()`
```cs
        protected void SpawnBullet()
        {
            //...
            rangedAttackAudioPlayer.Play();
        }
```

#### B. `CheckForGrounded()`

```cs
        public bool CheckForGrounded()
        {
            //...
                    landingAudioPlayer.Play(m_CurrentSurface);
            //...
        }
```

#### C. `OnHurt()`

```cs
        public void OnHurt(Damager damager, Damageable damageable)
        {
            //...
            m_Animator.SetBool(m_HashGroundedPara, false);
            hurtAudioPlayer.Play();
            //...
        }
```

#### D. `EnableMeleeAttack()`

```cs
        public void EnableMeleeAttack()
        {
            meleeDamager.EnableDamage();
            meleeDamager.disableDamageAfterHit = true;
            meleeAttackAudioPlayer.Play();
        }
```

#### E. 'PlayFootstep()'

```cs
        public void PlayFootstep()
        {
            footstepAudioPlayer.Play(m_CurrentSurface);
            //...
        }
```

### 3. Remove the AudioSource child objects from the player

### 4. Point the Inspector fields at our Wwise events and switches