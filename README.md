# Connect to SAP and do RFC Calls by c#.NET Code 
This is a sample code for Integrating SAP using .NET Connector 3.0 (NCO)

SAP .NET connector (NCO) connects Microsoft .NET application and SAP systems by using the SAP RFC. This is called as NCo. Similarly, e can connect by using the Java, which is called the JCO. We will go with the .NET connector. Before moving forward these are prerequisites to understanding the process easily. Must have basic knowledge of development using C# and ASP.Net For developing the connector you will require the following Dll’s. There are 32 bit (x86) and 64-bit versions are available.



Create a web application SAP_NCO_ConnectionDemo and add reference of sapnco.dll and sapnco_utils.dll

# Where to get NCO 3.0
+ The SAP .NET Connector 3.0.20 can be downloaded from the SAP Service Marketplace.
 + sapnco.dll
 + sapnco_utils.dll

Navigate to SAP Connector for Microsoft .NET
Download SAP Connector for Microsoft .NET Version 3.0
This will require a username and password. If you are a .net developer, probably you do not have it. Ask Your SAP Basis team to download and get it for you because they have the access to SAP marketplace.

Key fields
Given below are the key fields you will need to generate a remote session with SAP. These are available with the SAP Basis team or the system administrators.

+ SAP_USERNAME
+ SAP_APPSERVERHOST
+ SAP_PASSWORD
+ SAP_SYSNUM
+ SAP_CLIENT
+ SAP_LANGUAGE
+ SAP_POOLSIZE
+ SAP_ENVIRONMENT_NAME(ROUTER config) -> CONFIG_NAME inside RfcClient.cs file 
