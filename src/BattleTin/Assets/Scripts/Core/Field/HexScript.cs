using UnityEngine;
using UnityEngine.EventSystems;

public class Hex : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Vector2Int gridCoordinates;
    public Material defaultMaterial;
    public Material hoverMaterial;
    public Material selectMaterial;

    private MeshRenderer meshRenderer;
    private bool isSelected = false;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        HexSelectionManager.Instance.OnHexClicked(this);
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