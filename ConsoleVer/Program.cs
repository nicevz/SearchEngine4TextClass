using ConsoleVer;
using Lucene.Net.Analysis.Br;
using System.Diagnostics;
using System.Text;

string url = @"SampledJson.json";

Console.OutputEncoding = Encoding.Unicode;

void mywrtL(string a)
{
    Console.WriteLine("\u001b[32m" + "\u001b[1m" + a + "\u001b[0m");
}

mywrtL("========================================================");
mywrtL("Select Modes:");
mywrtL("0. Create Sampled dataset");
mywrtL("1. Do Full Text Search on ReviewText Field");
mywrtL("2. Do Restrained Full Text Search on ReviewText Field");
mywrtL("3. Search a Specific item feature");
mywrtL("========================================================");
int mode;
Console.Write("Your Choice: ");
if (int.TryParse(Console.ReadLine().Trim(), out mode))
{
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
            else
            {
                mywrtL("Wrong number!😡👊");
            }
            break;
        case 1:
            while (true)
            {
                Console.Write("Top N results?: ");
                int num;
                if (int.TryParse(Console.ReadLine(), out num))
                {
                    mywrtL("Write your search terms down below:");
                    string term = Console.ReadLine();
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    engine.FTS(term, num);
                    mywrtL($"Query Time: {sw.Elapsed.TotalMilliseconds}ms");
                }
                else
                    mywrtL("Wrong number!😡👊");
            }
            break;
        case 2:
            while (true)
            {
                Console.Write("Top N results?: ");
                int num;
                if (int.TryParse(Console.ReadLine(), out num))
                {
                    mywrtL("Write your search terms down below:");
                    string term1 = Console.ReadLine();
                    mywrtL("Which sub-category do you want to select?");
                    mywrtL("1. ProductID\t2. Summary\t3. ReviewerID\t4. ReviewerName");
                    int choice;
                    string choiceString;
                    if (int.TryParse(Console.ReadLine(), out choice))
                    {
                        switch (choice)
                        {
                            case 1:
                                choiceString = "ProductID";
                                break;
                            case 2:
                                choiceString = "Summary";
                                break;
                            case 3:
                                choiceString = "ReviewerID";
                                break;
                            case 4:
                                choiceString = "ReviewerName";
                                break;
                            default:
                                mywrtL("Wrong number!😡👊👊👊👊");
                                continue;
                        }
                    }
                    else
                    {
                        mywrtL("Wrong number!😡👊👊👊👊");
                        continue;
                    }
                    mywrtL("Write your sub-query down below:");
                    string term2 = Console.ReadLine();
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    engine.RFTS(term1, term2, choiceString, num);
                    mywrtL($"Query Time: {sw.Elapsed.TotalMilliseconds}ms");
                }
                else
                    mywrtL("Wrong number!😡👊👊👊👊");
            }
        case 3:
            while (true)
            {
                Console.Write("Top N results?: ");
                int num;
                if (int.TryParse(Console.ReadLine(), out num))
                {
                    mywrtL("Which sub-category do you want to select?");
                    mywrtL("1. ProductID\t2. Summary\t3. ReviewerID\t4. ReviewerName");
                    int choice;
                    string choiceString;
                    if (int.TryParse(Console.ReadLine(), out choice))
                    {
                        switch (choice)
                        {
                            case 1:
                                choiceString = "ProductID";
                                break;
                            case 2:
                                choiceString = "Summary";
                                break;
                            case 3:
                                choiceString = "ReviewerID";
                                break;
                            case 4:
                                choiceString = "ReviewerName";
                                break;
                            default:
                                mywrtL("Wrong number!😡👊👊👊👊");
                                continue;
                        }
                    }
                    else
                    {
                        mywrtL("Wrong number!😡👊👊👊👊");
                        continue;
                    }
                    mywrtL("Write your sub-query down below:");
                    string term2 = Console.ReadLine();
                    engine.RS(term2, choiceString, num);
                }
                else
                    mywrtL("Wrong number!😡👊👊👊👊");
            }
        default:
            mywrtL("Wrong mode!😡👊👊👊👊");
            break;
    }
}
mywrtL("Wrong number!😡👊👊👊👊");