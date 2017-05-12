using System.Collections.Generic;

namespace HBMO
{
    class Program
    {
        static void Main(string[] args)
        {
            View view = new View();
            Chromosome chromosome = new Chromosome();

            MaterializedView materializedView = new MaterializedView(view, chromosome);

            //Chromosome result = materializedView.HbmoViewSelection(100, 100, .75f, .6f, .05f, 5, 100);

            Dictionary<int, double> results = new Dictionary<int, double>();

            for (int i = 5; i <= 10; i++)
            {
                results.Add(i, materializedView.HbmoViewSelection(100, 100, .75f, .6f, .05f, i, 100).Tvec);
            }
        }
    }
}
