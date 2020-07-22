namespace Minotaur.FallibleTasks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	public static partial class FallibleFunctionManager {

		public static T[]? TryGenerateDistinctResults<T>(int maxFailedAttempts, int targetResultCount, Func<T?> fallibleFunction) where T : class, IEquatable<T> {
			if (maxFailedAttempts < 0)
				throw new ArgumentOutOfRangeException(nameof(maxFailedAttempts) + " must be >= 0.");
			if (targetResultCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(targetResultCount) + " must be > 0.");

			var distinctResults = new HashSet<T>(capacity: targetResultCount);

			var loopController = new LoopController<T>(
				maxFailedAttempts: maxFailedAttempts,
				targetResultCount: targetResultCount,
				distinctResults: distinctResults);

			var tasks = new Task[Environment.ProcessorCount];

			for (int i = 0; i < tasks.Length; i++) {
				tasks[i] = Task.Run(() => RunFallibleTask(
					loopController: loopController,
					fallibleFunction: fallibleFunction));
			}

			Task.WaitAll(tasks);

			if (distinctResults.Count < targetResultCount)
				return null;
			if (distinctResults.Count == targetResultCount)
				return distinctResults.ToArray();
			else
				return distinctResults.Take(targetResultCount).ToArray();
		}

		private static void RunFallibleTask<T>(LoopController<T> loopController, Func<T?> fallibleFunction) where T : class, IEquatable<T> {
			while (loopController.ShouldContinueLooping) {
				var result = fallibleFunction();
				Task.Run(() => loopController.UpdateLoopStatus(result));
			}
		}
	}
}
