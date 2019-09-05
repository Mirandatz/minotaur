namespace Tests {
	using NUnit.Framework;
	using Minotaur.Collections;

	public sealed class ArrayTests {

		[Test]
		public void Test1() {
			Array<float> temp = new float[] { 0, 1, 2, 3 };

			Assert.IsNotNull(temp);
		}
	}
}