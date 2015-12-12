"use strict";

function SettingsIO() {
	var NetworkAdapterSettingsKey = "NetworkAdapter";

	this.Read = function() {
		return { NetworkAdapter: System.Gadget.Settings.read(NetworkAdapterSettingsKey) };
	}
	
	this.Write = function(setting) {
		System.Gadget.Settings.write(NetworkAdapterSettingsKey, setting.NetworkAdapter);
	}
}