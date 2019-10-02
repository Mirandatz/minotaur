namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;

	public sealed class NonIntersectingRectangleCreator {

		public readonly Dataset Dataset;

		public NonIntersectingRectangleCreator(Dataset dataset) {
			Dataset = dataset;
		}

		public HyperRectangle CreateLargestNonIntersectingRectangle(int seedIndex, Array<HyperRectangle> hyperRectangles, NaturalRange dimensionExpansionOrder) {
			if (dimensionExpansionOrder.Length != Dataset.FeatureCount)
				throw new InvalidOperationException();

			var builder = HyperRectangleBuilder.InitializeWithLargestRectangle(Dataset);

			if (hyperRectangles.IsEmpty)
				return builder.Build();

			throw new NotImplementedException();
		}

	}
}
