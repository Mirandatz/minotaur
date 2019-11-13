namespace Minotaur.Output {
	using Minotaur.Theseus.Evolution;

	public interface IEvolutionLogger {

		public void LogGeneration(GenerationResult generationResult);
	}
}
