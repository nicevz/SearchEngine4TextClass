using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine4TextClass.Model
{
    internal class WrapperClass4Test
    {
        public int threadsCount { get; set; }
        public int samplingCount { get; set; }
        public string fileLocation { get; set; }
        public ObservableCollection<string> productIDs { get; set; } = new ObservableCollection<string>();
        public string productName { get; set; }

        public void callDeserialization()
        {
            JsonDeserialization jsonDeserializer = new JsonDeserialization(threadsCount, samplingCount, fileLocation);
            jsonDeserializer.SamplingPID();
            productIDs = jsonDeserializer.SampledProductIDs;
            productName = productIDs[0];
        }
    }
}
