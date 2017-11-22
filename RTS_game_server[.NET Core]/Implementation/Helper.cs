using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Implementation {
	static class Helper {
		public static void AddOffset(ref ArraySegment<byte> old, int offset) {
			old = new ArraySegment<byte>(old.Array, old.Offset + offset, old.Count - offset);
		}

		public static async Task Lock(this System.Threading.SemaphoreSlim semaphore, Action action) {
			try {
				await semaphore.WaitAsync();
				action();
			}
			finally {
				semaphore.Release();
			}
		}

		public static async Task Lock(this System.Threading.SemaphoreSlim semaphore, Func<Task> action) {
			try {
				await semaphore.WaitAsync();
				await action();
			}
			finally {
				semaphore.Release();
			}
		}

	}
}
