namespace DeepNestLib.GeneticAlgorithm
{
  using GeneticSharp;

  public class NestChromosome : ChromosomeBase
  {
    public NestChromosome(int numberOfParts) : base(numberOfParts) => CreateGenes();

    public override IChromosome CreateNew()
    {
      return new NestChromosome(Length);
    }

    public override Gene GenerateGene(int geneIndex)
    {
      return new Gene(RandomizationProvider.Current.GetInt(0, Length * 50));
    }
  }
}