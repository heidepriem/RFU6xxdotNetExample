using System;
using System.Collections;
using System.Text;
using Opc.Ua;
using static UA_Client_RFU6xx.AutoIDDataModels;

namespace UA_Client_RFU6xx
{
    public class RFU6xxClient : UAClientBase
    {
        public RFU6xxClient(ApplicationConfiguration configuration, Action<IList, IList> validateResponse) : base(configuration, validateResponse) {}

        public override void onConnect()
        {
            // Initialize the desired namespace indices for later use
            this.nsAutoID = Session.NamespaceUris.GetIndex("http://opcfoundation.org/UA/AutoID/");
            this.nsOpcDI = Session.NamespaceUris.GetIndex("http://opcfoundation.org/UA/DI/");
            this.nsRfu = Session.NamespaceUris.GetIndex("http://www.sick.com/RFU6xx/");

            // RFU6xx node - Objects\DeviceSet\RFU6xx
            this.rfu6xxNodeId = new NodeId("ns=" + this.nsRfu + ";i=5002");
        }


        /// <summary>Executes the AutoID method 'ScanStart' on the RFU, which
        /// starts a scan. If duration is 0, the scan won't stop automatically.</summary>
        /// <param name="duration">The scan duration in milliseconds. Runs indefinitely if 0 and scan has to be stopped manually.</param>
        /// <param name="cycles">The amount of cycles.</param>
        /// <returns>Returns true if the scan method has been executed</returns>
        public bool ScanStart(double duration, int cycles, bool dataAvailable)
        {
            // Define the UA Method to call
            // Method node - Objects\DeviceSet\RFU6xx\ScanStart
            NodeId methodId = new NodeId("ns=" + this.nsRfu + ";i=7002");

            // Define the input arguments
            var scanSettings = new ScanSettings(0, duration, cycles, dataAvailable, this.nsAutoID);
            var eo = new ExtensionObject(scanSettings);
            var inputArgs = new object[] {eo};

            Console.WriteLine("\nExecuting method ScanStart ...");
            var output = CallMethod(rfu6xxNodeId, methodId, inputArgs);
            if(output != null)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Execute the AutoID method 'ScanStop' on the RFU, which
        /// stops the scanning.</summary>
        public bool ScanStop()
        {
            // Define the UA Method to call
            // Method node - Objects\DeviceSet\RFU6xx\ScanStop
            NodeId methodId = new NodeId("ns=" + this.nsRfu + ";i=7003");

            Console.WriteLine("\nExecuting method ScanStop ...");
            var output = CallMethod(rfu6xxNodeId, methodId, null);
            if(output != null)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Reads the 'LastScanData' node of the RFU.
        /// </summary>
        /// <returns>The tag id last scanned by the RFU6xx.</returns>
        public string GetLastScanData()
        {
            // Define the node to read
            // Node - Objects\DeviceSet\RFU6xx\LastScanData
            NodeId nodeId = new NodeId("ns=" + this.nsRfu + ";i=6023");

            // Read the node value attribute
            Console.WriteLine("\nGetting LastScanData ...");
            var result = ReadNode(nodeId, Attributes.Value);

            // ReadNode() returns null when it fails
            // => Return null when 'result' is null
            if(result == null)
            {
                return null;
            }

            // Format the result data
            var tagId = result.Value.ToString();
            var statusCode = result.StatusCode;

            // Print out the data
            Console.WriteLine("LastScanData Results:");
            Console.WriteLine("\tTag ID = {0}", tagId);
            Console.WriteLine("\tStatus Code = {0}", statusCode.ToString());

            // Return the Tag ID
            return tagId;
        }


        /// <summary>
        /// Execute the AutoID method 'ReadTag' on the RFU
        /// </summary>
        /// <param name="switchField"></param>
        /// <param name="tagId"></param>
        /// <param name="codetype"></param>
        /// <param name="region"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="password"></param>
        public bool ReadTag(uint switchField, object tagId, string codetype, ushort region, uint offset, uint length, byte[] password)
        {
            // Define the UA Method to call
            // Method node - Objects\DeviceSet\RFU6xx\ReadTag
            NodeId methodId = new NodeId("ns=" + this.nsRfu + ";i=7004");

            // Define the input arguments
            // 'ReadTag' requires a ScanData object as an ExtensionObject
            // as well as the codetype, region, offset, length and an optional password
            var scanData = new ScanData(switchField, tagId, this.nsAutoID);
            var eo = new ExtensionObject(scanData);
            object[] inputArgs = new object[] { eo, codetype, region, offset, length, password };

            // Call the method
            Console.WriteLine("\nExecuting method ReadTag ...");
            var outputArgs = CallMethod(rfu6xxNodeId, methodId, inputArgs);

            // CallMethod() returns null if it fails
            // => Return false if 'outputArgs' is null
            if(outputArgs == null)
            {
                return false;
            }

            Console.WriteLine("ReadTag Result:");
            
            // Format the data
            var tagValueByte = outputArgs[0] as byte[];
            var tagValue = Encoding.UTF8.GetString(tagValueByte, 0, tagValueByte.Length);
            var statusCode = outputArgs[1];
            
            Console.WriteLine("\tTag Value = {0}", tagValue);
            Console.WriteLine("\tStatus Code = {0}", statusCode.ToString());

            // Return true since the tag was successfully read (outputArgs != null)
            return true;
        }


        /// <summary>
        /// Execute the AutoID method 'WriteTag' on the RFU
        /// </summary>
        /// <param name="switchField"></param>
        /// <param name="tagId"></param>
        /// <param name="codetype"></param>
        /// <param name="region"></param>
        /// <param name="offset"></param>
        /// <param name="data"></param>
        /// <param name="password"></param>
        public bool WriteTag(uint switchField, object tagId, string codetype, ushort region, uint offset, byte[] data, byte[] password)
        {
            // Define the UA Method to call
            // Method node - Objects\DeviceSet\RFU6xx\WriteTag
            NodeId methodId = new NodeId("ns=" + this.nsRfu + ";i=7005");

            // Define the input arguments
            // 'ReadTag' requires a ScanData object as an ExtensionObject
            // as well as the codetype, region, offset, data and an optional password
            var scanData = new ScanData(switchField, tagId, this.nsAutoID);
            var eo = new ExtensionObject(scanData);
            object[] inputArgs = new object[] { eo, codetype, region, offset, data, password };

            // Call the method
            Console.WriteLine("\nExecuting method WriteTag ...");
            var outputArgs = CallMethod(rfu6xxNodeId, methodId, inputArgs);

            // CallMethod() returns null if it fails
            // => Return false if 'outputArgs' is null
            if(outputArgs == null)
            {
                return false;
            }

            Console.WriteLine("WriteTag Result:");
            
            // Format the data
            var statusCode = outputArgs[0];
            Console.WriteLine("\tStatus Code = {0}", statusCode.ToString());

            // Return true since the tag was successfully read (outputArgs != null)
            return true;
        }



        #region Private Fields
        private int nsAutoID = 0;
        private int nsOpcDI = 0;
        private int nsRfu = 0;
        private NodeId rfu6xxNodeId = null;

        #endregion
    }
}