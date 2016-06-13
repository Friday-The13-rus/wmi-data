using System;
using System.Collections.Generic;

namespace WMI
{
	internal class CanonicalComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			if (x == null)
				return -1;

			if (y == null)
				return 1;

			var lengthDifference = x.Length - y.Length;
			if (lengthDifference != 0)
				return lengthDifference;

			for (int i = 0; i < x.Length; i++)
			{
				var xChar = x[i];
				var yChar = y[i];
				if (!Char.IsLetterOrDigit(xChar) && !Char.IsLetterOrDigit(yChar))
					continue;
				var compareResult = xChar - yChar;
				if (compareResult == 0)
					continue;
				return compareResult;
			}
			return 0;
		}
	}
}
