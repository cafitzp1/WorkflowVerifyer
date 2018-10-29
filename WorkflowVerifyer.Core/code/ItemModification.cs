using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowVerifyer.Core
{
    public class ItemModification
    {
        public String DocumentWorkflowItemID { get; private set; }
        public String Modification { get; private set; }

        public ItemModification(String a_DocumentWorkflowItemID, String a_Modification)
        {
            DocumentWorkflowItemID = a_DocumentWorkflowItemID;
            Modification = a_Modification;
        }
        public override String ToString()
        {
            return $"{DocumentWorkflowItemID}: {Modification}";
        }
    }
}