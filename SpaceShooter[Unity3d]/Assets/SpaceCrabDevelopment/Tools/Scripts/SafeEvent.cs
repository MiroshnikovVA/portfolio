using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceCrabDevelopment.Tools {


	namespace Internal {
		public abstract class SafeEventBase<TCallback> {

			protected struct ListenerPair {
				public ListenerPair(MonoBehaviour listener, TCallback callback) {
					Listener = listener;
					Callback = callback;
				}
				public MonoBehaviour Listener;
				public TCallback Callback;
			}

			protected LinkedList<ListenerPair> Listeners = new LinkedList<ListenerPair>();

			public void AddListener(MonoBehaviour listener, TCallback callback) {
				if (!listener) throw new ArgumentNullException("listener");
				if (callback == null) throw new ArgumentNullException("callback");
				Listeners.AddFirst(new ListenerPair(listener, callback));
			}

			public void RemoveListener(MonoBehaviour listener) {
				var cur = Listeners.First;
				while (cur != null) {
					var pair = cur.Value;
					if (!pair.Listener || pair.Listener == listener) {
						var remov = cur;
						cur = cur.Next;
						Listeners.Remove(remov);
					}
					else {
						cur = cur.Next;
					}
				}
			}

			protected void RemoveOld() {
				var cur = Listeners.First;
				while (cur != null) {
					var pair = cur.Value;
					if (!pair.Listener) {
						var remov = cur;
						cur = cur.Next;
						Listeners.Remove(remov);
					}
					else {
						cur = cur.Next;
					}
				}
			}
		}
	}


	public class SafeEvent : Internal.SafeEventBase<SafeEvent.EventAction> {

		public delegate void EventAction();

		public void Invoke() {
			RemoveOld();
			var cur = Listeners.First;
			while (cur != null) {
				cur.Value.Callback();
				cur = cur.Next;
			}
		}
	}

	public class SafeEvent<T> : Internal.SafeEventBase<SafeEvent<T>.EventAction> {

		public delegate void EventAction(T arg);

		public void Invoke(T arg) {
			RemoveOld();
			var cur = Listeners.First;
			while (cur != null) {
				cur.Value.Callback(arg);
				cur = cur.Next;
			}
		}
	}

	public class SafeEvent<T1, T2> : Internal.SafeEventBase<SafeEvent<T1, T2>.EventAction> {

		public delegate void EventAction(T1 arg1, T2 arg2);

		public void Invoke(T1 arg1, T2 arg2) {
			RemoveOld();
			var cur = Listeners.First;
			while (cur != null) {
				cur.Value.Callback(arg1, arg2);
				cur = cur.Next;
			}
		}
	}
}
