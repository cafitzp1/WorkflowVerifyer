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
        public String OldValuePair { get; private set; }
        public String NewValuePair { get; private set; }

        public ItemModification(String a_DocumentWorkflowItemID, String a_OldValuePair, String a_NewValuePair)
        {
            DocumentWorkflowItemID = a_DocumentWorkflowItemID;
            OldValuePair = a_OldValuePair;
            NewValuePair = a_NewValuePair;
        }
        
        public String InitialModification(String a_DocumentWorkflowItemID, String a_OldValuePair, String a_NewValuePair)
        {
            return $"- [{DocumentWorkflowItemID}]\t{a_OldValuePair} -> {a_NewValuePair}";
        }
        public String AddlModification(String a_OldValuePair, String a_NewValuePair)
        {
            return $"\t\t\t{a_OldValuePair} -> {a_NewValuePair}";
        }

        public override String ToString()
        {
            return $"- [{DocumentWorkflowItemID}]\t{OldValuePair} -> {NewValuePair}";
        }
    }
}