using System;
using System.Collections.Generic;
using System.Linq;

namespace HBMO
{
    public class MaterializedView
    {
        private readonly View _view;
        private readonly Chromosome _chromosome;


        public MaterializedView(View view, Chromosome chromosome)
        {
            _view = view;
            _chromosome = chromosome;
        }
        public Chromosome HbmoViewSelection(int generations, int spermathecaSize, float tournamentChance, float crossOverProbability, float mutationProbability, int topViewCount, int firstPoulationCount)
        {
            List<View> lattice = _view.CreateLattice();
            //Step 1:
            HbmoInitialModel hbmoInitialModel = _chromosome.InitializeHbmoModel(lattice);
            //Step 2:
            List<Chromosome> population = _chromosome.PopulateChromosome(lattice, topViewCount, firstPoulationCount);

            Random random = new Random();

            population = SetTvecForEachChromosome(lattice, population);
            //Step 3:
            Chromosome queen = population.First(p => p.Tvec == population.Min(c => c.Tvec));

            for (int i = 0; i < generations; i++)
            {
                List<Chromosome> spermatheca = new List<Chromosome>();
                //Step 4:
                while (spermatheca.Count < spermathecaSize)
                {
                    Chromosome drone = population[random.Next(population.Count)];

                    if (queen.Tvec > drone.Tvec)
                    {
                        if (spermatheca.Count >= spermathecaSize)
                            break;

                        spermatheca.Add(drone);
                        population.Remove(population.Find(p => p.Views == drone.Views));
                    }
                    else
                    {
                        double deltaTvec = queen.Tvec - drone.Tvec;
                        double randomNumber = random.NextDouble();

                        double annealing = CalculateAnnealingFunction(deltaTvec, hbmoInitialModel.Speed);

                        if (annealing > randomNumber)
                        {
                            if (spermatheca.Count >= spermathecaSize)
                                break;

                            spermatheca.Add(drone);
                            population.Remove(population.Find(p => p.Views == drone.Views));
                        }

                        hbmoInitialModel.Speed = hbmoInitialModel.Alpha * hbmoInitialModel.Speed;

                        if (hbmoInitialModel.Speed < hbmoInitialModel.QsMin)
                            hbmoInitialModel.Speed = hbmoInitialModel.QsMax;
                    }
                }

                //step 5:
                List<Chromosome> broodViews = CrossOver(spermatheca, queen, crossOverProbability, hbmoInitialModel.BroodViews);

                //step 6:
                List<Chromosome> improvedBbroodViews = Mutation(lattice, broodViews, mutationProbability, 10, topViewCount);

                //strp7:
                Chromosome bestBroodView = improvedBbroodViews.First(p => p.Tvec == improvedBbroodViews.Max(c => c.Tvec));

                if (bestBroodView.Tvec < queen.Tvec)
                    queen = bestBroodView;

                population.Clear();

                population = _chromosome.PopulateChromosome(lattice, topViewCount, firstPoulationCount);
                population = SetTvecForEachChromosome(lattice, population);
            }

            return queen;
        }

        private double CalculateFitnessFunction(List<View> lattice, List<View> chromosome)
        {
            List<View> tempLattice = lattice.ToList();
            double sizeView = 0;
            double sizeSmaView = 0;

            foreach (var gene in chromosome)
            {
                sizeView += gene.Size;
                tempLattice.Remove(gene);
            }

            foreach (var view in tempLattice)
            {
                if (chromosome.Any(t => t.Level < view.Level))
                    sizeSmaView += chromosome.Where(c => c.Level < view.Level && c.Dimensions.Intersect(view.Dimensions).Any()).Min(c => c.Size);
            }

            return sizeView + sizeSmaView;
        }

        private List<Chromosome> CrossOver(List<Chromosome> spermatheca, Chromosome queen, float probability,
            double iteration)
        {
            Random random = new Random();
            double crossOverPoint = Math.Floor((double)(spermatheca[0].Views.Count - 1) / 2);
            List<Chromosome> result = new List<Chromosome>();

            for (int i = 0; i < iteration; i++)
            {
                Chromosome father = spermatheca[random.Next(spermatheca.Count)];
                Chromosome mother = queen;

                if (random.NextDouble() > probability)
                {
                    List<View> firstChildViews = new List<View> { father.Views[0] };
                    List<View> secondChildViews = new List<View> { father.Views[0] };

                    for (int j = 1; j <= crossOverPoint; j++)
                    {
                        firstChildViews.Add(father.Views[j]);
                        secondChildViews.Add(mother.Views[j]);
                    }

                    for (int j = (int)crossOverPoint + 1; j < spermatheca[0].Views.Count; j++)
                    {
                        firstChildViews.Add(mother.Views[j]);
                        secondChildViews.Add(father.Views[j]);
                    }

                    if (IsValidChromosome(firstChildViews))
                        result.Add(new Chromosome
                        {
                            Views = firstChildViews,
                            Tvec = 0
                        });

                    if (IsValidChromosome(secondChildViews))
                        result.Add(new Chromosome
                        {
                            Views = secondChildViews,
                            Tvec = 0
                        });
                }
                else
                {
                    result.AddRange(new[] { father, mother });
                }
            }

            return result;
        }

        private List<Chromosome> Mutation(List<View> lattice, List<Chromosome> broodViews, float probability,
            int iteration, int topViewCount)
        {
            Random random = new Random();
            List<Chromosome> result = new List<Chromosome>();

            for (int i = 0; i < iteration; i++)
            {
                Chromosome chromosome = broodViews[random.Next(broodViews.Count)];

                if (random.NextDouble() <= probability)
                {
                    View selectedView = lattice[random.Next(lattice.Count - 1)];

                    chromosome.Views.Remove(chromosome.Views[random.Next(1, topViewCount - 1)]);

                    chromosome.Views.Add(selectedView);

                    if (IsValidChromosome(chromosome.Views))
                        result.Add(new Chromosome
                        {
                            Views = chromosome.Views,
                            Tvec = 0
                        });
                }
                else
                {
                    result.AddRange(new[] { chromosome });
                }
            }

            return SetTvecForEachChromosome(lattice, result);
        }

        private bool IsValidChromosome(List<View> chromosome)
        {
            return !chromosome.GroupBy(a => a.Label).Any(a => a.Count() > 1);
        }

        private List<Chromosome> SetTvecForEachChromosome(List<View> lattice, List<Chromosome> population)
        {
            foreach (var chromosome in population)
            {
                if ((int)chromosome.Tvec == 0)
                    chromosome.Tvec = CalculateFitnessFunction(lattice, chromosome.Views);
            }

            return population;
        }

        private double CalculateAnnealingFunction(double deltaTvec, double queenSpeed)
        {
            return 1 / Math.Exp(deltaTvec / queenSpeed);
        }
    }
}
