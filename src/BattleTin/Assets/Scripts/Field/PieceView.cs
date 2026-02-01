using System;
using System.Linq;
using MIDIFrogs.BattleTin.Field.Assets.Scripts.Field;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] private PieceVisuals visualsConfig;
        [SerializeField] private GameObject visualization;
        
        public int PieceId;
        public int TeamId;
        public bool IsKing;


        public void SetMask(MaskType mask)
        {
            Destroy(visualization);

            var v = visualsConfig.Pieces.First(x => x.Mask == mask && x.TeamIndex == TeamId);
            visualization = Instantiate(v.Visual, transform);
        }
    }
}
