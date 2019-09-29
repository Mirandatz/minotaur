namespace Minotaur.Theseus {
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math.Dimensions;

	public sealed class HyperRectangleCoverageComputer {
		private readonly Dataset _dataset;
		private readonly IConcurrentCache<HyperRectangle, DatasetCoverage> _cache;

		public HyperRectangleCoverageComputer(Dataset dataset, IConcurrentCache<HyperRectangle, DatasetCoverage> cache) {
			_dataset = dataset;
			_cache = cache;
		}

		public DatasetCoverage ComputeCoverage(HyperRectangle box) {
			if (_cache.TryGet(box, out var coverage)) {
				return coverage;
			} else {
				coverage = UncachedComputeCoverage(box);
				_cache.Add(key: box, value: coverage);
				return coverage;
			}
		}

		private DatasetCoverage UncachedComputeCoverage(HyperRectangle box) {
			var instanceCount = _dataset.InstanceCount;
			var instaceIsCovered = new bool[instanceCount];

			for (int i = 0; i < instaceIsCovered.Length; i++) {
				var instanceData = _dataset.GetInstanceData(i);
				instaceIsCovered[i] = box.Contains(instanceData);
			}

			return new DatasetCoverage(
				dataset: _dataset,
				instancesCovered: instaceIsCovered);
		}
	}
}
