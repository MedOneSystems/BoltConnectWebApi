# BoltConnectWebApi
A .Net Standardard 2.0 library to provide access to the BOLTConnect HL7 message feed.

Available on nuget.  https://www.nuget.org/packages/BoltConnectWebApi/0.2.1

## Basic Usage

Definition of the single function this library provides

```c#
	public static async Task<bool> SendMessages(
		string subscriberId, 
		string customerId,
		IEnumerable<string> messages,  
		string baseUrl = "https://boltconnect.azure-api.net/boltconnecthl7ingest/StoreMessage", 
		int retryCount = 10, 
		int readWriteTimeout = 0, 
		int timeout = 0)
```

```c#
	
	var subid = "subscriberid"; // both provided by us
	var custid = "customerid"; // both provided by us
	msgs = new []{
		"MSH|^~\&|REG....",
		"MSH|^~\&|REG....",
		"MSH|^~\&|REG....",
		"MSH|^~\&|REG...."
	};

	var result = await BoltConnect.SendMessages(subid, custid, msgs);

	if (result) {
		Console.WriteLine("Sent");
	}

```

## A few notes

* Don't override the baseUrl that is defaulted in the parameters above.
* The retry count is done with an exponential delay.  Math.Pow(2,retryCount).  Each failure will wait a little bit longer.
* Timeouts are provided in seconds.  Defaults to RestSharps and HttpClients defaults.
* The backend this service talks to is an Azure function, and under certain conditions it can take a while to respond to the 
  first message.  Set retry and timeout values appropriately.
