

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReadingCaptureFile
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //List<Pcap> o;
            //using (FileStream s = File.Open("a.json", FileMode.Open))
            //using (StreamReader sr = new StreamReader(s))
            //using (JsonReader reader = new JsonTextReader(sr))
            //{
            //    reader.SupportMultipleContent = true;

            //    JsonSerializer serializer = new();

            //    while (reader.Read())
            //    {
            //        //    // deserialize only when there's "{" character in the stream
            //        //    if (reader.TokenType == JsonToken.StartObject)
            //        //    {
            //        o = serializer.Deserialize<List<Pcap>>(reader);
            //        //    }
            //    }
            //}

            //string json = File.ReadAllText(jsonFilePath);
            //Dictionary<string, object> json_Dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            //foreach (var item in json_Dictionary)
            //{
            //    // parse here
            //}

            //using (StreamReader r = new StreamReader("a.json"))
            //{
            //    string json = r.ReadToEnd();
            //    dynamic array = JsonConvert.DeserializeObject(json);
            //    foreach (var item in array)
            //    {                    
            //        Console.WriteLine("{0} {1}", item._source.layers.frame, item._source.layers.ip);
            //    }
            //}
            //List<Pcap> items;
            //using (StreamReader r = new StreamReader("a.json"))
            //{
            //    string json = r.ReadToEnd();
            //     items = JsonConvert.DeserializeObject<List<Pcap>>(json); //Use a lot of memeory
            //}
            StringBuilder querystr = new StringBuilder();

            string outputFolder = "L3Mess/";
            var jsonDir = Directory.EnumerateFiles(outputFolder).Where(x => x.Contains(".json")).ToList();
            foreach (var jsonFile in jsonDir)
            {
                var item = ExtractJsonFile(jsonFile); 
                querystr.Append(item + Environment.NewLine); 
            }
            AdoCommand(querystr.ToString());


        }

        private static string ExtractJsonFile(string filePath)
        {
            Console.WriteLine($"***********Read {Path.GetFileName(filePath)}*************");
            var fileWExt = Path.GetFileNameWithoutExtension(filePath);
            var TestId = fileWExt.Split("_")[1];
            string FullQuery = string.Empty;
            string json;
            using (StreamReader r = new StreamReader(filePath))
            {
                json = r.ReadToEnd();
                var rw = JArray.Parse(json);
                var jsonData = rw.Children();
                var jsonData2 = jsonData.Select(u => u != null && u.Type == JTokenType.Property);

                List<JToken> tokens = jsonData.Children().Children().ToList();
                foreach (var item in rw)
                {
                    bool showQuery = false;
                    string qrSt = string.Empty; string vaSt = string.Empty;
                    //var dd =  item.ToObject<Pcap>();                   

                    var ddddd = item.SelectTokens("_source.layers.frame").FirstOrDefault()["frame.protocols"].ToString();
                    if (ddddd.Contains("lte_rrc"))
                    {
                        var Frame = item.SelectTokens("_source.layers.frame").FirstOrDefault();
                        var time = Convert.ToDouble(Frame["frame.time_epoch"].ToString());//Frame["frame.time"];
                         //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)time);
                        var Tokendt = FromUnixTime((long)time);
                        var TokenNo = Frame["frame.number"];                      
                        
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt += $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}','{TokenNo}'";
                        var lte_rrc = item.SelectToken("_source.layers.lte_rrc");
                        var cnteventA1 = lte_rrc.ToString().Contains("lte-rrc.eventA1_element");
                        if (cnteventA1)
                        {
                            showQuery = true;
                            qrSt += $",A1event";
                            vaSt += $",1";
                            // Console.WriteLine(lte_rrc);
                            // Console.WriteLine(cnteventA1);
                        }
                        var cnteventA2 = lte_rrc.ToString().Contains("lte-rrc.eventA2_element");
                        if (cnteventA2)
                        {
                            showQuery = true;
                            qrSt += $",A2event";
                            vaSt += $",1";
                            //  Console.WriteLine(lte_rrc);
                            // Console.WriteLine(cnteventA2);
                        }
                        var cnteventA3 = lte_rrc.ToString().Contains("lte-rrc.eventA3_element");
                        if (cnteventA3)
                        {
                            showQuery = true;
                            qrSt += $",A3event";
                            vaSt += $",1";
                            //Console.WriteLine(lte_rrc);
                            //Console.WriteLine(cnteventA3);
                        }
                        var cnteventA4 = lte_rrc.ToString().Contains("lte-rrc.eventA4_element");
                        if (cnteventA4)
                        {
                            showQuery = true;
                            qrSt += $",A4event";
                            vaSt += $",1";
                            //Console.WriteLine(lte_rrc);
                            //Console.WriteLine(cnteventA4);
                        }
                        var cnteventA5 = lte_rrc.ToString().Contains("lte-rrc.eventA5_element");
                        if (cnteventA5)
                        {
                            showQuery = true;
                            qrSt += $",A5event";
                            vaSt += $",1";
                            //Console.WriteLine(lte_rrc);
                            //Console.WriteLine(cnteventA5);
                        }
                        var cnteventA6 = lte_rrc.ToString().Contains("lte-rrc.eventA6_element");
                        if (cnteventA6)
                        {
                            showQuery = true;
                            qrSt += $",A6event";
                            vaSt += $",1";
                            Console.WriteLine(lte_rrc);
                            Console.WriteLine(cnteventA6);
                        }
                        var cnteventB1 = lte_rrc.ToString().Contains("lte-rrc.eventB1_element");
                        if (cnteventB1)
                        {
                            showQuery = true;
                            qrSt += $",B1event";
                            vaSt += $",1";
                            //Console.WriteLine(lte_rrc);
                            //Console.WriteLine(cnteventB1);
                        }
                        var cnteventNB1 = lte_rrc.ToString().Contains("lte-rrc.eventB1-NR_element");
                        if (cnteventNB1)
                        {
                            showQuery = true;
                            qrSt += $",B1NRevent";
                            vaSt += $",1";
                            //Console.WriteLine(lte_rrc);
                            //Console.WriteLine(cnteventNB1);
                        }
                        var cnteventB2 = lte_rrc.ToString().Contains("lte-rrc.eventB2_element");
                        if (cnteventB2)
                        {
                            showQuery = true;
                            qrSt += $",B2event";
                            vaSt += $",1";
                            //Console.WriteLine(lte_rrc);
                            //Console.WriteLine(cnteventB2);
                        }
                        var cnteventNB2 = lte_rrc.ToString().Contains("lte-rrc.eventB2-NR_element");
                        if (cnteventNB2)
                        {
                            showQuery = true;
                            qrSt += $",B2Nrevent";
                            vaSt += $",1";
                            //Console.WriteLine(lte_rrc);
                            //Console.WriteLine(cnteventNB2);
                        }
                    }
                    qrSt += ")";
                    vaSt += ")";
                    //var regex = new Regex(@"_source.layers.frame$");
                    //IEnumerable<string> objects = item.SelectTokensWithRegex<string>(regex);
                    if (showQuery)
                    {
                        Console.WriteLine($"{qrSt} {vaSt}");
                        FullQuery += $"\n {qrSt} {vaSt};\n";
                    }

                }
            }
            return FullQuery;
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
        private static int AdoCommand(string testResult)
        {
            int res = -250;
            using (SqlConnection cnn = new SqlConnection("Server = 185.192.112.74, 1561; Database = TmpKap400; User Id = sa; Password = Pr0b2001@ct1VE; Application Name = pcapProcess"))
            {
                using var cmm = new SqlCommand();
                try
                {
                    cmm.Connection = cnn;
                    cmm.CommandType = CommandType.Text;
                    cmm.CommandText = testResult;
                    cmm.CommandTimeout = 600;
                    cnn.Open();
                    res = cmm.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message} \n ReadingPcapJson@ {DateTime.Now} ,sql={testResult}");
                }
                finally
                {
                    cnn.Close();
                }
            }
            return res;
        }

    }
}
public static class DatetTimeExtention
{
    public static DateTime FromUnixTime222(this long unixTime)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddSeconds(unixTime);
    }
}
public class Pcap
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _score { get; set; }
        public Source _source { get; set; }
    }

    public class Source
    {
        public Layer layers { get; set; }
    }

public class Layer
{
    public JToken frame { get; set; }
    public JToken ip { get; set; }
    public JToken upd { get; set; }
    public JToken gsmtap { get; set; }
    public JToken lte_rrc { get; set; }

    public JToken nas_eps {get;set;}
        public JToken gsm_a_dtap { get; set; }
    }
    public static class JTokenExtention
    {
        public static IEnumerable<T> SelectTokensWithRegex<T>(this JToken jsonReader, Regex regex)
        {
            Newtonsoft.Json.JsonSerializer serializer = new ();
            while (jsonReader.HasValues)
            {
                if (regex.IsMatch(jsonReader.Value<String>()))
                {
                    yield return JsonConvert.DeserializeObject<T>(jsonReader.Path);
                }
            }
        }
    
}

