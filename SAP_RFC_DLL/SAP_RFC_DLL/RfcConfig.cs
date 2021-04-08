using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAP_RFC_DLL
{
    public class RfcConfig : IDestinationConfiguration
    {
        #region IDestinationConfiguration
        public bool ChangeEventsSupported()
        {
            return true;
        }

        RfcDestinationManager.ConfigurationChangeHandler changeHandler;
        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged
        {
            add { changeHandler += value; }
            remove { changeHandler -= value; }
        }

        public RfcConfigParameters GetParameters(string destinationName)
        {
            RfcConfigParameters rfcParams = new RfcConfigParameters();
            rfcParams.Add(RfcConfigParameters.AppServerHost, "SAP_HOST");
            rfcParams.Add(RfcConfigParameters.SystemNumber, "SAP_SYSNUM");
            rfcParams.Add(RfcConfigParameters.SystemID, "SAP_SYSID ");
            rfcParams.Add(RfcConfigParameters.User, "SAP_USER");
            rfcParams.Add(RfcConfigParameters.Password, "SAP_PASSWORD");
            rfcParams.Add(RfcConfigParameters.Client, "SAP_CLIENT");
            rfcParams.Add(RfcConfigParameters.Language, "EN");
            rfcParams.Add(RfcConfigParameters.PoolSize, "5");

            return rfcParams;

        }
    }
}
