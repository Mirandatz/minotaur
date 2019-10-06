namespace Minotaur.Theseus.IndividualBreeding {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;
	using Random = Random.ThreadStaticRandom;

	public sealed class PopulationBreeder {

		private readonly IndividualBreeder _individualBreeder;
		private readonly int _childrenPerGeneration;

		public PopulationBreeder(IndividualBreeder individualBreeder, int childrenPerGeneration) {
			if (childrenPerGeneration < 0)
				throw new ArgumentOutOfRangeException(nameof(childrenPerGeneration));

			_individualBreeder = individualBreeder;
			_childrenPerGeneration = childrenPerGeneration;
		}

		public Individual[] Breed(Array<Individual> population) {
			if (population.Length == 0)
				throw new ArgumentException(nameof(population) + " can't be empty.");

			var children = new Individual[_childrenPerGeneration];
			Parallel.For(0, children.Length, i => {
				var lhs = Random.Choice(population);
				var rhs = Random.Choice(population);
				children[i] = _individualBreeder.Breed(lhs, rhs);
			});

			return children;
		}
	}
}
