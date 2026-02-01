using System.Collections.Generic;
using MIDIFrogs.FutureInThePast.Editor.Dialogs;

namespace MIDIFrogs.FutureInThePast.Editor.Errors
{
    public class DialogNodeErrorData
    {
        public DialogErrorData ErrorData { get; set; }
        public List<LineNode> Nodes { get; set; }

        public DialogNodeErrorData()
        {
            ErrorData = new DialogErrorData();
            Nodes = new List<LineNode>();
        }
    }
}