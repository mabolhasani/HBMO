namespace HBMO
{
    class Program
    {
        static void Main(string[] args)
        {
            View view = new View();
            Chromosome chromosome = new Chromosome();

            MaterializedView materializedView = new MaterializedView(view, chromosome);

            Chromosome result = materializedView.HbmoViewSelection(100, 10, .5f, .7f, .25f, 3, 10);
        }
    }
}
