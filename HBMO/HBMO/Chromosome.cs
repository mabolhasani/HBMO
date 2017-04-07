using System;
using System.Collections.Generic;

namespace HBMO
{
    public class Chromosome
    {
        public List<View> Views { get; set; }
        public double Tvec { get; set; }

        public List<Chromosome> PopulateChromosome(List<View> lattice, int topViewCount, int populationCount)
        {
            List<Chromosome> chromosomes = new List<Chromosome>();

            Random random = new Random();
            for (int i = 0; i < populationCount; i++)
            {
                List<View> chromosome = new List<View> { lattice[0] };

                while (chromosome.Count < topViewCount + 1)
                {
                    int selectedViewIndex = random.Next(lattice.Count - 1);

                    if (!chromosome.Contains(lattice[selectedViewIndex]))
                    {
                        chromosome.Add(lattice[selectedViewIndex]);
                    }
                }

                chromosomes.Add(new Chromosome
                {
                    Views = chromosome,
                    Tvec = 0
                });
            }
            return chromosomes;
        }

        public HbmoInitialModel InitializeHbmoModel(List<View> lattice)
        {
            double tvecMax = lattice[0].Size * lattice.Count;
            double tvecMin = tvecMax / 2;
            double qsMax = tvecMin + (tvecMax - tvecMin) * new Random().Next(1);
            return new HbmoInitialModel
            {
                TvecMax = tvecMax,
                TvecMin = tvecMin,
                QsMax = qsMax,
                QsMin = tvecMax / 10,
                Speed = qsMax,
                Alpha = 0.98f,
                BroodViews = 4
            };
        }
    }
}
