using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storm
{
	[Flags]
	public enum StormPersistenceEvents
	{
		None = 0,
		Load = 1,
		Insert = 2,
		Update = 4,
		Delete = 8,
	}
}
