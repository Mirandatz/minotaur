namespace Minotaur {
	using System;
	using System.Threading;

	public static class Timers {

		private static long _totalTicks;
		public static long TotalTicks => _totalTicks;
		public static void IncrementTotalTicks(long incrementValue) {
			if (incrementValue < 0)
				throw new InvalidOperationException();

			Interlocked.Add(ref _totalTicks, incrementValue);
		}

		private static long _cfsbeTicks;
		public static long CFSBETicks => _cfsbeTicks;
		public static void IncrementCfsbeTicks(long incrementValue) {
			if (incrementValue < 0)
				throw new InvalidOperationException();

			Interlocked.Add(ref _cfsbeTicks, incrementValue);
		}

		private static long _fitnessEvaluationTicks;
		public static long FitnessEvaluationTicks => _fitnessEvaluationTicks;
		public static void IncrementFitnessEvaluationTicks(long incrementValue) {
			if (incrementValue < 0)
				throw new InvalidOperationException();

			Interlocked.Add(ref _fitnessEvaluationTicks, incrementValue);
		}

		private static long _nsga2Ticks;
		public static long NSGA2Ticks => _nsga2Ticks;
		public static void IncrementNSGA2Ticks(long incrementValue) {
			if (incrementValue < 0)
				throw new InvalidOperationException();

			Interlocked.Add(ref _nsga2Ticks, incrementValue);
		}
	}
}

