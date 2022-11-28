using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;

namespace UA_Client_RFU6xx
{
    public abstract class UAClientBase
    {
        public UAClientBase(ApplicationConfiguration configuration, Action<IList, IList> validateResponse)
        {
            m_validateResponse = validateResponse;
            m_configuration = configuration;
            m_configuration.CertificateValidator.CertificateValidation += CertificateValidation;
        }

        #region Public Properties
        /// <summary>
        /// Gets the client session.
        /// </summary>
        public Session Session => m_session;

        /// <summary>
        /// Gets or sets the server URL.
        /// </summary>
        public string ServerUrl { get; set; } = "opc.tcp://localhost:4840";
        #endregion



        #region Public Methods
        /// <summary>
        /// Creates a session with the UA server
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (m_session != null && m_session.Connected == true)
                {
                    Console.WriteLine("Session already connected!");
                }
                else
                {
                    Console.WriteLine("Connecting...");

                    // Get the endpoint by connecting to server's discovery endpoint.
                    // Try to find the first endpoint without security.
                    EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(m_configuration, discoveryUrl: ServerUrl, useSecurity: false);
                    EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(m_configuration);
                    ConfiguredEndpoint endpoint = new ConfiguredEndpoint(collection: null, endpointDescription, endpointConfiguration);

                    // Create the session
                    Session session = await Session.Create(
                        configuration: m_configuration,
                        endpoint: endpoint,
                        updateBeforeConnect: false,
                        checkDomain: false,
                        sessionName: m_configuration.ApplicationName,
                        sessionTimeout: 30 * 60 * 1000,
                        identity: new UserIdentity(),
                        preferredLocales: null
                    );

                    // Assign the created session
                    if (session != null && session.Connected)
                    {
                        m_session = session;

                        onConnect();
                    }

                    // Session created successfully.
                    Console.WriteLine($"New Session Created with SessionName = {m_session.SessionName}");
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log Error
                Console.WriteLine($"Create Session Error : {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Executed when successfully created session.
        /// </summary>
        public abstract void onConnect();

        /// <summary>
        /// Disconnects the session.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (m_session != null)
                {
                    Console.WriteLine("\nDisconnecting...");

                    m_session.Close();
                    m_session.Dispose();
                    m_session = null;

                    // Log Session Disconnected event
                    Console.WriteLine("Session Disconnected.");
                }
                else
                {
                    Console.WriteLine("Session not created!");
                }
            }
            catch (Exception ex)
            {
                // Log Error
                Console.WriteLine($"Disconnect Error : {ex.Message}");
            }
        }

        /// <summary>Reads a node from the server.</summary>
        /// <returns>The data values read from the specified node. Null if failed.</returns>
        /// <param name="nodeId">The node id to be read.</param>
        /// <param name="attributeId">
        /// The attribute to read.
        /// <example>For example:
        /// <code>Attributes.Value</code>
        /// </example>
        /// </param>
        public DataValue ReadNode(NodeId nodeId, uint attributeId)
        {
            // Check if connected to server
            if (m_session == null || m_session.Connected == false)
            {
                Console.WriteLine("Session not connected!");
                return null;
            }

            try
            {
                #region Read a node by calling the Read Service

                // Build a list of nodes to be read (in this case only one node)
                ReadValueIdCollection nodesToRead = new ReadValueIdCollection()
                {
                    new ReadValueId() { NodeId = nodeId, AttributeId = attributeId}
                };

                Console.WriteLine("Reading node...");

                // Call Read Service
                m_session.Read(
                    null,
                    0,
                    TimestampsToReturn.Both,
                    nodesToRead,
                    out DataValueCollection resultsValues,
                    out DiagnosticInfoCollection diagnosticInfos);

                // Validate the results
                m_validateResponse(resultsValues, nodesToRead);

                // Return the first element of DataValueCollection 'resultsValues' since it is the only element
                return resultsValues[0];
                #endregion
            }
            catch (Exception ex)
            {
                // Log Error
                Console.WriteLine($"Read Nodes Error : {ex.Message}.");

                // Return null since there was an error
                return null;
            }
        }

        /// <summary>Calls a UA method.</summary>
        /// <returns>The list of output argument values.</returns>
        /// <param name="inputArgs">The input arguments as an object array.</param>
        public IList<object> CallMethod(NodeId parentId, NodeId methodId, object[] inputArgs)
        {
            // Check if connected to server
            if (m_session == null || m_session.Connected == false)
            {
                Console.WriteLine("Session not connected!");
                return null;
            }

            try
            {
                IList<object> outputArguments = null;

                // Invoke Call service
                Console.WriteLine("Calling UAMethod for node {0} ...", methodId);
                
                outputArguments = m_session.Call(parentId, methodId, inputArgs);
                
                // Display amount of output arguments
                Console.WriteLine("Method call returned {0} output argument(s)", outputArguments.Count);

                return outputArguments;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine("Method call error: {0}", ex.Message);

                // Return null since there was an error
                return null;
            }
        }
        #endregion



        #region Private Methods

        /// <summary>
        /// Handle DataChange notifications from Server
        /// </summary>
        private void OnMonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                // Log MonitoredItem Notification event
                MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
                Console.WriteLine("Notification Received for Variable \"{0}\" and Value = {1}.", monitoredItem.DisplayName, notification.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnMonitoredItemNotification error: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Handles the certificate validation event.
        /// This event is triggered every time an untrusted certificate is received from the server.
        /// </summary>
        private void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            bool certificateAccepted = true;

            // ****
            // Implement a custom logic to decide if the certificate should be
            // accepted or not and set certificateAccepted flag accordingly.
            // The certificate can be retrieved from the e.Certificate field
            // ***

            ServiceResult error = e.Error;
            while (error != null)
            {
                Console.WriteLine(error);
                error = error.InnerResult;
            }

            if (certificateAccepted)
            {
                Console.WriteLine("Untrusted Certificate accepted. SubjectName = {0}", e.Certificate.SubjectName);
            }

            e.AcceptAll = certificateAccepted;
        }
        #endregion



        #region Private Fields

        private ApplicationConfiguration m_configuration;
        private Session m_session;
        private readonly Action<IList, IList> m_validateResponse;

        #endregion
    }
}