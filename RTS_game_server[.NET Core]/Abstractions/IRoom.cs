using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMars2000Server.Abstractions {
	public interface IRoom {
		Task<bool> TryEnter(IClient clientState);

		Task<bool> TryExit(IClient clientState);

		int Id { get; }
	}

	public interface IHostRoom : IRoom {
		void OnChildrenRoomClosed(IRoom childrenRoom);
	}
}
