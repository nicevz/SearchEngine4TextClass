using ConsoleVer;
using Lucene.Net.Analysis.Br;
using System.Diagnostics;
using System.Text;

string url = @"SampledJson.json";

Console.OutputEncoding = Encoding.Unicode;
Console.WriteLine("=============================================");
Console.WriteLine("Select Modes:");
Console.WriteLine("0. Create Sampled dataset");
Console.WriteLine("1. Do Full Text Search on ReviewText Field");
Console.WriteLine("2. Do Restrained FTS");
//Console.WriteLine("4. Search for Specific item");
Console.WriteLine("=============================================");
int mode;
Console.Write("Your Choice: ");
int.TryParse(Console.ReadLine().Trim(), out mode);
EngineBody engine = new EngineBody(16, url);
engine.InitializeLucene();
var a = engine.TaskScheduler4AddingDocs().Result;
engine.writer.Commit();

switch (mode)
{
    case 0:
        Console.Write("num of samples: ");
        int samplenum;
        if (int.TryParse(Console.ReadLine(), out samplenum))
        {
            JsonDeserialization jsonDeserializer = new JsonDeserialization(16, samplenum, url, "");
            jsonDeserializer.GetSampledJSONFile();
        }
        Console.WriteLine("Wrong number!😡👊");
        break;
    case 1:
        while (true)
        {
            Console.Write("Top N results?: ");
            int num;
            if (int.TryParse(Console.ReadLine(), out num))
            {
                Console.WriteLine("Write your search terms down below:");
                string term = Console.ReadLine();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                engine.FTS(term, num);
                Console.WriteLine($"Query Time: {sw.Elapsed.TotalMilliseconds}ms");
            }
            else
                Console.WriteLine("Wrong number!😡👊");
        }
        break;
    case 2:
        while (true)
        {
            Console.Write("Top N results?: ");
            int num;
            if (int.TryParse(Console.ReadLine(), out num))
            {
                Console.WriteLine("Write your search terms down below:");
                string term1 = Console.ReadLine();
                Console.WriteLine("Write your PID down below:");
                string term2 = Console.ReadLine();
                engine.RFTS(term1, term2, num);
            }
            else
                Console.WriteLine("Wrong number!😡👊");
        }
        break;
    default:
        Console.WriteLine("Wrong mode!😡👊");
        break;
}
