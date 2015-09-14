using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WMI
{
	class PropertySettersDictionary<T> : Dictionary<string, Action<T, object>>
	{
	}
}
