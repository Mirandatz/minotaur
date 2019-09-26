namespace Minotaur.ExtensionMethods.SystemArray {
	using System.Collections.Generic;
	using Minotaur.Collections;

	public static class GenericSystemArrayExtensions {

		public static T[] Concatenate<T>(this T[] lhs, T[] rhs) {
			if (rhs is null)
				throw new System.ArgumentNullException(nameof(rhs));

			var concatenated = new T[lhs.Length + rhs.Length];
			lhs.CopyTo(concatenated, 0);
			rhs.CopyTo(concatenated, lhs.Length);

			return concatenated;
		}

		public static IEnumerator<T> GetGenericEnumerator<T>(this T[] array) {
			return ((IEnumerable<T>) array).GetEnumerator();
		}

		public static Array<T> AsReadOnly<T>(this T[] array) => Array<T>.Wrap(array);
	}

	public static class ReferenceTypesSystemArrayExtensions {

		public static bool ContainsNulls<T>(this T[] array) where T : class {
			for (int i = 0; i < array.Length; i++)
				if (array[i] == null)
					return true;

			return false;
		}
	}

	public static class FloatSystemArrayExtensions {

		public static bool FloatSequenceEquals(this float[] lhs, float[] rhs) {
			if (ReferenceEquals(lhs, rhs))
				return true;

			if (lhs.Length != rhs.Length)
				return false;

			for (int i = 0; i < lhs.Length; i++) {
				if (lhs[i] != rhs[i])
					return false;
			}

			return true;
		}
	}
}
