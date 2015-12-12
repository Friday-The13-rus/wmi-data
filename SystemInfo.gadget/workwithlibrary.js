var dllCLSID = "{A79AC85C-547C-3ED3-AD94-530DC4BBB672}";
var Classname = "WMI.DataReturner";
var LibPath = "file:///" + System.Gadget.path.replace(new RegExp("\\\\", "g"), "/") + "/WMI.dll"
var LibName = "WMI";
var Version = "1.0.0.4"
var oShell = new ActiveXObject("WScript.Shell");
var token = "7cc237d377f44c1e";
var runtimeVersion = "v4.0.30319";
var regRoot;

function RegisterLibrary() {
	var classRoot = regRoot + "\\Software\\Classes\\" + Classname + "\\";
	var clsidRoot = regRoot + "\\Software\\Classes\\CLSID\\" + dllCLSID + "\\";
	try {
		oShell.RegWrite(classRoot, Classname, "REG_SZ");
		oShell.RegWrite(classRoot + "CLSID\\", dllCLSID, "REG_SZ");
		oShell.RegWrite(clsidRoot, Classname, "REG_SZ");
		oShell.RegWrite(clsidRoot + "InprocServer32\\", "mscoree.dll", "REG_SZ");
		oShell.RegWrite(clsidRoot + "InprocServer32\\ThreadingModel", "Both", "REG_SZ");
		oShell.RegWrite(clsidRoot + "InprocServer32\\Class", Classname, "REG_SZ");
		oShell.RegWrite(clsidRoot + "InprocServer32\\Assembly", LibName + ", Version=" + Version + ", Culture=neutral, PublicKeyToken=" + token, "REG_SZ");
		oShell.RegWrite(clsidRoot + "InprocServer32\\RuntimeVersion", runtimeVersion, "REG_SZ");
		oShell.RegWrite(clsidRoot + "InprocServer32\\CodeBase", LibPath, "REG_SZ");
		oShell.RegWrite(clsidRoot + "InprocServer32\\" + Version + "\\Class", Classname, "REG_SZ");
		oShell.RegWrite(clsidRoot + "InprocServer32\\" + Version + "\\Assembly", LibName + ", Version=" + Version + ", Culture=neutral, PublicKeyToken=" + token, "REG_SZ");
		oShell.RegWrite(clsidRoot + "InprocServer32\\" + Version + "\\RuntimeVersion", runtimeVersion, "REG_SZ");
		oShell.RegWrite(clsidRoot + "InprocServer32\\" + Version + "\\CodeBase", LibPath, "REG_SZ");
		oShell.RegWrite(clsidRoot + "ProgId\\", Classname, "REG_SZ");
		oShell.RegWrite(clsidRoot + "Implemented Categories\\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}\\", "", "REG_SZ");
	}
	catch (err) {
		return null;
	}
}

function UnregisterLibrary() {
	var classRoot = regRoot + "\\Software\\Classes\\" + Classname + "\\";
	var clsidRoot = regRoot + "\\Software\\Classes\\CLSID\\" + dllCLSID + "\\";
	try {
		oShell.RegDelete(clsidRoot + "Implemented Categories\\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}\\");
		oShell.RegDelete(clsidRoot + "Implemented Categories\\");
		oShell.RegDelete(clsidRoot + "ProgId\\");
		oShell.RegDelete(clsidRoot + "InprocServer32\\" + Version + "\\");
		oShell.RegDelete(clsidRoot + "InprocServer32\\");
		oShell.RegDelete(clsidRoot);
		oShell.RegDelete(classRoot + "CLSID\\");
		oShell.RegDelete(classRoot);
	}
	catch (err) {
	}
}

function IsLibraryRegistred(){
	var classRoot = regRoot + "\\Software\\Classes\\" + Classname + "\\";
	try {
		return oShell.RegRead(classRoot) == className;
	}
	catch (err){
		return false;
	}
}

function ActivateLibrary(root) {
	regRoot = root;
	try {
		if (!IsLibraryRegistred())
			RegisterLibrary();
		return new ActiveXObject(Classname);
	}
	catch (err) {
		UnregisterLibrary();
		throw err;
	}
}

function GetLibrary() {
	var Lib;
	Lib = ActivateLibrary("HKCU");
	if (Lib == null)
		Lib = ActivateLibrary("HKLM");
	return Lib;
}