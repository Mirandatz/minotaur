namespace Minotaur {
	using System;
	using McMaster.Extensions.CommandLineUtils;

	public sealed class Program {

		public static int Main(string[] args) {
			if (args.Length == 1 && args[0] == "--lazy-dev-switch")
				args = LazyDevArguments();

			return CommandLineApplication.Execute<ProgramSettings>(args);
		}

		private static string[] LazyDevArguments() {
			throw new NotImplementedException();
		}

		private static int OnExecute(ProgramSettings settings) {
			throw new NotImplementedException();
		}
	}
}
