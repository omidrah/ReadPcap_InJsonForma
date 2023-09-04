using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ReadingCaptureFile
{
    public class Program
    {
        public static void Main(string[] args)
        {
            StringBuilder querystr = new StringBuilder();
            string outputFolder = "L3Mess/";
            var jsonDir = Directory.EnumerateFiles(outputFolder).Where(x => x.Contains(".json")).ToList();
            foreach (var jsonFile in jsonDir)
            {
                var item = ExtractJsonFileByJSonSerializer(jsonFile);
                //ExtractJsonFile(jsonFile); 
                if (!string.IsNullOrEmpty(item))
                    querystr.Append(item + Environment.NewLine);
            }
            if (querystr != null)
            {
                AdoCommand(querystr.ToString());
                Console.WriteLine(querystr.ToString());
            }
        }
        
        public static string ExtractJsonFileByJSonSerializer(string filePath)
        {
            var filename = Path.GetFileName(filePath);
            var fileWExt = Path.GetFileNameWithoutExtension(filePath);
            var TestId = fileWExt.Split("_")[1];
            string FullQuery = string.Empty;
            IList<object> result = null;
            string jsonString = File.ReadAllText(filePath);//.Replace("\n","").Replace("\r","");            
            /*ReadKey from excel */
            string path = "sample_data.xlsx";
            FileInfo fileInfo = new(path);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; //for ignore check licence

            ExcelPackage package = new(fileInfo);            
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();

            // get number of rows and columns in the sheet
            int rows = worksheet.Dimension.Rows; // 20
            int columns = worksheet.Dimension.Columns; // 3
            Dictionary<Guid, dicItems> dd = new();

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                using (JsonReader reader1 = new JsonTextReader(streamReader))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new();
                    // read the json from a stream
                    // json size doesn't matter because only a small piece is read at a time from the HTTP request
                    result = serializer.Deserialize<List<object>>(reader1);
                }
            }   
            foreach (var tt in result)
            {
                var ss = Newtonsoft.Json.Linq.JObject.Parse(tt.ToString());                
                var tokenNO = ss.Descendants().OfType<JProperty>().Where(x => x.Name =="frame.number").FirstOrDefault().Value.ToString();
                var tokenDT = ss.Descendants().OfType<JProperty>().Where(x => x.Name == "frame.time_epoch").FirstOrDefault().Value.ToString();
                    //tt.ToString().FirstOrDefault(t => t.ToString() == "frame.time_epoch").ToString();
                for (int i = 2; i <= rows; i++)  //i=1 is header
                {
                    var k_onSheet = worksheet.Cells[i, 3].Value?.ToString();
                    var v_onSheet = worksheet.Cells[i, 4].Value?.ToString();
                    var need_readValue = worksheet.Cells[i, 5].Value?.ToString(); //1 mean value of this key should be read
                    var EventName = $"{worksheet.Cells[i, 1].Value}";
                    var nu = ss.Descendants().OfType<JProperty>().Where(x => x.Name == k_onSheet);
                    IEnumerable<Item> selectItem = null;
                    if (!string.IsNullOrEmpty(v_onSheet))
                    {
                        selectItem = nu.Select(x =>
                        {
                            return
                             new Item
                             {
                                 Name = x.Name.ToString(),
                                 Value = x.HasValues ? x.Value.ToString() : string.Empty,
                                 Path = x.Path.Replace("']['", ".").Replace("']", ".").Replace("['", ".")
                                 //x.Ancestors().OfType<JProperty>().FirstOrDefault().Name
                             };
                        });                    
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(need_readValue) && int.TryParse(need_readValue,out int kkk))
                        {
                            if (kkk == 1)
                            {
                                selectItem = nu.Select(x => new Item
                                {
                                    Name = x.Name.ToString(),
                                    //Value = x.HasValues ? x.Value: string.Empty,
                                    Value = x.HasValues ? x.Value.ToString() : string.Empty,
                                    Path = x.Path.Replace("']['", ".").Replace("']", ".").Replace("['", ".").Replace("\'", "\''")
                                    //x.Ancestors().OfType<JProperty>().FirstOrDefault().Name
                                });
                            }
                        }
                        else
                        {
                            selectItem = nu.Select(x => new Item
                            {
                                Name = x.Name.ToString(),
                                //Value = x.HasValues ? x.Value: string.Empty,
                                Value = string.Empty,
                                Path = x.Path.Replace("']['", ".").Replace("']", ".").Replace("['", ".").Replace("\'", "\''")
                                //x.Ancestors().OfType<JProperty>().FirstOrDefault().Name
                            });
                        }
                    }
                   
                    
                    if (selectItem.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(v_onSheet))
                        {
                            selectItem.ToList().ForEach((it) =>
                                {
                                    if (it.Value.ToString() == v_onSheet && it.Name==k_onSheet ) 
                                    {
                                        dd.Add(Guid.NewGuid(), new dicItems
                                        {
                                            Parent = it.Path,
                                            TokenNo = int.Parse(tokenNO),
                                            Tokendt = tokenDT,
                                            Value = it.Name + " : " + it.Value.ToString(),
                                            Key = EventName
                                        });
                                     }
                                });
                        }
                        else
                        {
                            selectItem.ToList().ForEach((it) =>
                            {
                                dd.Add(Guid.NewGuid(), new dicItems
                                {
                                    Parent = it.Path,
                                    TokenNo = int.Parse(tokenNO),
                                    Tokendt = tokenDT,
                                    Value = string.IsNullOrEmpty(it.Value.ToString()) ? it.Name :it.Name + " : " + it.Value.ToString(),
                                    Key = EventName
                                });
                            });
                        }


                    }

                }
            }          
            if (dd.Count > 2) 
            {
                setQuery(dd, TestId, filename);
            }
            return FullQuery;
        }

        private static void setQuery(Dictionary<Guid, dicItems> dd, string TestId, string filename)
        {
            StringBuilder fullquery = new StringBuilder();
            foreach (var item in dd)
            {
                var qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName";
                var vaSt = $"values ('{item.Key}',{TestId},'{DateTime.Now}','{filename}'";               
                /*DateTime Token*/               
                var Tokendt = FromUnixTime(item.Value.Tokendt);
                //ToString("MM/dd/yyyy hh:mm:ss.fff tt") => Date and Time with Milliseconds
                qrSt += $",TokenTime"; vaSt += $",'{Tokendt.ToString("MM/dd/yyyy hh:mm:ss.fff tt")}'";
                
                qrSt += $",TokenNo"; vaSt += $",{item.Value.TokenNo}";  
                qrSt += $",Event";
                vaSt += $",'{item.Value.Key}'";
                qrSt += $",V1";
                vaSt += $",'{item.Value.Value}'";
                qrSt += $",V2";
                vaSt += $",'{item.Value.Parent}'";
                qrSt += ")";
                vaSt += $");";
                //Console.WriteLine(echPrpp);
                fullquery.Append($"{qrSt}{vaSt}\n");

            }
            if (!string.IsNullOrEmpty(fullquery.ToString()))
            {

                var numrec = AdoCommand(fullquery.ToString()); 
                Console.WriteLine(fullquery.ToString());
                Console.WriteLine("**************************");
                Console.WriteLine($"Number Of Record:{numrec}");
                Console.ReadKey();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unixTime">unixTime is double</param>
        /// <returns></returns>
        public static DateTime FromUnixTime(string unixTime)
        {
            double.TryParse(unixTime, out double res);
            DateTime dateTime2 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            //dateTime2 = dateTime2.AddSeconds(res);//incorrect 
            /*برای بدست آوردن میکروثانیه ها  عدد دریافتی در 1000 ضرب شده و از تابع موردنظر استفاده شده است .*/
            dateTime2 = dateTime2.AddMilliseconds(res * 1000).ToLocalTime();
            return dateTime2;
        }
        private static int AdoCommand(string testResult)
        {
            int res = -250;
            using (SqlConnection cnn = new SqlConnection("Server = 185.192.112.74, 1561; Database = TmpKap400; User Id = sa; Password = gkKMJUhmBqj%SC!w#2d8YbX3DR9@suFVAI)WZLHt*^TcxyGn+vP7paEez(N&5Q64"))
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

    public class dicItems
    {
        public int TokenNo { get; set; }
        public string Tokendt { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Parent { get; set; }
    }

    public class Item
    {        
        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
    }
}
