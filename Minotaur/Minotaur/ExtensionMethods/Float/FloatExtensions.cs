namespace Minotaur.ExtensionMethods.Float {

	public static class FloatExtensions {

		public static bool IsNanOrInfinity(this float value) {
			return float.IsNaN(value) || float.IsInfinity(value);
		}
	}
}
