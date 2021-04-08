using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAP.Middleware.Connector;
namespace SAP_RFC_DLL
{
    public class Class1
    {
        public string SAP_RFC_ReadTable_MARD()
        {
            RfcClient rfcclient = new RfcClient();
            return rfcclient.RfcReadTable_MARD();
        }
        public string SAP_RFC_ReadTable_OKKE()
        {
            RfcClient rfcclient = new RfcClient();
            return rfcclient.RfcReadTable_OKKE();
        }
    }
}
