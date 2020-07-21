namespace Minotaur.FallibleTasks {
	using System;
	using System.Collections.Generic;
	using System.Threading;

	public sealed class LoopController<T> where T : class, IEquatable<T> {

		private const int StateContinueLooping = 0;
		private const int StateStopLooping = 1;
		private const int StatePeekingValue = 2;
		private int _state = StateContinueLooping;

		private readonly int _maxFailedAttempts;
		private int _failedAttempts;

		private readonly int _targetResultCount;
		private readonly HashSet<T> _distinctResults;

		public LoopController(int maxFailedAttempts, int targetResultCount, HashSet<T> distinctResults) {
			if (maxFailedAttempts < 0)
				throw new ArgumentOutOfRangeException(nameof(maxFailedAttempts) + " must be >= 0.");
			if (targetResultCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(targetResultCount) + " must be > 0.");

			_maxFailedAttempts = maxFailedAttempts;
			_targetResultCount = targetResultCount;
			_distinctResults = distinctResults;
			_failedAttempts = 0;
		}

		public bool ShouldContinueLooping {
			get {
				var currentState = Interlocked.CompareExchange(
					location1: ref _state,
					value: StatePeekingValue,
					comparand: StatePeekingValue);

				if (currentState == StateContinueLooping)
					return true;
				if (currentState == StateStopLooping)
					return false;

				throw new InvalidOperationException();
			}
		}

		public void UpdateLoopStatus(T? fallibleFunctionResult) {
			if (fallibleFunctionResult is null) {
				FunctionFailed();
				return;
			}

			bool resultIsUnique;
			int distinctResultsCount;
			lock (_distinctResults) {
				resultIsUnique = _distinctResults.Add(fallibleFunctionResult);
				distinctResultsCount = _distinctResults.Count;
			}

			if (!resultIsUnique)
				FunctionFailed();
			else
				FunctionSucceeded(distinctResultsCount);
		}

		private void FunctionFailed() {
			var updatedFailedCount = Interlocked.Increment(ref _failedAttempts);
			if (updatedFailedCount >= _maxFailedAttempts) {
				Interlocked.CompareExchange(
					location1: ref _state,
					value: StateStopLooping,
					comparand: StateContinueLooping);
			}
		}

		private void FunctionSucceeded(int distinctResultsCount) {
			if (distinctResultsCount >= _targetResultCount) {
				Interlocked.CompareExchange(
					location1: ref _state,
					value: StateStopLooping,
					comparand: StateContinueLooping);
			}
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
