using UnityEngine;

namespace Framework.Volumes
{
    /// <summary>
    /// An asset that represents a layer on which volumes sharing the layer
    /// can be blended between.
    /// </summary>
    [CreateAssetMenu(fileName = "New Volume Layer", menuName = "Framework/Volumes/Layer", order = 0)]
    public class VolumeLayer : ScriptableObject
    {
    }
}
