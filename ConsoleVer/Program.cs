using ConsoleVer;
using System.Collections.ObjectModel;
using System.Text.Json;

string url = @"C:\Users\z-py\OneDrive - whu.edu.cn\桌面\Video_Games_5.json";
//JsonDeserialization JsonDeserializer = new JsonDeserialization(16, 1000, url);
//JsonDeserializer.SamplingPID();
//JsonDeserializer.InitializeLucene();
//ObservableCollection<ReviewObject> allR = new ObservableCollection<ReviewObject>();
//foreach (var taskResult in JsonDeserializer.TaskScheduler4AddingDocs().Result)
//{
//    allR = new ObservableCollection<ReviewObject>(allR.Concat(taskResult));
//}
//List<string> strings = new List<string>();
//foreach(var a in allR)
//{
//    strings.Add(JsonSerializer.Serialize(a));
//}
//File.WriteAllLines(url+"sampled", strings.ToArray());
//JsonDeserializer.LuceneSample();

Class1 class1 = new Class1(16, url);
class1.InitializeLucene();
ObservableCollection<ReviewObject> allR = new ObservableCollection<ReviewObject>();
foreach (var taskResult in class1.TaskScheduler4AddingDocs().Result)
{
    allR = new ObservableCollection<ReviewObject>(allR.Concat(taskResult));
}
class1.LuceneSample();
