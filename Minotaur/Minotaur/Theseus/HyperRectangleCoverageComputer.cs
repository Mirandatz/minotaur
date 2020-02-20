namespace Minotaur.Theseus {
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math.Dimensions;

	public sealed class HyperRectangleCoverageComputer {
		public readonly Dataset Dataset;

		public HyperRectangleCoverageComputer(Dataset dataset) {
			Dataset = dataset;
		}

		public DatasetCoverage ComputeCoverage(HyperRectangle hyperRectangle) {
			var instanceCount = Dataset.InstanceCount;
			var instaceIsCovered = new bool[instanceCount];

			for (int i = 0; i < instaceIsCovered.Length; i++) {
				var instanceData = Dataset.GetInstanceData(i);
				instaceIsCovered[i] = hyperRectangle.Contains(instanceData);
			}

			return new DatasetCoverage(
				dataset: Dataset,
				instancesCovered: instaceIsCovered);
		}

		public DatasetCoverage[] ComputeCoverages(Array<HyperRectangle> hyperRectangles) {
			var coverages = new DatasetCoverage[hyperRectangles.Length];

			Parallel.For(0, hyperRectangles.Length, i => {
				var currentRectangle = hyperRectangles[i];
				var currentCoverage = ComputeCoverage(hyperRectangle: currentRectangle);
				coverages[i] = currentCoverage;
			});

			return coverages;
		}
	}
}
