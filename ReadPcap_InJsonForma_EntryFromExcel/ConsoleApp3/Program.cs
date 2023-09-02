using OfficeOpenXml;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
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
        private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };
        public static string ExtractJsonFileByJSonSerializer(string filePath)
        {
            var filename = Path.GetFileName(filePath);
            var fileWExt = Path.GetFileNameWithoutExtension(filePath);
            var TestId = fileWExt.Split("_")[1];
            string FullQuery = string.Empty;
            string jsonString = File.ReadAllText(filePath);//.Replace("\n","").Replace("\r","");


            ReadOnlySpan<byte> jsonReadOnlySpan = File.ReadAllBytes(filePath);

            // Read past the UTF-8 BOM bytes if a BOM exists.
            if (jsonReadOnlySpan.StartsWith(Utf8Bom))
            {
                jsonReadOnlySpan = jsonReadOnlySpan.Slice(Utf8Bom.Length);
            }
            var reader = new Utf8JsonReader(jsonReadOnlySpan);
            var OnecCheck = false;
            DateTime Tokendt = DateTime.Now;
            int itemcnt = 0; string parentKey = string.Empty; bool Set3f = false;
            /*ReadKey from excel */
            string path = "sample_data.xlsx";
            FileInfo fileInfo = new FileInfo(path);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; //for ignore check licence

            ExcelPackage package = new ExcelPackage(fileInfo);            
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();

            // get number of rows and columns in the sheet
            int rows = worksheet.Dimension.Rows; // 20
            int columns = worksheet.Dimension.Columns; // 3
            
            Dictionary<string, string> dd = new(); 
            while (reader.Read())
            {
                string qrSt = string.Empty; string vaSt = string.Empty;
                JsonTokenType tokenType = reader.TokenType;
                switch (tokenType)
                {
                    case JsonTokenType.StartObject:
                    case JsonTokenType.Null:
                    case JsonTokenType.EndObject:
                    case JsonTokenType.StartArray:
                    case JsonTokenType.EndArray:
                    //  parentKey = string.Empty;
                        break;
                    //case JsonTokenType.EndObject:                    
                    //case JsonTokenType.EndArray:
                    //    parentKey = string.Empty;
                    //    break;
                    case JsonTokenType.PropertyName:
                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("_index")))
                        {
                            if (dd.Count > 2)
                            {
                                setQuery(dd, TestId, filename);
                            }
                            dd = new Dictionary<string, string>();
                            itemcnt = 0;
                            parentKey = string.Empty;
                            Set3f = false;
                            break;
                        }
                        else
                        {                          
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("frame.time_epoch")))
                            {
                                // Assume valid JSON, known schema
                                reader.Read();
                                //var time = Convert.ToDouble(reader.GetString());
                                //Tokendt = FromUnixTime((long)time);                               
                                dd.Add("Tokendt", reader.GetString());
                                break;
                            }                            
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("frame.number")))
                            {
                                // Assume valid JSON, known schema
                                reader.Read();
                                dd.Add("TokenNo", reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.reportConfigToAddModList")))
                            {
                                parentKey = $"{reader.GetString()}.item {itemcnt}.";
                                itemcnt++;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.SRB_ToAddMod_element")))
                            {
                                parentKey = $"{reader.GetString()}.item {itemcnt}.";
                                itemcnt++;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.DRB_ToAddMod_element")))
                            {
                                parentKey = $"{reader.GetString()}.item {itemcnt}.";
                                itemcnt++;
                            }
                            //this attribute mab by pardon
                            if (reader.ValueTextEquals(Encoding.UTF32.GetBytes("lte-rrc.ReportConfigToAddMod_element")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    itemcnt = 0;
                                }
                                else
                                {
                                    itemcnt++;
                                }
                                parentKey = $"item {itemcnt}.{reader.GetString()}";

                                //using var jsonTags = JsonDocument.ParseValue(ref reader);
                                //var jsonTitle = jsonTags.RootElement.GetProperty("lte-rrc.triggerType");
                            }
                            else
                            {
                                // loop through the worksheet rows and columns
                                for (int i = 2; i <= rows; i++)  //i=1 is header
                                {
                                    //for (int j = 1; j <= columns; j++) //col1=Value1,col2=EventName in Table,col3=Event alias
                                    //{
                                    //    string content = worksheet.Cells[i, j].Value.ToString();
                                    var k_onSheet = worksheet.Cells[i, 1].Value?.ToString();
                                    var v_onSheet = worksheet.Cells[i, 2].Value?.ToString();
                                    if (!string.IsNullOrEmpty(k_onSheet))
                                    {
                                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes(k_onSheet)))
                                        {
                                            if (!string.IsNullOrEmpty(parentKey))
                                            {
                                                var EventName = $"{worksheet.Cells[i, 3].Value}";
                                                if (dd.Any(x => x.Key == EventName))
                                                {
                                                    var exsitKey = dd.First(x => x.Key == EventName);
                                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                                    reader.Read();
                                                    itemparm += $":\"{reader.GetString()}\"";
                                                    var newval = exsitKey.Value + "," + itemparm;
                                                    dd[exsitKey.Key] = newval;
                                                    // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                                }
                                                else
                                                {                                                   
                                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                                    reader.Read();
                                                    itemparm += $":\"{reader.GetString()}\"";
                                                    dd.Add(EventName, itemparm);
                                                }
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(v_onSheet)) //has value.need check value
                                                {

                                                    var EventName = $"{worksheet.Cells[i, 3].Value}";
                                                    var parm = "\"" + reader.GetString() + "\""; //value of key on json file
                                                    reader.Read();  //go to next token
                                                    var valueOnFile = reader.GetString(); //value of next token on json file
                                                    if (v_onSheet == valueOnFile)
                                                    {
                                                        parm += $":\"{reader.GetString()}\"";
                                                        dd.Add(EventName, parm);
                                                    }
                                                }
                                                else
                                                {
                                                    var EventName = $"{worksheet.Cells[i, 3].Value}";
                                                    var parm = "\"" + reader.GetString() + "\"";
                                                    reader.Read();
                                                    parm += $":\"{reader.GetString()}\"";
                                                    dd.Add(EventName, parm);
                                                }
                                           }
                                        }
                                    }                                    
                                    //else
                                    //{
                                        
                                    //        parentKey = reader.GetString() + ">";
                                    //        itemcnt++;
                                     
                                    //}
                                }
                            }
                        }
                        break;
                }
            }
            if (dd.Count > 2) //for last token.
            {
                setQuery(dd, TestId, filename);
            }
            return FullQuery;
        }


        private static void setQuery(Dictionary<string, string> dd, string TestId, string filename)
        {
            StringBuilder fullquery = new StringBuilder();
            foreach (var item in dd.Where(x => x.Key != "Tokendt" && x.Key != "TokenNo"))
            {
                var qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName";
                var vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}'";
                /*DateTime Token*/
                var dt = dd.FirstOrDefault(x => x.Key.Equals("Tokendt")).Value;
                var Tokendt = FromUnixTime(dt);
                //ToString("MM/dd/yyyy hh:mm:ss.fff tt") => Date and Time with Milliseconds
                qrSt += $",TokenTime"; vaSt += $",'{Tokendt.ToString("MM/dd/yyyy hh:mm:ss.fff tt")}'";
                /*Tonken Number*/
                var tNo = dd.FirstOrDefault(x => x.Key.Equals("TokenNo")).Value;

                qrSt += $",TokenNo"; vaSt += $",{tNo}";
                qrSt += $",Event";
                vaSt += $",'{item.Key}'";
                qrSt += $",V1";
                vaSt += $",'{{{item.Value}}}'";
                qrSt += ")";
                vaSt += $");";
                //Console.WriteLine(echPrpp);
                fullquery.Append($"{qrSt}{vaSt}\n");

            }
            if (!string.IsNullOrEmpty(fullquery.ToString()))
            {

                AdoCommand(fullquery.ToString()); ;
                Console.WriteLine(fullquery.ToString());
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
}
