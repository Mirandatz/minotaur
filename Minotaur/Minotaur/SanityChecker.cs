namespace Minotaur {
	using System;

	public static class SanityChecker {

		private static readonly object _lock = new object();

		private static bool _isDefined = false;
		private static bool _performChecks = true;

		public static bool PerformChecks {
			get {
				lock (_lock) {
					if (!_isDefined)
						throw new InvalidOperationException();

					return _performChecks;
				}
			}

			set {
				lock (_lock) {
					if (!_isDefined) {
						_performChecks = value;
						_isDefined = true;
					} else {
						throw new InvalidOperationException();
					}
				}
			}
		}

		public static void Run(Action action) {
			if (PerformChecks)
				action();
		}
	}
}
