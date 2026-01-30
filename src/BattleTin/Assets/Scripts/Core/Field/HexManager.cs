using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class HexSelectionManager : MonoBehaviour
{
    public static HexSelectionManager Instance;

    public GameObject playerUnitPrefab;
    private GameObject currentSelectedUnit;
    private Hex currentSelectedHex;
    private List<Hex> availableMoveHexes = new List<Hex>();

    void Awake()
    {
        availableMoveHexes = FindObjectsOfType<Hex>().ToList();
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
      
    }

    public void OnHexClicked(Hex clickedHex)
    {
        Debug.Log(string.Join("", availableMoveHexes));
        Debug.Log(clickedHex);
        if (currentSelectedUnit != null && availableMoveHexes.Contains(clickedHex))
        {
            MoveUnitToHex(clickedHex);
            ClearAvailableMoves();
            return;
        }
        if (clickedHex.transform.childCount > 0 && clickedHex.transform.GetChild(0).gameObject.CompareTag("PlayerUnit"))
        {
            SelectUnit(clickedHex.transform.GetChild(0).gameObject, clickedHex);
            ShowAvailableMoves(clickedHex);
        }
        else
        {
            ClearSelection();
        }
    }

    void SelectUnit(GameObject unit, Hex hex)
    {
        ClearSelection();
        currentSelectedUnit = unit;
        currentSelectedHex = hex;
        hex.Select(); // Подсветили гекс под фигуркой
        // Можно добавить эффект на саму фигурку
    }

    void ShowAvailableMoves(Hex fromHex)
    {
       availableMoveHexes = FindObjectsOfType<Hex>().ToList();
    }

    void MoveUnitToHex(Hex targetHex)
    {
        if (currentSelectedUnit == null) return;

        Debug.Log("doMove");
        currentSelectedUnit.transform.DOMove(targetHex.transform.position + Vector3.up * 0.5f, 0.5f);
        // Прикрепить фигурку к новому гексу (опционально)
        currentSelectedUnit.transform.SetParent(targetHex.transform);
        currentSelectedHex.Deselect();
        currentSelectedHex = targetHex;
        currentSelectedHex.Select(); // Теперь она выбрана на новом месте
    }

    void ClearAvailableMoves()
    {
        foreach (Hex hex in availableMoveHexes)
        {
            hex.Deselect();
        }
        availableMoveHexes.Clear();
    }

    void ClearSelection()
    {
        if (currentSelectedHex != null) currentSelectedHex.Deselect();
        currentSelectedUnit = null;
        currentSelectedHex = null;
        ClearAvailableMoves();
    }
}