function WmiObj() {

	//Stub for autocomplete
	this.NetLib = {
		GetCpuData: function () { throw new Error("Not implemented"); },
		GetDriveData: function () { throw new Error("Not implemented"); },
		GetNetworkData: function () { throw new Error("Not implemented"); }
	};
	
	this.Start = function () {
		this.NetLib = GetLibrary();
		this.NetLib.Start();
	}
	
	this.Stop = function (unregister) {
		this.NetLib.Stop();
		if (unregister)
			UnregisterLibrary();
	}
}