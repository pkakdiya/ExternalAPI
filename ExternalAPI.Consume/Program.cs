using Api.Client;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExternalAPI.Consume
{
    class Program
    {
        static void Main(string[] args)
        {
            ApplicationTask applicationTask = new ApplicationTask("https://us-central1-abacuscorev2live.cloudfunctions.net/dailyorders/download", "75kY0sSlOILOlfLE6hQbFu57f2VA1JirTEy79Thy");

            applicationTask.PerformTask1();

            applicationTask.PerformTask2();

            applicationTask.PerformTask3();
        }
    }

    public class ParameterModel
    {
        public string StartDate { get; set; }

        public string project_id { get; set; }

        public bool ReportType { get; set; }
    }

    public class Store
    {
        public string Name { get; set; }

        public Dictionary<string, string> StoreValues { get; set; }
    }

    public abstract class Task
    {

        string _url;

        string _apiKey;

        public Task(string apiURL, string apiKey)
        {
            _url = apiURL;
            _apiKey = apiKey;
        }


        public static List<Store> GetStores(string[] csvContent)
        {
            List<Store> stores = new List<Store>();

            if (csvContent != null && csvContent.Length > 0)
            {
                string[] fileHeaders = csvContent[0].Split(new char[] { ',' });

                for (int i = 1; i < csvContent.Length; i++)
                {
                    string[] rowContents = csvContent[i].Split(new char[] { ',' });

                    if (rowContents.Length > 0)
                    {
                        var store = new Store();

                        if (!stores.Any(s => s.Name == rowContents[0]))
                        {
                            store.Name = rowContents[0];
                            store.StoreValues = new Dictionary<string, string>();

                            for (int j = 1; j < rowContents.Length; j++)
                            {
                                if (!string.IsNullOrEmpty(fileHeaders[j]))
                                {
                                    if (store.StoreValues.ContainsKey(fileHeaders[j]))
                                    {
                                        store.StoreValues[fileHeaders[j]] = rowContents[j];
                                    }
                                    else
                                    {
                                        store.StoreValues.Add(fileHeaders[j], rowContents[j]);
                                    }
                                }
                            }

                            stores.Add(store);
                        }
                    }
                }
            }

            return stores;
        }

        public static void ConvertStoreToCSV(List<Store> stores, string fileLocation)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (stores.Count > 0)
            {
                stringBuilder.AppendLine($",{string.Join(",", stores.First().StoreValues.Keys)}");

                foreach (var store in stores)
                {
                    stringBuilder.AppendLine($"{store.Name},{string.Join(",", store.StoreValues.Values)}");
                }

                File.WriteAllText(fileLocation, stringBuilder.ToString());
            }
        }

        public List<Store> GetCSVFileStore(string csvFilePath)
        {
            string[] csvFileContent = File.ReadAllLines(csvFilePath);

            return GetStores(csvFileContent);
        }

        public List<Store> GetFromAPICSVStore(string apiCSVFileLocation, ParameterModel dataModel)
        {
            APIClient client = new APIClient(_url, _apiKey);

            string apiCsvContent = client.GetDataFromAPI(dataModel).Result;

            File.WriteAllText(apiCSVFileLocation, apiCsvContent);

            string[] apiFileContent = File.ReadAllLines(apiCSVFileLocation);

            return GetStores(apiFileContent);
        }

        public abstract void PerformTask1();

        public abstract void PerformTask2();

        public abstract void PerformTask3();
    }

    public class ApplicationTask : Task
    {
        public ApplicationTask(string url, string apiKey) :
            base(url, apiKey)
        {

        }

        public override void PerformTask1()
        {
            List<Store> csvFileStore = GetCSVFileStore(@"C:\Users\pkakadiy\Downloads\Application\Base.csv");

            ParameterModel dataModel = new ParameterModel()
            {
                StartDate = "2019-05-27",
                project_id = "roll-d-mobile-app",
                ReportType = true
            };

            List<Store> fromAPICSVFileStore = GetFromAPICSVStore(@"C:\Users\pkakadiy\Downloads\Application\FromAPI.csv", dataModel);

            /* Update 27-May Data from API to CSV*/

            csvFileStore.ForEach(store =>
            {
                if (store.StoreValues.ContainsKey("27-05-2019"))
                {
                    store.StoreValues["27-05-2019"] = fromAPICSVFileStore.Where(s => s.Name == store.Name
                        && s.StoreValues.ContainsKey("2019-06-27")).Select(s => s.StoreValues["2019-06-27"]).FirstOrDefault();
                }
            });

            ConvertStoreToCSV(csvFileStore, @"C:\Users\pkakadiy\Downloads\Application\Task1.csv");
        }

        public override void PerformTask2()
        {
            List<Store> csvFileStore = GetCSVFileStore(@"C:\Users\pkakadiy\Downloads\Application\Base.csv");

            ParameterModel dataModel = new ParameterModel()
            {
                StartDate = "2019-05-27",
                project_id = "roll-d-mobile-app",
                ReportType = true
            };

            List<Store> fromAPICSVFileStore = GetFromAPICSVStore(@"C:\Users\pkakadiy\Downloads\Application\FromAPI.csv", dataModel);

            /* Update 28-May Data from API to CSV*/

            csvFileStore.ForEach(store =>
            {
                if (!store.StoreValues.ContainsKey("28-06-2019"))
                {
                    var data = fromAPICSVFileStore.Where(s => s.Name == store.Name && s.StoreValues.ContainsKey("2019-06-28")).Select(s => s.StoreValues["2019-06-28"]).ToList();

                    KeyValuePair<string, string> TotalkeyValuePair = new KeyValuePair<string, string>();

                    TotalkeyValuePair = store.StoreValues.ElementAt(store.StoreValues.Count - 1);

                    store.StoreValues.Remove("Total");

                    if (data.Count > 0)
                    {
                        foreach (var item in data)
                        {
                            store.StoreValues.Add("28-06-2019", item);
                        }
                    }
                    else
                    {
                        store.StoreValues.Add("28-06-2019", "");
                    }

                    store.StoreValues.Add(TotalkeyValuePair.Key, TotalkeyValuePair.Value);

                }
            });

            ConvertStoreToCSV(csvFileStore, @"C:\Users\pkakadiy\Downloads\Application\Task2.csv");
        }

        public override void PerformTask3()
        {
            List<Store> csvFileStore = GetCSVFileStore(@"C:\Users\pkakadiy\Downloads\Application\Base.csv");

            ParameterModel dataModel = new ParameterModel()
            {
                StartDate = "2019-05-27",
                project_id = "roll-d-mobile-app",
                ReportType = true
            };

            List<Store> fromAPICSVFileStore = GetFromAPICSVStore(@"C:\Users\pkakadiy\Downloads\Application\FromAPI.csv", dataModel);

            /*try to remove 2 rows e.g. Roll'd - 140 William Street and Roll'd - Australia Square*/

            var recordOne = csvFileStore.First(s => s.Name == "Roll'd - 140 William Street");

            var recordTwo = csvFileStore.First(s => s.Name == "Roll'd - Australia Square");

            csvFileStore.Remove(recordOne);

            csvFileStore.Remove(recordTwo);

            ConvertStoreToCSV(csvFileStore, @"C:\Users\pkakadiy\Downloads\Application\Task3.csv");

            var recordOneFromAPI = fromAPICSVFileStore.First(s => s.Name == "Roll'd - 140 William Street");

            var recordTwoFromAPI = fromAPICSVFileStore.First(s => s.Name == "Roll'd - Australia Square");

            csvFileStore.Add(recordOneFromAPI);

            csvFileStore.Add(recordTwoFromAPI);

            ConvertStoreToCSV(csvFileStore, @"C:\Users\pkakadiy\Downloads\Application\Task3_After_Row_Added.csv");
        }
    }
}
