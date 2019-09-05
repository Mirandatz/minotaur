namespace Minotaur.Tests.ArrayTests {
	using System;
	using Minotaur.Collections;
	using NUnit.Framework;

	public sealed class BinarySearchFirstOccurenceTests {

		public const float ValueToSearch = 7.0f;

		[TestCase(new float[] { 7 })]
		[TestCase(new float[] { 7, 7 })]
		[TestCase(new float[] { 7, 7, 7 })]

		[TestCase(new float[] { 0, 7 })]
		[TestCase(new float[] { 0, 0, 7 })]
		[TestCase(new float[] { 0, 0, 0, 7 })]

		[TestCase(new float[] { 7, 9 })]
		[TestCase(new float[] { 7, 9, 9 })]
		[TestCase(new float[] { 7, 9, 9, 9 })]

		[TestCase(new float[] { 0, 7, 9 })]
		[TestCase(new float[] { 0, 7, 7, 9 })]
		[TestCase(new float[] { 0, 7, 7, 7, 9 })]

		[TestCase(new float[] { 0, 0, 7, 7, 7, 9 })]
		[TestCase(new float[] { 0, 7, 7, 7, 9, 9 })]

		[TestCase(new float[] { 0, 0, 7, 7, 7, 9, 9 })]
		public void ContainsNumber(
			float[] arrayValues
			) {
			var array = Array<float>.Wrap(arrayValues);
			var expectedIndex = LinearSearch(array, ValueToSearch);
			var actualIndex = array.BinarySearchFirstOccurence(ValueToSearch);

			Assert.AreEqual(
				expected: expectedIndex,
				actual: actualIndex);
		}

		public static int LinearSearch(Array<float> array, float value) {
			for (int i = 0; i < array.Length; i++)
				if (array[i] == value)
					return i;

			throw new InvalidOperationException();
		}
	}
}