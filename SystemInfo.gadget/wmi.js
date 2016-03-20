function WmiObj() {
	this.NetLib = {};

	var StopTimer = function () {
		clearInterval(timerId);
	}
	
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