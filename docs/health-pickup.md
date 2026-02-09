## Health Pickup

Health pickup (and probably a number of other prefabs) rely on `InventoryItem` for audio calls, so we replace the `AudioClip` member with a `WwiseAudioPlayer` and call Play() on it.