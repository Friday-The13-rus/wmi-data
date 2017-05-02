using System.Collections.Generic;

namespace WMI
{
	internal class TrimEndSlashesComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			if (x == null)
				return -1;

			if (y == null)
				return 1;

			return string.CompareOrdinal(x.TrimEnd('\\'), y.TrimEnd('\\'));
		}
	}
}
