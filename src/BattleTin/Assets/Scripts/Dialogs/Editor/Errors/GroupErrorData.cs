using System.Collections.Generic;
using MIDIFrogs.FutureInThePast.Editor.Dialogs;

namespace MIDIFrogs.FutureInThePast.Editor.Errors
{
    public class GroupErrorData
    {
        public DialogErrorData ErrorData { get; set; }
        public List<LinesGroup> Groups { get; set; }

        public GroupErrorData()
        {
            ErrorData = new DialogErrorData();
            Groups = new List<LinesGroup>();
        }
    }
}