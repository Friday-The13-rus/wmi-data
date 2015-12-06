using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WMI
{
	public static class ReaderWriterLockSlimExtensions
	{
		public class LockController : IDisposable
		{
			private readonly ReaderWriterLockSlim _locker;
			private readonly bool _isReadLock;

			public LockController(ReaderWriterLockSlim locker, bool isReadLock)
			{
				_locker = locker;
				_isReadLock = isReadLock;
			}

			public void Dispose()
			{
				if (_isReadLock)
					_locker.ExitReadLock();
				else
					_locker.ExitWriteLock();
			}
		}

		public static LockController WriteLock(this ReaderWriterLockSlim locker)
		{
			return new LockController(locker, false);
		}

		public static LockController ReadLock(this ReaderWriterLockSlim locker)
		{
			return new LockController(locker, true);
		}

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
