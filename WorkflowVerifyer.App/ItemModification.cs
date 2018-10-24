using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowVerifyer.App
{
    internal class ItemModification
    {
        private readonly String m_DocumentWorkflowItemID;
        private readonly String m_Modification;

        public String DocumentWorkflowItemID { get => m_DocumentWorkflowItemID; }
        public String Modification { get => m_Modification; }

        public ItemModification(String a_DocumentWorkflowItemID, String a_Modification)
        {
            m_DocumentWorkflowItemID = a_DocumentWorkflowItemID;
            m_Modification = a_Modification;
        }

        public override String ToString()
        {
            return $"{m_DocumentWorkflowItemID}: {m_Modification}";
        }
    }
}