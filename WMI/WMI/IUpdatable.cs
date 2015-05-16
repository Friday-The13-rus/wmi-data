using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WMI
{
	interface IUpdatable<in T>
	{
		void Update(T other);
	}
}
