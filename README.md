# OPC UA .NET RFU6xx Client Example

Example controlling a SICK RFU6xx with OPC UA via the .net reference stack.

Author: Richard Rehan

This is a C# example project on how to implement an OPC UA client for the RFU6xx RFID scanners by SICK.

<br/>

## Installation

For this project the [.NET Core SDK 3.1](https://dotnet.microsoft.com/en-us/download/dotnet/3.1) should be installed.

The only NuGet package needed is the official OPC UA .NET Standard package by the OPC Foundation:

* `OPCFoundation.NetStandard.Opc.Ua`

<br/>

## Configuration

To connect this client to your RFU6xx server, you have to adjust the discovery URL after initializing the RFU6xxClient in `Program.cs`.

```csharp
// Set the Discovery URL of the Client
rfuClient.ServerUrl = "opc.tcp://localhost:4840";
```

<br/>

## Usage

### Starting the client

To start the client, execute this command in the root directory of this project:
```
dotnet run -p UA-Client-RFU6xx.csproj
```

After the client successfully connects to the server, it calls some methods on the RFU6xx.

This can be found in `Program.cs`. In this example it:

* starts the scan process to scan for tags (ScanStart),
* stops the scan process (ScanStop),
* gets the last scanned tag (LastScanData)
* tries to read some data on that tag (ReadTag) and
* writes data on that tag (WriteTag).

<br/>

More information is provided in the code.

<br/>

## License
[MIT](https://choosealicense.com/licenses/mit/)