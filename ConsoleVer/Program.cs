using ConsoleVer;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

string url = @"SampledJson.json";

Console.OutputEncoding = Encoding.Unicode;
Console.WriteLine("=============================================");
Console.WriteLine("Select Modes:");
Console.WriteLine("1. Create Sampled dataset");
Console.WriteLine("2. Do Full Text Search on ReviewText Field");
Console.WriteLine("3. Do Restrained FTS");
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
	case 1:
		JsonDeserialization jsonDeserializer = new JsonDeserialization(16, 200, url, "");
		jsonDeserializer.GetSampledJSONFile();
		break;
	case 2:
		while (true)
		{
			Console.WriteLine("Write your search terms down below:");
			string term = Console.ReadLine();
			engine.FTS(term);
		}
		break;
	case 3:
		while (true) {
			Console.WriteLine("Write your search terms down below:");
			string term1 = Console.ReadLine();
			Console.WriteLine("Write your PID down below:");
			string term2 = Console.ReadLine();
			engine.RFTS(term1, term2);
		}
		break;
	//case 4:
	//	break;
	default:
		Console.WriteLine("Wrong mode!😡👊");
		break;
}
