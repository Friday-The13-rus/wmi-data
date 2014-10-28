using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WMI
{
	public static class ReaderWriterLockSlimExtensions
	{
		public static void WriteLock(this ReaderWriterLockSlim locker, Action action)
		{
			locker.EnterWriteLock();
			try
			{
				action();
			}
			finally
			{
				locker.ExitWriteLock();
			}
		}

		public static T ReadLock<T>(this ReaderWriterLockSlim locker, Func<T> func)
		{
			locker.EnterReadLock();
			try
			{
				return func();
			}
			finally
			{
				locker.ExitReadLock();
			}
		}
	}
}
