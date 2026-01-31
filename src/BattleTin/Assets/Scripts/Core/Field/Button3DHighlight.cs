using Newtonsoft.Json.Bson;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MIDIFrogs.BattleTin.Core
{
    public class Button3DHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Material defaultMaterial;
        public Material hoverMaterial;
        public Material selectMaterial;
        public Material highlightMaterial;

        private bool isSelected = false;
        private bool isHighlighted = false;
        [SerializeField] private MeshRenderer meshRenderer;

        void Start()
        {
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
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
                meshRenderer.material = isHighlighted ? highlightMaterial : defaultMaterial;
        }

        public void Select()
        {
            isSelected = true;
            meshRenderer.material = selectMaterial;
        }

        public void Deselect()
        {
            isSelected = false;
            meshRenderer.material = isHighlighted ? highlightMaterial : defaultMaterial;
        }

        public void Highlight()
        {
            isHighlighted = true;
            meshRenderer.material = highlightMaterial;
        }

        public void RemoveHighlight()
        {
            isHighlighted = false;
            meshRenderer.material = defaultMaterial;
        }
    }
}
