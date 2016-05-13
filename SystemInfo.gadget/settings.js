"use strict";

System.Gadget.onSettingsClosing = SettingsClosing;

var Settings = {};
var wmi;

function Load() {
	wmi = new WmiObj();
	wmi.Start();
	
	Settings = new SettingsObj();
	Settings.Load();
}

function UnLoad() {
	wmi.Stop(false);
}

function SettingsClosing(event){
	if (event.closeAction == event.Action.commit) 
		Settings.Save();
	event.cancel = false;
}

function SelectorManager(selector) {
	var selector = selector;

	var ClearFocus = function () {
		selector.blur();
	}
	
	this.Add = function (text){
		selector.options.add(new Option(text));
	}
	
	this.GetSelectedElementText = function() {
		return selector.options[selector.options.selectedIndex].text;
	}
	
	this.SetSelectedIndex = function(index) {
		selector.options.selectedIndex = index;
		ClearFocus();
	}
	
	this.Enable = function () {
		selector.disabled = false;
	}
	
	this.Disable = function () {
		selector.disabled = true;
	}
	
	this.Disable();
	ClearFocus();
}

function SettingsObj() {
	var settingsIO = new SettingsIO();
	var adaptersManager = new SelectorManager(InternetAdapters);;
	
	var LoadAdapters = function () {
		var networkAdapters = wmi.NetLib.GetNetworkData();
		var adapters = [];
		for (var i = 0; i < networkAdapters.length; i++) {
			adapters.push(networkAdapters[i].Name);
		}
		return adapters;
	}
	
	this.SelectedAdapter = function () {
		return settingsIO.Read().NetworkAdapter;
	}

	var LoadInternal = function () {		
		var adapters = LoadAdapters();
		if (adapters.length == 0) {
			return false;
		}
		
		var selectedAdapter = this.SelectedAdapter();
		var selectedElementIndex = 0;
		
		for (var i = 0; i < adapters.length; i++) {
			adaptersManager.Add(adapters[i]);
			if (selectedAdapter == adapters[i]) {
				selectedElementIndex = i;
			}
		}
		
		adaptersManager.SetSelectedIndex(selectedElementIndex);
		adaptersManager.Enable();
		return true;
	}
	
	this.Load = function () {
		var context = this;
		var timerId = setInterval( function () {
			var isSucces = LoadInternal.call(context);
			if (isSucces) {
				clearInterval(timerId)
			}
		}, 1000);
	}
	
	this.Save = function() {
		settingsIO.Write({ NetworkAdapter: adaptersManager.GetSelectedElementText() });
	}
}