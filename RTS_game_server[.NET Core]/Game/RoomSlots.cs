using ConsoleMars2000Server.Abstractions;
using ConsoleMars2000Server.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Game
{
	class RoomSlots {

		class Slot {
			public Slot(int slotId) {
				SlotId = slotId;
			}

			public Player Player;
			public int SlotId { get; private set; }
		}

		class Player {
			public IClient Client { get; private set; }
			public int SlotID { get; set; }
			public Slot Slot { get; set; }
			public bool IsReady { get; internal set; }
			public int ColorIndex { get; internal set; } = -1;
			public int SubColorIndex { get; internal set; } = 0;

			public Player(IClient client) {
				Client = client;
			}
		}

		public RoomSlots(SlotChangedClientCallback onChangeSlot, SlotChangedReadyCallback onChangeReady) {
			_onChangeSlot = onChangeSlot;
			_onChangeReady = onChangeReady;
		}

		ConcurrentDictionary<int, Slot> _slots = new ConcurrentDictionary<int, Slot>();
		ConcurrentDictionary<IClient, Player> _players = new ConcurrentDictionary<IClient, Player>();
		HashSet<int> _colors = new HashSet<int>();

		ConcurrentQueue<Func<Task>> _eventQueue = new ConcurrentQueue<Func<Task>>();

		async Task RunEvents() {
			while (_eventQueue.TryDequeue(out var action)) await action();
		}


		private SemaphoreSlim _selectSlotSemaphore = new SemaphoreSlim(1);
		private SlotChangedClientCallback _onChangeSlot;
		private SlotChangedReadyCallback _onChangeReady;
		private volatile int _maxColorIndex;

		#region Private
		void SetPlayerInSlot(int slotId, Player player) {
			var slot = _slots.GetOrAdd(slotId, (id) => new Slot(id));
			if (slot.Player == null) {
				slot.Player = player;
				player.Slot = slot;
				player.SlotID = slotId;
				player.IsReady = false;
				if (player.ColorIndex == -1 || _colors.Contains(player.ColorIndex)) {
					for (int nextColor = 0; nextColor < _slots.Count; nextColor++) {
						if (!_colors.Contains(nextColor)) {
							_colors.Add(nextColor);
							player.ColorIndex = nextColor;
							break;
						}
					}
				} else {
					_colors.Add(player.ColorIndex);
				}
				_eventQueue.Enqueue(() => _onChangeSlot(slotId, player.Client, player.ColorIndex, player.SubColorIndex));
			}
		}

		void FreeSlotFromPlayer(Player player) {
			Slot slot = player.Slot;
			if (slot != null) {
				slot.Player = null;
				player.Slot = null;
				player.SlotID = -1;
				if (player.IsReady) {
					player.IsReady = false;
					_eventQueue.Enqueue(() => _onChangeReady(slot.SlotId, false));
				}
				_colors.Remove(player.ColorIndex);
				_eventQueue.Enqueue(() => _onChangeSlot(slot.SlotId, null, -1, 0));
			}
		}

		public async Task<bool> IsAllReady() {
			bool rezult = true;
			await _selectSlotSemaphore.Lock(() => {
				var slots = _slots.Values;
				foreach (var slot in slots) {
					if (slot.Player != null && !slot.Player.IsReady) {
						rezult = false;
						return;
					}
				}
			});
			return rezult;
		}

		Player GetPlayer(IClient playerClient) {
			return _players.GetOrAdd(playerClient, (pl) => new Player(playerClient));
		}

		Slot GetSlot(int slotId) {
			return _slots.GetOrAdd(slotId, (id) => new Slot(id));
		}

		bool IsClientInSlot(IClient client, int slotId) {
			if (_slots.TryGetValue(slotId, out var slot)) {
				var player = slot.Player;
				return (player.Client == client);
			}
			return false;
		}
		#endregion



		public async Task ClientSetColor(IClient client, int slotId, int colorIndex, int subColorIndex) {
			await _selectSlotSemaphore.Lock(() => {
				if (_slots.TryGetValue(slotId, out var slot)) {
					var player = slot.Player;
					if (player.Client == client) {
						if (player.ColorIndex == colorIndex && player.SubColorIndex != subColorIndex) {
							player.SubColorIndex = subColorIndex;
							_eventQueue.Enqueue(() => _onChangeSlot(slot.SlotId, slot.Player.Client, slot.Player.ColorIndex, slot.Player.SubColorIndex));
						}
						else {
							int startColorIndex = colorIndex;
							_maxColorIndex = Math.Max(colorIndex+1, _slots.Count);
							while (_colors.Contains(colorIndex)) {
								colorIndex++;
								subColorIndex = 0;
								if (startColorIndex == colorIndex) break;
								if (colorIndex >= _maxColorIndex) {
									colorIndex = 0;
								}
							}
							if (!_colors.Contains(colorIndex)) {
								_colors.Remove(player.ColorIndex);
								player.ColorIndex = colorIndex;
								_colors.Add(colorIndex);
								player.SubColorIndex = subColorIndex;
								_eventQueue.Enqueue(() => _onChangeSlot(slot.SlotId, slot.Player.Client, slot.Player.ColorIndex, slot.Player.SubColorIndex));
							}
						}
					}
				}
			});
			await RunEvents();
		}

		public int ClientSlot(IClient client) {
			if (_players.TryGetValue(client, out var player)) {
				return player.SlotID;
			}
			return -1;
		}

		public async Task ClientSelectSlot(IClient client, int slotId) {
			await _selectSlotSemaphore.Lock(() => {
				Player player;
				player = GetPlayer(client);
				FreeSlotFromPlayer(player);
				SetPlayerInSlot(slotId, player);
			});
			await RunEvents();
		}

		public async Task ClientSetReady(IClient client, int slotId, bool isReady) {
			await _selectSlotSemaphore.Lock(() => {
				var slot = GetSlot(slotId);
				var player = slot.Player;
				if (player!=null && player.Client == client) {
					player.IsReady = isReady;
					_eventQueue.Enqueue(() => _onChangeReady(slot.SlotId, isReady));
				}
			});
			await RunEvents();
		}

		public async Task GetAllStateCallback(SlotChangedClientCallback onChangeSlot, SlotChangedReadyCallback onChangeReady) {
			await _selectSlotSemaphore.Lock(() => {
				var values = _slots.Values;
				foreach (var slot in values) {
					var player = slot.Player;
					if (player != null) {
						_eventQueue.Enqueue(() => _onChangeSlot(slot.SlotId, slot.Player.Client, slot.Player.ColorIndex, slot.Player.SubColorIndex));
						if (player.IsReady) {
							_eventQueue.Enqueue(() => _onChangeReady(slot.SlotId, slot.Player.IsReady));
						}
					}
				}
			});
			await RunEvents();
		}

		public async Task RemoveClient(IClient client) {
			await _selectSlotSemaphore.Lock(() => {
				if (_players.TryRemove(client, out var player)) {
					FreeSlotFromPlayer(player);
				}
			});
			await RunEvents();
		}


		struct InternallCallbackRecord {
			public InternallCallbackRecord(int slotId, IClient client) {
				SlotId = slotId;
				Client = client;
			}

			public int SlotId;
			public IClient Client;
		}

		public delegate Task SlotChangedClientCallback(int slotId, IClient client, int colorIndex, int subColorIndex);
		public delegate Task SlotChangedReadyCallback(int slotId, bool isReady);
	}
}
