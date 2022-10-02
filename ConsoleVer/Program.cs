using ConsoleVer;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

string url = @"C:\Users\z-py\OneDrive - whu.edu.cn\桌面\Video_Games_5.json";
////JsonDeserialization JsonDeserializer = new JsonDeserialization(16, 1000, url);
////JsonDeserializer.SamplingPID();
////JsonDeserializer.InitializeLucene();
////ObservableCollection<ReviewObject> allR = new ObservableCollection<ReviewObject>();
////foreach (var taskResult in JsonDeserializer.TaskScheduler4AddingDocs().Result)
////{
////    allR = new ObservableCollection<ReviewObject>(allR.Concat(taskResult));
////}
////List<string> strings = new List<string>();
////foreach(var a in allR)
////{
////    strings.Add(JsonSerializer.Serialize(a));
////}
////File.WriteAllLines(url+"sampled", strings.ToArray());
////JsonDeserializer.LuceneSample();

//Class1 class1 = new Class1(16, url);
//class1.InitializeLucene();
//ObservableCollection<ReviewObject> allR = new ObservableCollection<ReviewObject>();
//foreach (var taskResult in class1.TaskScheduler4AddingDocs().Result)
//{
//    allR = new ObservableCollection<ReviewObject>(allR.Concat(taskResult));
//}
//class1.LuceneSample();

Console.OutputEncoding = Encoding.Unicode;
Console.WriteLine("=============================================");
Console.WriteLine("Select Modes:");
Console.WriteLine("1. Create Sampled dataset");
Console.WriteLine("2. Do Full Text Search on ReviewText Field");
Console.WriteLine("3. Do Restrained FTS");
Console.WriteLine("4. Search for Specific item");
Console.WriteLine("=============================================");
int mode = 0;
int.TryParse(Console.ReadLine().Trim(), out mode);

switch (mode)
{
	case 1:
		break;
	case 2:
		Class1 class1 = new Class1(16, url);
		class1.InitializeLucene();
		ObservableCollection<ReviewObject> allR = new ObservableCollection<ReviewObject>();
		foreach (var taskResult in class1.TaskScheduler4AddingDocs().Result)
		{
			allR = new ObservableCollection<ReviewObject>(allR.Concat(taskResult));
		}
		class1.LuceneSample();
		break;
	case 3:
		break;
	case 4:
		break;
	default:
		Console.WriteLine("Wrong mode!😡👊");
		break;
}
