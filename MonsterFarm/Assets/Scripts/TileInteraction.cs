using UnityEngine;

/// Deprecated. Mouse-based farm interaction (hover highlight + click to plant/harvest) has
/// been replaced by AvatarInteraction — walk the avatar onto a field tile and press E.
///
/// This component is now inert (does nothing). It is kept as an empty class so existing
/// scene objects that still reference it don't show a "missing script" error. You can safely
/// remove the component from the GameObject and delete this file once the scene is cleaned up.
public class TileInteraction : MonoBehaviour
{
}
