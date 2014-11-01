onerror = Error;
execScript('Function Message(prompt, title)\r\n'
	+ ' Message = MsgBox(prompt, 16, title)\r\n'
	+ 'End Function', "vbscript");

var rowHeight = 14;
var widthBar = 80;

var CpuData = {};
var RamData = {};
var HddData = {};
var NetData = {};

function CpuObj() {
	this.cores = [];
	this.allCores = 0;
	this.CoresCount = function () {
		return this.cores.length;
	}

	for (var i = 0; i < System.Machine.CPUs.count; i++) {
		this.cores[i] = 0;
		cpuDiv.innerHTML +=
			'<div id="Core' + i + '" style="top:' + (i * rowHeight) + 'px; left:3px; width:30px">0 %</div>' +
			'<div style="top:' + (i * rowHeight) + 'px; width:' + widthBar + 'px; margin: 2px 0 0 35px;">' +
				'<img src="image/bars/back1.png" style="width:' + widthBar + 'px; height:8px; border: 1px solid #111111;"/>' +
				'<img id="Core' + i + 'Width" src="image/bars/cpu1.png" style="width:0px; height:8px; top:1px; left:1px;"/>' +
			'</div>';
	}
	totalPercent.innerHTML = 0 + ' %';

	this.Update = function () {
		var j = 0;
		for (var i = 0; i < NetLib.GetCoresCount(); i++) {
			var temp = NetLib.GetProcessorData(i);

			if (temp.name != "_Total") {
				this.cores[j] = parseInt(temp.usePercent);
				j++;
			}
			else {
				this.allCores = parseInt(temp.usePercent);
			}
		}
	}

	this.UpdateWithoutWMI = function () {
		var tempAllCores = 0;
		for (var i = 0; i < System.Machine.CPUs.count; i++) {
			var tempPercent = Math.min(Math.max(System.Machine.CPUs.item(i).usagePercentage, 0), 100);
			this.cores[i] = parseInt(tempPercent);
			tempAllCores += parseInt(tempPercent);
		}
		this.allCores = Math.round(tempAllCores / this.CoresCount());
	}

	this.Draw = function () {
		document.getElementById('totalPercent').innerHTML = this.allCores + ' %';
		for (i = 0; i < this.CoresCount() ; i++) {
			document.getElementById('Core' + i).innerHTML = this.cores[i] + ' %';
			document.getElementById('Core' + i + 'Width').style.width = CalcWidthBar(this.cores[i]);
		}
	}
}

function RamObj() {
	this.free = 0;
	this.total = System.Machine.totalMemory;
	this.use = 0;
	this.percentUse = 0;

	useRam.innerHTML = '0 Мб';
	ramDiv.innerHTML =
		'<div id="percentUseRam" style="top:' + (0 * rowHeight) + 'px; left:3px; width:30px">0 %</div>' +
		'<div style="top:' + (0 * rowHeight) + 'px; width:' + widthBar + 'px; margin: 2px 0 0 35px;">' +
			'<img src="image/bars/back1.png" style="width:' + widthBar + 'px; height:8px; border: 1px solid #111111;"/>' +
			'<img id="percentUseRamWidth" src="image/bars/ram1.png" style="width:0px; height:8px; top:1px; left:1px;"/>' +
		'</div>';

	this.Update = function () {
		this.free = System.Machine.availableMemory;
		this.use = this.total - this.free;
		this.percentUse = (100 * this.use / this.total).toFixed();
	}

	this.Draw = function () {
		var useRamTemp = formatBytes(this.use, 'mb');
		document.getElementById('useRam').innerHTML = useRamTemp;
		document.getElementById('percentUseRam').innerHTML = this.percentUse + ' %';
		document.getElementById('percentUseRamWidth').style.width = CalcWidthBar(this.percentUse);
		ramSect.title =
			'Всего памяти: ' + formatBytes(this.total, 'mb') + '\r\n' +
			'Занято памяти: ' + useRamTemp + '\r\n' +
			'Свободно памяти: ' + formatBytes(this.free, 'mb')
	}
}

function DriveObj() {
	this.drives = [];
	this.DrivesCount = function () {
		return this.drives.length;
	}

	var Drive = function () {
		this.Name = 'null:';
		this.VolumeName = 'null';
		this.FreeSpace = '0 байт';
		this.UseSpace = '0 байт';
		this.Space = '0 байт';
		this.ActivePercent = 0;
		this.UsePercent = 0;
	}

	var PaintDrive = function (i) {
		hddDiv.innerHTML +=
			'<img style="top:' + ((i * 3 + 0) * rowHeight + 3 * (i - 1)) + 'px; left:-1px;" class="divider" src="image/horizontalDivider.png" alt=""/>' +
			'<div id="Drive' + i + '" style="top:' + (3 * i * (rowHeight + 1)) + 'px; left:0px; width:120px; height:' + (3 * rowHeight) + 'px; ">' +
				'<div id="Drive' + i + 'Name" style="top:' + 0 * rowHeight + 'px; left:3px; text-overflow: ellipsis; overflow: hidden; width: 65px;">null</div>' +
				'<div id="Drive' + i + 'FreeSpace" style="top:' + 0 * rowHeight + 'px; right:4px;">0</div>' +
				'<div id="Drive' + i + 'UsePerc" style="top:' + 1 * rowHeight + 'px; left:3px; width:30px">0 %</div>' +
				'<div style="top:' + 1 * rowHeight + 'px; width:' + widthBar + 'px; margin: 2px 0 0 35px;">' +
					'<img src="image/bars/back1.png" style="width:' + widthBar + 'px; height:8px; border: 1px solid #111111;"/>' +
					'<img id="Drive' + i + 'UsePercWidth" src="image/bars/hdd1.png" style="width:0px; height:8px; top:1px; left:1px;"/>' +
				'</div>' +
				'<div id="Drive' + i + 'ActivePercent" style="top:' + 2 * rowHeight + 'px; left:3px; width:30px">0 %</div>' +
				'<div style="top:' + 2 * rowHeight + 'px; width:' + widthBar + 'px; margin: 2px 0 0 35px;">' +
					'<img src="image/bars/back1.png" style="width:' + widthBar + 'px; height:8px; border: 1px solid #111111;"/>' +
					'<img id="Drive' + i + 'ActivePercentWidth" src="image/bars/hdd2.png" style="width:0px; height:8px; top:1px; left:1px;"/>' +
				'</div>' +
			'</div>';
	}

	var AvailDrivesCount = function () {
		var validLetters = 0;
		var letters = 'ABCDEFJGHGKLMNOPQRSTUVWXYZ';

		for (var i = 0; i < letters.length; i++)
			try {
				if (System.Shell.drive(letters.charAt(i)).isReady) {
					validLetters++;
				}
			}
			catch (e) {
			}

		return validLetters;
	}

	for (i = 0; i < AvailDrivesCount() ; i++) {
		this.drives[i] = new Drive();
		PaintDrive(i);
	}

	this.Update = function () {
		var drivesCount = NetLib.GetDrivesCount();
		if (this.DrivesCount() > drivesCount) {
			hddDiv.innerHTML = '';
			this.drives = [];

			for (var i = 0; i < this.DrivesCount() ; i++) {
				this.drives[i] = new Drive();
				PaintDrive(i);
			}
		}
		else if (this.DrivesCount() < drivesCount) {
			for (var i = this.DrivesCount() ; i < drivesCount; i++) {
				this.drives[i] = new Drive();
				PaintDrive(i);
			}
		}

		for (var i = 0; i < this.DrivesCount() ; i++) {
			var temp = NetLib.GetDriveData(i);

			this.drives[i].Name = temp.name;
			this.drives[i].FreeSpace = formatBytes(temp.freeSpace, 'b');
			this.drives[i].Space = formatBytes(temp.space, 'b');
			this.drives[i].UseSpace = formatBytes(temp.space - temp.freeSpace, 'b');
			this.drives[i].VolumeName = temp.volumeName;
			this.drives[i].UsePercent = temp.usePercent;
			this.drives[i].ActivePercent = temp.activePercent;
		}
	}

	this.Draw = function () {
		for (i = 0; i < this.DrivesCount() ; i++) {
			var drive = document.getElementById('Drive' + i);
			var driveName = this.drives[i].Name + ' ' + this.drives[i].VolumeName;
			drive.onclick = OpenDrive;
			drive.title = driveName + '\r\n' +
				'Всего места: ' + this.drives[i].Space + '\r\n' +
				'Занято места: ' + this.drives[i].UseSpace + '\r\n' +
				'Свободно места: ' + this.drives[i].FreeSpace;

			var nameDiv = document.getElementById('Drive' + i + 'Name');
			var spaceDiv = document.getElementById('Drive' + i + 'FreeSpace');

			nameDiv.innerHTML = driveName;
			spaceDiv.innerHTML = this.drives[i].FreeSpace;
			nameDiv.style.width = 120 - 12 - spaceDiv.offsetWidth + 'px';
			document.getElementById('Drive' + i + 'UsePerc').innerHTML = this.drives[i].UsePercent + ' %';
			document.getElementById('Drive' + i + 'UsePercWidth').style.width = CalcWidthBar(this.drives[i].UsePercent);
			document.getElementById('Drive' + i + 'ActivePercent').innerHTML = this.drives[i].ActivePercent + ' %';
			document.getElementById('Drive' + i + 'ActivePercentWidth').style.width = CalcWidthBar(this.drives[i].ActivePercent);
		}
	}
}

function NetObj() {
	this.received = '0 байт';
	this.sent = '0 байт';

	networkDiv.innerHTML =
		'<div style="top:' + (0 * rowHeight) + 'px; left:3px;">' +
			'<img src="image/down.png">' +
			'<div id="NetReceived" style="left:13px; width:45px">0 байт</div>' +
		'</div>' +
		'<div style="top:' + (0 * rowHeight) + 'px; left:60px;">' +
			'<img src="image/up.png">' +
			'<div id="NetSent" style="left:13px; width:45px">0 байт</div>' +
		'</div>';

	this.Update = function () {
		var temp = NetLib.GetNetworkData();
		this.received = formatBytes(temp.received, 'b');
		this.sent = formatBytes(temp.sent, 'b');
	}

	this.Draw = function () {
		document.getElementById('NetReceived').innerHTML = this.received;
		document.getElementById('NetSent').innerHTML = this.sent;
	}
}

function Start() {
	//debugger;
	CpuData = new CpuObj();
	RamData = new RamObj();
	NetData = new NetObj();
	HddData = new DriveObj();
	CalculateHeight();

	NetLib = GetLibrary();
	NetLib.Start();

	setTimeout(function () { setInterval(Update, 1000) }, 5000);
}

function Error(message, source, lineno) {
	var line = 'File: ' + source + '\r\nLine ' + lineno + '\r\n' +
		'Message:' + message;
	System.Diagnostics.EventLog.writeEntry(line, 1);

	Message(line, 'Error!');
}

function Stop() {
	NetLib.Stop();
	UnregisterLibrary();
}

function Update() {
	CpuData.Update();
	RamData.Update();
	HddData.Update();
	NetData.Update();

	DisplayData();
}

function DisplayData() {
	CpuData.Draw();
	RamData.Draw();
	HddData.Draw();
	NetData.Draw();
	CalculateHeight();
}

function formatBytes(bytes, size) {
	if (size == "b") {
		if (bytes > 1073741824) return (bytes / 1073741824).toFixed(2) + ' Гб';
		if (bytes > 1048576) return (bytes / 1048576).toFixed(2) + ' Мб';
		if (bytes > 1024) return (bytes / 1024).toFixed(2) + ' Кб';
		return bytes + ' байт';
	}
	if (size == 'kb') {
		if (bytes > 1048576) return (bytes / 1048576).toFixed(2) + ' Гб';
		if (bytes > 1024) return (bytes / 1024).toFixed(2) + ' Мб';
		return bytes + ' Кб';
	}
	if (size == 'mb') {
		if (bytes > 1024) return (bytes / 1024).toFixed(2) + ' Гб';
		return bytes + ' Мб';
	}
}

function CalculateHeight() {
	cpuSect.style.height = 18 + CpuData.CoresCount() * rowHeight;
	ramSect.style.height = 18 + 1 * rowHeight;
	hddSect.style.height = 3 + HddData.DrivesCount() * rowHeight * 3 + 3 * (HddData.DrivesCount() - 1);
	networkSect.style.height = 18 + 1 * rowHeight;
	document.body.style.height = parseInt(cpuSect.style.height) + parseInt(ramSect.style.height) + parseInt(hddSect.style.height) + parseInt(networkSect.style.height) + 3;
	bottom.style.top = parseInt(document.body.style.height) - 10;
	middle.style.height = parseInt(document.body.style.height) - 20;
}

function CalcWidthBar(percent) {
	return Math.round(percent * widthBar / 100);
}

function OpenDrive() {
	System.Shell.execute(this.outerText.slice(0, 2));
}