using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAP.Middleware.Connector;
using System.Xml.Linq;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace SAP_RFC_DLL
{
    class RfcClient
    {
        const string CONFIG_NAME = "connectiontest";
        RfcDestination _ECCsystem;
        private RfcConfig config;

        public RfcDestination GetDestination(string destinationName)
        {
            try
            {
                _ECCsystem = null;
                _ECCsystem = RfcDestinationManager.GetDestination(destinationName);
            }
            catch
            {
                Console.WriteLine("Environment Not Registered", string.Format("SAP environment not registered for conn : '{0}'", destinationName));
            }

            if (_ECCsystem == null)
            {
                try
                {
                    RfcDestinationManager.RegisterDestinationConfiguration(config);
                    _ECCsystem = RfcDestinationManager.GetDestination(destinationName);
                    Console.WriteLine("SAP Environment Registered", string.Format("SAP environment registered for conn : '{0}'", destinationName));
                }
                catch
                {
                    throw new Exception(string.Format("SAP Environment Registration Process Failed for conn : {0}", destinationName));
                }
            }

            return _ECCsystem;
        }

        public RfcClient()
        {
            config = new RfcConfig();
            _ECCsystem = GetDestination(CONFIG_NAME);
        }


        public static string InnerXML(this XElement el)
        {
            var reader = el.CreateReader();
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }
        public string RfcReadTable_MARD()
        {
            string rfcRequest = "<RFC_READ_TABLE><QUERY_TABLE>MARD</QUERY_TABLE><DELIMITER>*</DELIMITER><ROWSKIPS>0</ROWSKIPS><ROWCOUNT>0</ROWCOUNT><TABLE><OPTIONS><ROW><TEXT>MATNR IN (</TEXT></ROW><ROW><TEXT>'testConnection'</TEXT></ROW><ROW><TEXT>)</TEXT></ROW></OPTIONS></TABLE></RFC_READ_TABLE>";
            RfcClient client = new RfcClient();
            try
            {
                XElement response = client.PullRequestToSAPrfc(rfcRequest);
                return InnerXML(response);
            }
            catch (RfcLogonException ex)
            {
                return  "Logon Failed";
            }
            catch (RfcInvalidStateException ex)
            {
                return "RFC Failed";
            }
            catch (RfcBaseException ex)
            {
                return  "communication error";
            }
            catch (Exception ex)
            {
                return  "Connection error";
            }
            finally
            {
                //client.disconnectDestination();
            }
            
        }

        public string RfcReadTable_OKKE()
        {
            bool state = false;
            string rfcRequest = "<RFC_READ_TABLE><QUERY_TABLE>OKKE</QUERY_TABLE><DELIMITER>*</DELIMITER><ROWSKIPS>0</ROWSKIPS><ROWCOUNT>0</ROWCOUNT><TABLE><OPTIONS><ROW><TEXT>MATNR IN (</TEXT></ROW><ROW><TEXT>'testConnection'</TEXT></ROW><ROW><TEXT>)</TEXT></ROW></OPTIONS></TABLE></RFC_READ_TABLE>";

            RfcClient client = new RfcClient();
            try
            {
                XElement response = client.PullRequestToSAPrfc(rfcRequest);
                //state = true;
                return InnerXML(response);
            }
            catch (RfcLogonException ex)
            {
                return "Logon Failed";
            }
            catch (RfcInvalidStateException ex)
            {
                return "RFC Failed";
            }
            catch (RfcBaseException ex)
            {
                return "communication error";
            }
            catch (Exception ex)
            {
                return "Connection error";
            }
            finally
            {
                //client.disconnectDestination();
            }

        }

        public XElement PullRequestToSAPrfc(string XMLRequest)
        {
            IRfcFunction requestFn;
            requestFn = PrepareRfcFunctionFromXML(XElement.Parse(XMLRequest));

            RfcSessionManager.BeginContext(_ECCsystem);
            requestFn.Invoke(_ECCsystem);
            RfcSessionManager.EndContext(_ECCsystem);

            XElement XMLResponse = PrepareXMLFromrfc(requestFn);

            return XMLResponse;
        }


        public IRfcFunction PrepareRfcFunctionFromXML(XElement xmlFunction)
        {
            RfcRepository repo = _ECCsystem.Repository;
            IRfcFunction RfcFunction = repo.CreateFunction(xmlFunction.Name.ToString());
            foreach (XElement xelement in xmlFunction.Elements())
            {
                if (xelement.Name.ToString().Equals("TABLE"))
                {
                    if (NotProcessSpecialTable(xelement))
                        continue;
                    IRfcTable options = RfcFunction.GetTable(xelement.Descendants().First().Name.ToString());
                    foreach (XElement row in xelement.Elements().First().Elements())
                    {
                        options.Append();
                        foreach (XElement rowElement in row.Elements())
                        {
                            string elementName = rowElement.Name.ToString();
                            RfcElementMetadata elementMeta = options.GetElementMetadata(elementName);
                            var elementValue = getValueAsMetadata(ref elementMeta, rowElement.Value);
                            if (elementValue is string && string.IsNullOrEmpty((string)elementValue)) { continue; }
                            options.SetValue(elementName, elementValue);
                        }
                    }
                }
                else if (xelement.Name.ToString().Equals("STRUCT"))
                {
                    IRfcStructure options = RfcFunction.GetStructure(xelement.Descendants().First().Name.ToString());
                    foreach (XElement structElement in xelement.Elements().First().Elements())
                    {
                        string elementName = structElement.Name.ToString();
                        RfcElementMetadata elementMeta = options.GetElementMetadata(elementName);
                        var elementValue = getValueAsMetadata(ref elementMeta, structElement.Value);
                        if (elementValue is string && string.IsNullOrEmpty((string)elementValue)) { continue; }
                        options.SetValue(elementName, elementValue);
                    }
                }
                else
                {
                    string elementName = xelement.Name.ToString();
                    RfcElementMetadata elementMeta = RfcFunction.GetElementMetadata(elementName);
                    var elementValue = getValueAsMetadata(ref elementMeta, xelement.Value);
                    if (elementValue is string && string.IsNullOrEmpty((string)elementValue)) { continue; }
                    RfcFunction.SetValue(elementName, elementValue);
                }
            }
            return RfcFunction;
        }

        public XElement PrepareXMLFromrfc(IRfcFunction rfcFunction)
        {
            var XMLRoot = new XElement(rfcFunction.Metadata.Name);
            for (int functionIndex = 0; functionIndex < rfcFunction.ElementCount; functionIndex++)
            {
                var functionMatadata = rfcFunction.GetElementMetadata(functionIndex);
                if (functionMatadata.DataType == RfcDataType.TABLE)
                {
                    var rfcTable = rfcFunction.GetTable(functionMatadata.Name);
                    var XMLTable = new XElement(functionMatadata.Name);
                    foreach (IRfcStructure rfcStracture in rfcTable)
                    {
                        XElement XMLRow = new XElement("ROW");
                        for (int i = 0; i < rfcStracture.ElementCount; i++)
                        {
                            RfcElementMetadata rfcElementMetadata = rfcStracture.GetElementMetadata(i);
                            if (rfcElementMetadata.DataType == RfcDataType.BCD)
                            { XMLRow.Add(new XElement(rfcElementMetadata.Name, rfcStracture.GetString(rfcElementMetadata.Name))); }
                            else
                            {
                                XMLRow.Add(new XElement(rfcElementMetadata.Name, rfcStracture.GetString(rfcElementMetadata.Name)));
                            }
                        }

                        XMLTable.Add(XMLRow);
                    }
                    XMLRoot.Add(XMLTable);
                }
                else if (functionMatadata.DataType == RfcDataType.STRUCTURE)
                {
                    var rfcStructure = rfcFunction.GetStructure(functionMatadata.Name);
                    XElement XMLRow = new XElement(functionMatadata.Name);
                    for (int elementIndex = 0; elementIndex < rfcStructure.ElementCount; elementIndex++)
                    {
                        RfcElementMetadata eleMeta = rfcStructure.GetElementMetadata(elementIndex);
                        XMLRow.Add(new XElement(eleMeta.Name, rfcStructure.GetString(eleMeta.Name)));
                    }
                    XMLRoot.Add(XMLRow);
                }
                else
                {
                    RfcElementMetadata rfcElement = rfcFunction.GetElementMetadata(functionIndex);
                    XMLRoot.Add(new XElement(rfcElement.Name, rfcFunction.GetString(rfcElement.Name)));
                }
            }
            return XMLRoot;
        }

        private object getValueAsMetadata(ref RfcElementMetadata elementMeta, string value)
        {
            switch (elementMeta.DataType)
            {
                case RfcDataType.BCD:
                    return value;
                case RfcDataType.NUM:
                    if (value.Contains("."))
                    {
                        int elementValue;
                        int.TryParse(value, out elementValue);
                        return elementValue;
                    }
                    else
                    {
                        return Convert.ToInt32(value);
                    }
                case RfcDataType.INT1:
                    return Convert.ToInt32(value);
                case RfcDataType.INT2:
                    return Convert.ToInt32(value);
                case RfcDataType.INT4:
                    return Convert.ToInt32(value);
                case RfcDataType.INT8:
                    return Convert.ToInt64(value);
                case RfcDataType.CHAR:
                    return value;
                case RfcDataType.DATE:
                    return DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);


                default:
                    return string.Empty;
            }
        }
    }
}
