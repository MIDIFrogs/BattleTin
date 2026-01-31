using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEngine;

namespace MIDIFrogs.BattleTin.UI
{
    [CreateAssetMenu(menuName = "Masks/Visualization options")]
    internal class MaskVisualizationOptions : ScriptableObject
    {
        [SerializeField] private List<MaskVisualization> visualizations;

        internal List<MaskVisualization> Visualizations => visualizations;

        public MaskVisualization GetVisualization(MaskType type) => visualizations.FirstOrDefault(x => x.MaskType == type);

        public string GetMaskName(MaskType type) => visualizations.FirstOrDefault(x => x.MaskType == type).MaskName ?? type.ToString();
    }
}
