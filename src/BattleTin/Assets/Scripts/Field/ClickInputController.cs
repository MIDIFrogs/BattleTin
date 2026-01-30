using MIDIFrogs.BattleTin.Field.Assets.Scripts.Field;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MIDIFrogs.BattleTin.Field
{
    public class ClickInputController : MonoBehaviour
    {
        Camera cam;

        void Awake()
        {
            cam = Camera.main;
        }

        void Update()
        {
            if (!Mouse.current.leftButton.isPressed)
                return;

            var mousePosition = Mouse.current.position.ReadValue();

            if (!Physics.Raycast(
                cam.ScreenPointToRay(mousePosition),    
                out var hit
            )) return;

            if (hit.collider.TryGetComponent<PieceView>(out var piece))
            {
                Debug.Log($"Clicked piece {piece.PieceId}");
            }

            if (hit.collider.TryGetComponent<CellView>(out var cell))
            {
                Debug.Log($"Clicked cell {cell.CellId}");
            }
        }
    }
}
