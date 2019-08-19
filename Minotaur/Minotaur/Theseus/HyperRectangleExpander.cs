namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math.Dimensions;

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

			// @Improve performance
			// @Improve exception messages
			if (others.ContainsNulls())
				throw new ArgumentException(nameof(others));

			if (target.Dimensions.Length != dimensionExpansionOrder.Length)
				throw new ArgumentException();

			if (others.Any(d => d.Dimensions.Length != dimensionExpansionOrder.Length))
				throw new ArgumentException();

			if (!dimensionExpansionOrder.Distinct().OrderBy(d => d).SequenceEqual(Enumerable.Range(start: 0, count: dimensionExpansionOrder.Length)))
				throw new ArgumentException();

			return EnlargeImpl(
				target: target,
				others: others,
				dimensionExpansionOrder: dimensionExpansionOrder);
		}

		private HyperRectangle EnlargeImpl(
			HyperRectangle target,
			Array<HyperRectangle> others,
			Array<int> dimensionExpansionOrder
			) {

			for (int i = 0; i < dimensionExpansionOrder.Length; i++) {
				var dimesionIndex = dimensionExpansionOrder[i];

			}

			throw new NotImplementedException();
		}
	}
}
