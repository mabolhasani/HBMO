namespace HBMO
{
    class Program
    {
        static void Main(string[] args)
        {
            View view = new View();
            Chromosome chromosome = new Chromosome();

            MaterializedView materializedView = new MaterializedView(view, chromosome);

            Chromosome result = materializedView.HbmoViewSelection(3, 100, .75f, .6f, .05f, 10, 100);
        }
    }
}
