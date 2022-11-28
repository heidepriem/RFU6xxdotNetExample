using System;
using Opc.Ua;

namespace UA_Client_RFU6xx
{
    public static class AutoIDDataModels
    {

        /// <summary>
        /// 'ScanSettings' data model of the AutoID Companion Specification.
        /// </summary>
        public class ScanSettings : EncodeableObject
        {
            public int locationTypeSpecified { get; set; }
            public double duration { get; set; }
            public int cycles { get; set; }
            public bool dataAvailable { get; set; }
            public int nsAutoID { get; set; }

            //public ScanSettings(){}

            /// <summary>
            /// 'ScanSettings' data model object constructor.
            /// Initializes the 'ScanSettings' object with all parameters.
            /// </summary>
            public ScanSettings(int locationTypeSpecified, double duration, int cycles, bool dataAvailable, int nsAutoID)
            {
                this.locationTypeSpecified = locationTypeSpecified;
                this.duration = duration;
                this.cycles = cycles;
                this.dataAvailable = dataAvailable;
                this.nsAutoID = nsAutoID;
            }

            public override ExpandedNodeId TypeId => ExpandedNodeId.Parse("ns=" + nsAutoID +";i=3010");
            public override ExpandedNodeId BinaryEncodingId => ExpandedNodeId.Parse("ns=" + nsAutoID + ";i=3010");
            public override ExpandedNodeId XmlEncodingId => NodeId.Null;

            public override void Decode(IDecoder decoder)
            {
                this.locationTypeSpecified = decoder.ReadInt32("LocationTypeSpecified");
                this.duration = decoder.ReadDouble("Duration");
                this.cycles = decoder.ReadInt32("Cycles");
                this.dataAvailable = decoder.ReadBoolean("DataAvailable");
            }

            public override void Encode(IEncoder encoder)
            {
                encoder.WriteInt32("LocationTypeSpecified", this.locationTypeSpecified);
                encoder.WriteDouble("Duration", this.duration);
                encoder.WriteInt32("Cycles", this.cycles);
                encoder.WriteBoolean("DataAvailable", this.dataAvailable);
            }

            public override string ToString()
            {
                return String.Format("ScanSettings:\n\tLocationTypeSpecified: {0}, Duration: {1}, Cycles: {2}, DataAvailable: {3}",
                    this.locationTypeSpecified, this.duration, this.cycles, this.dataAvailable);
            }
        }

        /// <summary>
        /// 'ScanData' data model of the AutoID Companion Specification.
        /// </summary>
        public class ScanData : Union
        {
            public uint switchField { get; set; }
            public object tagId { get; set; }
            public int nsAutoID { get; set; }

            //public ScanData(){}

            /// <summary>
            /// 'ScanData' data model object constructor.
            /// Initializes the 'ScanData' object with all parameters.
            /// </summary>
            /// <param name="switchField">The index stating which type of tag identifier to use.
            /// <example>
            /// <code>0 = NULL</code>
            /// <code>1 = ByteString</code>
            /// <code>2 = String</code>
            /// <code>3 = EPC</code>
            /// <code>4 = Custom:TID</code>
            /// </example>
            /// </param>
            /// <param name="tagId">The tagId in the specified type in 'switchField'.</param>
            public ScanData(uint switchField, object tagId, int nsAutoID)
            {
                this.switchField = switchField;
                this.nsAutoID = nsAutoID;
                this.tagId = tagId;
            }

            public override ExpandedNodeId TypeId => ExpandedNodeId.Parse("ns=" + nsAutoID +";i=5030");
            public override ExpandedNodeId BinaryEncodingId => ExpandedNodeId.Parse("ns=" + nsAutoID + ";i=5030");
            public override ExpandedNodeId XmlEncodingId => NodeId.Null;

            public override void Decode(IDecoder decoder)
            {
                this.switchField = decoder.ReadUInt32("SwitchField");

                this.tagId = decoder.ReadByteString("ByteString");
                this.tagId = decoder.ReadString("String");
                this.tagId = decoder.ReadString("Epc");
                this.tagId = decoder.ReadString("Custom:TID");
            }

            public override void Encode(IEncoder encoder)
            {
                encoder.WriteUInt32("SwitchField", this.switchField);

                switch (this.switchField)
                {
                    case 0:
                        try
                        {
                            encoder.WriteByteString("ByteString", (byte[])this.tagId);
                        }catch(Exception ex)
                        {
                            Console.WriteLine("Exception: {0}", ex.Message);
                            return;
                        }
                        break;
                    case 1:
                        try
                        {
                            encoder.WriteString("String", (string)this.tagId);
                        }catch(Exception ex)
                        {
                            Console.WriteLine("Exception: {0}", ex.Message);
                            return;
                        }
                        break;
                    case 2:
                        try
                        {
                            encoder.WriteString("Epc", (string)this.tagId);
                        }catch(Exception ex)
                        {
                            Console.WriteLine("Exception: {0}", ex.Message);
                            return;
                        }
                        break;
                    case 3:
                        try
                        {
                            encoder.WriteString("Custom:TID", (string)this.tagId);
                        }catch(Exception ex)
                        {
                            Console.WriteLine("Exception: {0}", ex.Message);
                            return;
                        }
                        break;
                    default:
                        break;
                }
            }

            public override string ToString()
            {
                return String.Format("ScanData:\n\tSwitchField: {0}, TagId: {1}",
                    this.switchField, this.tagId.ToString());
            }
        }
    }
}