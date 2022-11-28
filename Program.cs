using System.Text;
using System;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Configuration;

namespace UA_Client_RFU6xx
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("OPC UA - RFU6xx Client");

            try
            {
                // Define the UA Client application
                ApplicationInstance application = new ApplicationInstance();
                application.ApplicationName = "OPC UA - RFU6xx Client";
                application.ApplicationType = ApplicationType.Client;

                // Load the application configuration
                await application.LoadApplicationConfiguration("UA-Client-RFU6xx.Config.xml", silent: false);
                // Check the application certificate
                await application.CheckApplicationInstanceCertificate(silent: false, minimumKeySize: 0);

                // Create the UA Client object and connect to configured server
                RFU6xxClient rfuClient = new RFU6xxClient(application.ApplicationConfiguration, ClientBase.ValidateResponse);
                
                // Set the discovery URL of the Client
                rfuClient.ServerUrl = "opc.tcp://localhost:4840";
                
                bool connected = await rfuClient.ConnectAsync();

                if (connected)
                {
                    // Run tests for available methods when successfully connected

                    var lastScanData = "";

                    // Start the scan process on the RFU6xx
                    var scanDuration = 2000;
                    if(rfuClient.ScanStart(scanDuration, 0, false))
                    {
                        await Task.Delay(scanDuration + 100);
                        lastScanData = rfuClient.GetLastScanData();
                    }

                    await Task.Delay(2_000);

                    rfuClient.ReadTag(2, lastScanData, "RAW:STRING", 3, 0, 16, new byte[0]);

                    await Task.Delay(2_000);
                    
                    byte[] data = Encoding.UTF8.GetBytes("testwrite123test");
                    rfuClient.WriteTag(2, lastScanData, "RAW:STRING", 3, 0, data, new byte[0]);

                    await Task.Delay(2_000);

                    rfuClient.ReadTag(2, lastScanData, "RAW:STRING", 3, 0, 16, new byte[0]);

                    await Task.Delay(2_000);

                    byte[] data2 = Encoding.UTF8.GetBytes("affedeafbead1234");
                    rfuClient.WriteTag(2, lastScanData, "RAW:STRING", 3, 0, data2, new byte[0]);

                    await Task.Delay(2_000);

                    rfuClient.ReadTag(2, lastScanData, "RAW:STRING", 3, 0, 16, new byte[0]);

                    await Task.Delay(2_000);

                    rfuClient.Disconnect();
                }
                else
                {
                    Console.WriteLine("Could not connect to server!");
                }

                Console.WriteLine("\nProgram ended.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
