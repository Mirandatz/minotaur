namespace Minotaur {
	using System;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using McMaster.Extensions.CommandLineUtils;
	using Minotaur.Random;

	public sealed class Program {

		public const int ExpectedTotal = 1000 * 1000;

		public static int Main(string[] args) {

			while (true) {
				long failCount;
				int nullCount;

				using (var cts = new CancellationTokenSource()) {
					var options = new ParallelOptions();
					options.CancellationToken = cts.Token;

					(failCount, nullCount) = WithParallelOptions(options);
				}

				if (failCount >= 200) {
					if (nullCount == 0)
						throw new InvalidProgramException();
				} else {
					Console.Write(".");
				}

			}

			//if (args.Length == 1 && args[0] == "--lazy-dev-switch")
			//	args = LazyDevArguments();

			//return CommandLineApplication.Execute<ProgramSettings>(args);
		}

		private static (long, int) WithParallelOptions(ParallelOptions parallelOptions) {
			var buffer = new object[1000];
			var maxFailures = 200;
			var failCount = 0L;

			Parallel.For(
				fromInclusive: 0,
				toExclusive: buffer.Length,
				parallelOptions: parallelOptions,
				(index, loopState) => {

					var updatedFailCount = Interlocked.Read(ref failCount);
					if (updatedFailCount >= maxFailures)
						loopState.Stop();

					while (true) {
						var sucess = Random.ThreadStaticRandom.Bool(biasForTrue: 0.85f);
						if (sucess) {
							buffer[index] = new object();
							break;
						} else {
							updatedFailCount = Interlocked.Increment(ref failCount);
							if (updatedFailCount >= maxFailures)
								loopState.Stop();
						}
					}
				});

			var nullCount = buffer.Count(o => o is null);

			if (failCount >= 200) {
				if (nullCount == 0)
					throw new InvalidProgramException();
			} else {
				Console.Write(".");
			}

			return (failCount, nullCount);
		}

		private static string[] LazyDevArguments() {
			throw new NotImplementedException();
		}

		private static int OnExecute(ProgramSettings settings) {
			throw new NotImplementedException();
		}
	}
}
