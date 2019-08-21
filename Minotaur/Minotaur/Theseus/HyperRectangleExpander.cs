namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math.Dimensions;

	// @Improve exception messages
	public sealed class HyperRectangleExpander {

		public readonly Dataset Dataset;

		public HyperRectangle Enlarge(
			HyperRectangle target,
			Array<HyperRectangle> others,
			Array<int> dimensionExpansionOrder
			) {
			if (target is null)
				throw new ArgumentNullException(nameof(target));
			if (others is null)
				throw new ArgumentNullException(nameof(others));
			if (dimensionExpansionOrder is null)
				throw new ArgumentNullException(nameof(dimensionExpansionOrder));

			throw new NotImplementedException();

			// @Add buttloads of checks
			//var enlarged = MutableHyperRectangle.FromRectangle(target);

			//for (int i = 0; i < dimensionExpansionOrder.Length; i++) {
			//	var dimensionIndex = dimensionExpansionOrder[i];

			//	Enlarge(
			//		target: enlarged,
			//		others: others,
			//		dimensionIndex: dimensionIndex);
			//}

			//return enlarged.ToHyperRectangle();
		}
	}
}
