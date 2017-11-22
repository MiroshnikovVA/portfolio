using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Implementation {
	public class ActionQueue {

		public static void TestDeadLock() {
			var a = new ActionQueue();
			var b = new ActionQueue();

			a.Enqueue(() =>
				a.Enqueue(() => {
					Console.WriteLine("!!");
				})	
			);

			OneLockTest(a, b, "A", "B");
			OneLockTest(b, a, "B", "A");

			void OneLockTest(ActionQueue x, ActionQueue y, string xName, string yName) {
				Console.WriteLine($"1 {xName}");
				x.Enqueue(async () => {
					Console.WriteLine($"2 {xName}");
					await Task.Delay(1000);
					Console.WriteLine($"3 {xName}");
					Console.WriteLine($"{xName} wait {yName} lock");
					y.Enqueue(async () => {
						Console.WriteLine($"4 {xName}.{yName}");
						await Task.Delay(100);
						Console.WriteLine($"5 {xName}.{yName}");
						await Task.Delay(100);
						Console.WriteLine($"6 {xName}.{yName}");
					});
					Console.WriteLine($"7 {xName}");
				});
				Console.WriteLine($"8 {xName}");
			}
		}

		ConcurrentQueue<Adapter> _queue = new ConcurrentQueue<Adapter>();
		SemaphoreSlim _workingSemaphore = new SemaphoreSlim(1);
		private volatile bool _working;

		public struct Adapter {
			public Func<Task> AsyncAction;
			public Action<Exception> Exception;

			public SemaphoreSlim Semaphore;
		}

		public void Enqueue(Func<Task> asyncAction, Action<Exception> onException = null) {
			var adapter = new Adapter() {
				AsyncAction = asyncAction,
				Exception = onException
			};
			_queue.Enqueue(adapter);
			var noAwait = QueueWork();
		}

		public async Task EnqueueLock(Func<Task> asyncAction) {
			Exception exception = null;
			var adapter = new Adapter() {
				AsyncAction = asyncAction,
				Exception = (e) => exception = e,
				Semaphore = new SemaphoreSlim(0)
			};
			_queue.Enqueue(adapter);
			var noAwait = QueueWork();
			await adapter.Semaphore.WaitAsync();
			if (exception != null) throw exception;
		}

		async Task QueueWork() {
			if (await TryGetWorker()) {
				while (_queue.TryDequeue(out var adapter)) {
					try {
						await adapter.AsyncAction();
					}
					catch (Exception excep) {
						await ReleseWorker();
						adapter.Exception?.Invoke(excep);
					}
					if (adapter.Semaphore != null)
						adapter.Semaphore.Release();
				}
				await ReleseWorker();
				if (!_queue.IsEmpty) await QueueWork();
			}
		}

		async Task<bool> TryGetWorker() {
			try {
				await _workingSemaphore.WaitAsync();
				if (!_working) {
					_working = true;
					return true;
				}
				else {
					return false;
				}
			}
			finally {
				_workingSemaphore.Release();
			}
		}

		async Task ReleseWorker() {
			try {
				await _workingSemaphore.WaitAsync();
				_working = false;
			}
			finally {
				_workingSemaphore.Release();
			}
		}

		public Task EnqueueLock(Action action) {
			Func<Task> f = () => {
				action();
				return Task.CompletedTask;
			};
			return EnqueueLock(f);
		}

		public void Enqueue(Action action, Action<Exception> onException = null) {
			Func<Task> f = () => {
				action();
				return Task.CompletedTask;
			};
			Enqueue(f, onException);
		}
	}
}
