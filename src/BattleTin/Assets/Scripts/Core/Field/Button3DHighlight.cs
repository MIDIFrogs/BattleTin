using UnityEngine;
using UnityEngine.EventSystems;

namespace MIDIFrogs.BattleTin.Core
{
    [RequireComponent(typeof(MeshRenderer))]
    public class Button3DHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Material defaultMaterial;
        public Material hoverMaterial;
        public Material selectMaterial;

        private bool isSelected = false;
        private MeshRenderer meshRenderer;

        void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            defaultMaterial = meshRenderer.material;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isSelected)
                meshRenderer.material = hoverMaterial;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isSelected)
                meshRenderer.material = defaultMaterial;
        }

        public void Select()
        {
            isSelected = true;
            meshRenderer.material = selectMaterial;
        }

        public void Deselect()
        {
            isSelected = false;
            meshRenderer.material = defaultMaterial;
        }
    }
}
