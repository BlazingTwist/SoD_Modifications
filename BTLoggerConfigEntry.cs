using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class BTLoggerConfigEntry{
	public bool logMessage = true;
	public bool logStackTrace = false;
	
	public bool AnythingToLog(){
		return logMessage;
	}
}