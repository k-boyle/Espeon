using System.Collections.Generic;

namespace Espeon.Core.Databases {
	public class ModuleInfo {
		public string Name { get; set; }

		public List<string> Aliases { get; set; }
		public List<CommandInfo> Commands { get; set; }
	}

	public class CommandInfo {
		public ModuleInfo Module { get; set; }
		public string ModuleName { get; set; }

		public string Name { get; set; }
		public List<string> Aliases { get; set; }
	}
}