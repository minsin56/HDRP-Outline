using System;
using UnityEngine;

namespace Modules.Rendering.Outline
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Renderer))]
    public class OutlineComponent : MonoBehaviour
    {
        private void OnEnable()
        {
            OutlinePass.OutlineRenderers.Add(GetComponent<Renderer>());
        }

        private void OnDisable()
        {
            OutlinePass.OutlineRenderers.Remove(GetComponent<Renderer>());
        }
    }
}