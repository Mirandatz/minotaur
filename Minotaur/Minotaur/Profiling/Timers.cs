namespace Minotaur.Profiling {
	using System;
	using System.Threading;

	public static class Timers {

		private static long _cfsbeTicks = 0;
		public static long CFSBETicks => _cfsbeTicks;
		public static void IncrementCfsbeTicks(long incrementValue) {
			if (incrementValue < 0)
				throw new InvalidOperationException();

			Interlocked.Add(ref _cfsbeTicks, incrementValue);
		}

		private static long _fitnessEvaluationTicks = 0;
		public static long FitnessEvaluationTicks => _fitnessEvaluationTicks;
		public static void IncrementFitnessEvaluationTicks(long incrementValue) {
			if (incrementValue < 0)
				throw new InvalidOperationException();

			Interlocked.Add(ref _fitnessEvaluationTicks, incrementValue);
		}
	}
}

