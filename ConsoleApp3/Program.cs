

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            var fileWExt = Path.GetFileNameWithoutExtension(filePath);
            var TestId = fileWExt.Split("_")[1];
            string FullQuery = string.Empty;
            bool showQuery = false;
            string jsonString = File.ReadAllText(filePath);//.Replace("\n","").Replace("\r","");


            ReadOnlySpan<byte> jsonReadOnlySpan = File.ReadAllBytes(filePath);

            // Read past the UTF-8 BOM bytes if a BOM exists.
            if (jsonReadOnlySpan.StartsWith(Utf8Bom))
            {
                jsonReadOnlySpan = jsonReadOnlySpan.Slice(Utf8Bom.Length);
            }
            var reader = new Utf8JsonReader(jsonReadOnlySpan);
            int count = 0;
            int total = 0;
            bool isagain = false;
            DateTime Tokendt = DateTime.Now;
            int TokenNo = 0;
            byte[] ToktimeEpoch = Encoding.UTF8.GetBytes("frame.time_epoch");
            byte[] TokNo = Encoding.UTF8.GetBytes("frame.number");
            byte[] a1Event = Encoding.UTF8.GetBytes("lte-rrc.a1_Threshold");
            byte[] a2Event = Encoding.UTF8.GetBytes("lte-rrc.a2_Threshold");
            byte[] a3Event = Encoding.UTF8.GetBytes("lte-rrc.a3_Threshold");
            byte[] a4Event = Encoding.UTF8.GetBytes("lte-rrc.a4_Threshold");
            byte[] sCodecL1Event = Encoding.UTF8.GetBytes("Codec Bitmap for SysID 1");
            byte[] sCodecL2Event = Encoding.UTF8.GetBytes("Codec Bitmap for SysID 2");
            byte[] crEvent = Encoding.UTF8.GetBytes("rrc.rrcConnectionRequest_element");//rrc.rrcConnectionRequest_element
            byte[] rccCsEvent = Encoding.UTF8.GetBytes("rrc.rrcConnectionSetup_r3_element");
            byte[] cscEvent = Encoding.UTF8.GetBytes("rrc.rrcConnectionSetupComplete_element"); //rrc.rrcConnectionSetupComplete_element
            byte[] ddtEvent = Encoding.UTF8.GetBytes("gsm_a.dtap.msg_cc_type"); //gsm_a.dtap.gsm_a.dtap.msg_cc_type
            byte[] crrcEvent = Encoding.UTF8.GetBytes("rrc.releaseCause"); //rrc.rrcConnectionRelease_tree.rrc.releaseCause
            byte[] crcEvent = Encoding.UTF8.GetBytes("rrc.rrcConnectionReleaseComplete_element");


            while (reader.Read())
            {
                string qrSt = string.Empty; string vaSt = string.Empty;
                JsonTokenType tokenType = reader.TokenType;
                switch (tokenType)
                {
                    case JsonTokenType.StartObject:
                        total++;
                        break;
                    case JsonTokenType.Null:
                        break;
                    case JsonTokenType.PropertyName:
                        if (reader.ValueTextEquals(ToktimeEpoch))
                        {
                            // Assume valid JSON, known schema
                            reader.Read();
                            //var ddd = reader.GetString();
                            var time = Convert.ToDouble(reader.GetString());
                            Tokendt = FromUnixTime((long)time);
                            break;
                        }
                        if (reader.ValueTextEquals(TokNo))
                        {
                            // Assume valid JSON, known schema
                            reader.Read();
                            TokenNo = Convert.ToInt32(reader.GetString());
                            break;
                        }
                        if (reader.ValueTextEquals(a1Event)) //event1
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            // Assume valid JSON, known schema
                            reader.Read();
                            var ddd = reader.GetString();
                            qrSt += $",Event,v1 )";
                            vaSt += $",'A1event','{Encoding.UTF8.GetString(a1Event)}:{ddd}')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        if (reader.ValueTextEquals(a2Event)) //event2
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            // Assume valid JSON, known schema
                            reader.Read();
                            var ddd = reader.GetString();
                            qrSt += $",Event,v2 )";
                            vaSt += $",'A2event','{Encoding.UTF8.GetString(a2Event)}:{ddd}')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        if (reader.ValueTextEquals(a3Event)) //event3
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            // Assume valid JSON, known schema
                            reader.Read();
                            var ddd = reader.GetString();
                            qrSt += $",Event,v3 )";
                            vaSt += $",'A3event','{Encoding.UTF8.GetString(a3Event)}:{ddd}')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        if (reader.ValueTextEquals(a4Event)) //event4
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            // Assume valid JSON, known schema
                            reader.Read();
                            var ddd = reader.GetString();
                            qrSt += $",Event,v4 )";
                            vaSt += $",'A4event','{Encoding.UTF8.GetString(a4Event)}:{ddd}')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        //|| reader.ValueTextEquals(sCodecL2Event)
                        if (reader.ValueTextEquals(sCodecL1Event)) //Codec Bitmap for SysID 1 , Codec Bitmap for SysID 2
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            reader.Read();
                            ReadOnlySpan<byte> jsonElement = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                            //رسیدن به شروع آبجکت 
                            var dd = Encoding.UTF8.GetString(jsonElement); //JsonToken.StartObject for SysId1
                            string v1Str = string.Empty;
                            while (reader.TokenType != JsonTokenType.EndObject)
                            {
                                reader.Read();
                                if (reader.TokenType != JsonTokenType.EndObject)
                                {
                                    var vvv = reader.GetString();
                                    if (vvv.StartsWith("gsm_a"))
                                    {
                                        v1Str += vvv + ":";
                                    }
                                    else
                                    {
                                        v1Str += vvv + ",";
                                    }
                                }
                            }
                            reader.Read(); reader.Read(); reader.Read(); reader.Read(); reader.Read();
                            string v2Str = string.Empty;
                            if (reader.ValueTextEquals(sCodecL2Event))
                            {
                                reader.Read();
                                ReadOnlySpan<byte> jsonElement2 = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                                //رسیدن به شروع آبجکت 
                                var dd2 = Encoding.UTF8.GetString(jsonElement2); //JsonToken.StartObject for SysId2                                
                                while (reader.TokenType != JsonTokenType.EndObject)
                                {
                                    reader.Read();
                                    if (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        var vvv = reader.GetString();
                                        if (vvv.StartsWith("gsm_a"))
                                        {
                                            v2Str += vvv + ":";
                                        }
                                        else
                                        {
                                            v2Str += vvv + ",";
                                        }
                                    }
                                }
                            }
                            // Assume valid JSON, known schema
                            if (!string.IsNullOrEmpty(v1Str))
                            {
                                qrSt += $",Event ,V1)";
                                vaSt += $",'WCDMA Supported Codec List','{v1Str}')";
                            }
                            if (!string.IsNullOrEmpty(v2Str))
                            {
                                qrSt += $",V2)";
                                vaSt += $",'{v2Str}')";
                            }
                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        if (reader.ValueTextEquals(crEvent)) //rrc.rrcConnectionRequest_element
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            // Assume valid JSON, known schema
                            //reader.Read();
                            var ddd = reader.GetString();
                            qrSt += $",Event )";
                            vaSt += $",'RRC Connection Request')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        if (reader.ValueTextEquals(rccCsEvent)) //rrc.rrcConnectionSetup_r3_element
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            // Assume valid JSON, known schema
                            //reader.Read();
                            var ddd = reader.GetString();
                            qrSt += $",Event )";
                            vaSt += $",'RCC Connection Setup')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        if (reader.ValueTextEquals(cscEvent)) //rrc.rrcConnectionSetupComplete_element
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            // Assume valid JSON, known schema
                            //reader.Read();
                            var ddd = reader.GetString();
                            qrSt += $",Event )";
                            vaSt += $",'RRCConnectionSetupComplete(cs-domain)(ps-domain)')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        if (reader.ValueTextEquals(ddtEvent))
                        {
                            //DownlinkDirectTransfer(cs-domain)(DTAP) (CC) Disconnect,UplinkDirectTransfer(cs-domain)(DTAP) (CC) Alerting
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            // Assume valid JSON, known schema
                            reader.Read();
                            var ddd = reader.GetString();
                            switch (ddd)
                            {
                                case "0x25":
                                    qrSt += $",Event )";
                                    vaSt += $",'DownlinkDirectTransfer(cs-domain)(DTAP) (CC) Disconnect')";
                                    break;
                                case "0x01":
                                    qrSt += $",Event )";
                                    vaSt += $",'UplinkDirectTransfer(cs-domain)(DTAP) (CC) Alerting')";
                                    break;
                                case "0x02":
                                    qrSt += $",Event )";
                                    vaSt += $",'domain(DTAP) (CC) Call Proceeding')";
                                    break;
                                case "0x05":
                                    qrSt += $",Event )";
                                    vaSt += $",'DownlinkDirectTransfer(cs-domain)(DTAP) (CC) Setup')";
                                    break;

                                /*Call End*/
                                case "0x2a":
                                    qrSt += $",Event,V1 )";
                                    vaSt += $",'(DTAP) (CC) Release Complete','Cause - (28) Invalid number format (incomplete number)')";
                                    break;
                                /*Call End*/
                                case "0x2d":
                                    qrSt += $",Event )";
                                    vaSt += $",'(DTAP) (CC) Release')";
                                    break;
                                /*Call End*/
                                case "0x3f":
                                    qrSt += $",Event )";
                                    vaSt += $",'(DTAP) (RR) Immediate Assignment')";
                                    break;
                                /*Call End*/
                                case "0x24":
                                    qrSt += $",Event )";
                                    vaSt += $",'(DTAP) (MM) CM Service Request')";
                                    break;
                            }

                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        if (reader.ValueTextEquals(crcEvent)) //RRC Release Complete
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            // Assume valid JSON, known schema
                            //reader.Read();
                            var ddd = reader.GetString();
                            qrSt += $",Event )";
                            vaSt += $",'RRC Release Complete')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        if (reader.ValueTextEquals(crrcEvent)) //RRC abnormal Connection Release and RRC Connection Release
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenNo,TokenTime";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}',{TokenNo},'{Tokendt}'";
                            // Assume valid JSON, known schema
                            reader.Read();
                            var ddd = Convert.ToInt32(reader.GetString());
                            if (ddd == 0)
                            {
                                qrSt += $",Event,V1 )";
                                vaSt += $",'RRC Connection Release','rrc.releaseCause:{ddd}')";
                            }
                            else
                            {
                                qrSt += $",Event ,V1)";
                                vaSt += $",'RRC abnormal Connection Release','rrc.releaseCause:{ddd}')";
                            }
                            FullQuery += $"{qrSt} {vaSt};\n";
                            break;
                        }
                        break;
                }

            }

            //var ddd  =  JsonDocument.Parse(jsonString);
            //var topic = ddd.RootElement.EnumerateArray();

            //foreach (var item in topic)
            //{
            //  var source=  item.EnumerateObject().FirstOrDefault(it => it.Name.Contains("_source") && it.Value.ValueKind == JsonValueKind.Object).Value;
            //  var layer = source.EnumerateObject().FirstOrDefault(it => it.Name.Contains("layers") && it.Value.ValueKind == JsonValueKind.Object).Value;
            //    //   .Where(it => it.Value.ValueKind == JsonValueKind.Array && it.Name == "frame.time_epoch")
            //    //.SelectMany(it => it.Value.EnumerateArray().Select(that => that.GetString()))
            //}


            // FullQuery = anotherMethod(TestId, FullQuery, jsonString);
            // if (showQuery)
            return FullQuery;
        }

        private static string anotherMethod(string TestId, string FullQuery, string jsonString)
        {
            var items = System.Text.Json.JsonSerializer.Deserialize<List<Pcap>>(jsonString);
            foreach (var item in items)
            {
                string qrSt = string.Empty; string vaSt = string.Empty;
                var time = Convert.ToDouble(item._source.layers.frame?.GetProperty("frame.time_epoch").ToString());
                var Tokendt = FromUnixTime((long)time);
                var TokenNo = item._source.layers.frame?.GetProperty("frame.number");

                if (item._source.layers.lte_rrc != null)
                {
                    var lte_rrc = item._source.layers.lte_rrc.ul_dcch_msg_element;
                    var cnteventA1 = lte_rrc.ToString().Contains("lte-rrc.eventA1_element");
                    if (cnteventA1)
                    {
                        var v1 = lte_rrc.Value.GetProperty("lte-rrc.eventA1_element.lte-rrc.a1_Threshold").ToString();
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                        qrSt += $",Event ";
                        vaSt += $",'A1event'";
                        if (v1 != null)
                        {
                            qrSt += ",V1";
                            vaSt += $",'{v1}'";
                        }
                        FullQuery += $"{qrSt}) {vaSt});\n";
                    }
                    var cnteventA2 = lte_rrc.ToString().Contains("lte-rrc.eventA2_element");
                    if (cnteventA2)
                    {
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                        qrSt += $",Event  ";
                        vaSt += $",'A2event'";

                        var dd = lte_rrc.Value.ToString().Contains("lte-rrc.a2_Threshold");
                        var v2 = lte_rrc.Value.GetProperty("lte-rrc.a2_Threshold").ToString();
                        if (v2 != null)
                        {
                            qrSt += ",V2";
                            vaSt += $",'{v2}'";
                        }
                        FullQuery += $"{qrSt}) {vaSt});\n";
                    }
                    var cnteventA3 = lte_rrc.ToString().Contains("lte-rrc.eventA3_element");
                    if (cnteventA3)
                    {
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                        qrSt += $",Event ";
                        vaSt += $",'A3event'";
                        var v3 = lte_rrc.Value.GetProperty("lte-rrc.eventA3_element.lte-rrc.a2_Threshold").ToString();
                        if (v3 != null)
                        {
                            qrSt += ",V3";
                            vaSt += $",'{v3}'";
                        }
                        FullQuery += $"{qrSt}) {vaSt});\n";
                    }
                    var cnteventA4 = lte_rrc.ToString().Contains("lte-rrc.eventA4_element");
                    if (cnteventA4)
                    {
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                        qrSt += $",Event) ";
                        vaSt += $",'A4event')";
                        FullQuery += $"{qrSt} {vaSt};\n";
                    }
                    var cnteventA5 = lte_rrc.ToString().Contains("lte-rrc.eventA5_element");
                    if (cnteventA5)
                    {
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                        qrSt += $",Event) ";
                        vaSt += $",'A5event')";
                        FullQuery += $"{qrSt} {vaSt};\n";

                    }
                    var cnteventA6 = lte_rrc.ToString().Contains("lte-rrc.eventA6_element");
                    if (cnteventA6)
                    {
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                        qrSt += $",Event ) ";
                        vaSt += $",'A6event')";
                        FullQuery += $" {qrSt} {vaSt};\n";
                    }
                    var cnteventB1 = lte_rrc.ToString().Contains("lte-rrc.eventB1_element");
                    if (cnteventB1)
                    {
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                        qrSt += $",Event) ";
                        vaSt += $",'B1event')";
                        FullQuery += $"{qrSt} {vaSt};\n";
                    }
                    var cnteventNB1 = lte_rrc.ToString().Contains("lte-rrc.eventB1-NR_element");
                    if (cnteventNB1)
                    {
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                        qrSt += $",Event)";
                        vaSt += $",'B1NRevent')";
                        FullQuery += $"{qrSt} {vaSt};\n";
                    }
                    var cnteventB2 = lte_rrc.ToString().Contains("lte-rrc.eventB2_element");
                    if (cnteventB2)
                    {
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                        qrSt += $",Event) ";
                        vaSt += $",'B2event')";
                        FullQuery += $"{qrSt} {vaSt};\n";
                    }
                    var cnteventNB2 = lte_rrc.ToString().Contains("lte-rrc.eventB2-NR_element");
                    if (cnteventNB2)
                    {
                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                        qrSt += $",Event) ";
                        vaSt += $",'B2Nrevent')";
                        FullQuery += $"{qrSt} {vaSt};\n";
                    }
                }
            }

            return FullQuery;
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
                    var Frame = item.SelectTokens("_source.layers.frame").FirstOrDefault();
                    var time = Convert.ToDouble(Frame["frame.time_epoch"].ToString());//Frame["frame.time"];
                                                                                      //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)time);
                    var Tokendt = FromUnixTime((long)time);
                    var TokenNo = Frame["frame.number"];

                    var ddddd = item.SelectTokens("_source.layers.frame").FirstOrDefault()["frame.protocols"].ToString();
                    if (ddddd.Contains("lte_rrc"))
                    {

                        var lte_rrc = item.SelectToken("_source.layers.lte_rrc");

                        var cnteventA1 = lte_rrc.ToString().Contains("lte-rrc.eventA1_element");
                        if (cnteventA1)
                        {
                            var v1 = lte_rrc.SelectToken("lte-rrc.eventA1_element.lte-rrc.a1_Threshold");

                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event ";
                            vaSt += $",'A1event'";
                            if (v1 != null)
                            {
                                qrSt += ",V1";
                                vaSt += $",'{v1.Value<string>()}'";
                            }
                            FullQuery += $"{qrSt}) {vaSt});\n";
                            // Console.WriteLine(lte_rrc);
                            // Console.WriteLine(cnteventA1);
                        }
                        var cnteventA2 = lte_rrc.ToString().Contains("lte-rrc.eventA2_element");
                        if (cnteventA2)
                        {
                            //foreach (var sourcePair in lte_rrc)
                            //{
                            //    if (sourcePair.Value<string>() == "eventA2_element")
                            //    {
                            //        var arole = "l";
                            //    }
                            //}
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event  ";
                            vaSt += $",'A2event'";
                            var v2 = lte_rrc.SelectToken("lte-rrc.DL_DCCH_Message_element.lte-rrc.eventA2_element.lte-rrc.a2_Threshold");
                            if (v2 != null)
                            {
                                qrSt += ",V2";
                                vaSt += $",'{v2.Value<string>()}'";
                            }
                            FullQuery += $"{qrSt}) {vaSt});\n";
                        }
                        var cnteventA3 = lte_rrc.ToString().Contains("lte-rrc.eventA3_element");
                        if (cnteventA3)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event ";
                            vaSt += $",'A3event'";
                            var v3 = lte_rrc.SelectToken("lte-rrc.eventA3_element.lte-rrc.a2_Threshold");
                            if (v3 != null)
                            {
                                qrSt += ",V3";
                                vaSt += $",'{v3.Value<string>()}'";
                            }
                            FullQuery += $"{qrSt}) {vaSt});\n";
                        }
                        var cnteventA4 = lte_rrc.ToString().Contains("lte-rrc.eventA4_element");
                        if (cnteventA4)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event) ";
                            vaSt += $",'A4event')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                        }
                        var cnteventA5 = lte_rrc.ToString().Contains("lte-rrc.eventA5_element");
                        if (cnteventA5)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event) ";
                            vaSt += $",'A5event')";
                            FullQuery += $"{qrSt} {vaSt};\n";

                        }
                        var cnteventA6 = lte_rrc.ToString().Contains("lte-rrc.eventA6_element");
                        if (cnteventA6)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event ) ";
                            vaSt += $",'A6event')";
                            FullQuery += $" {qrSt} {vaSt};\n";
                        }
                        var cnteventB1 = lte_rrc.ToString().Contains("lte-rrc.eventB1_element");
                        if (cnteventB1)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event) ";
                            vaSt += $",'B1event')";
                            FullQuery += $"{qrSt} {vaSt};\n";

                        }
                        var cnteventNB1 = lte_rrc.ToString().Contains("lte-rrc.eventB1-NR_element");
                        if (cnteventNB1)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event)";
                            vaSt += $",'B1NRevent')";
                            FullQuery += $"{qrSt} {vaSt};\n";

                        }
                        var cnteventB2 = lte_rrc.ToString().Contains("lte-rrc.eventB2_element");
                        if (cnteventB2)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event) ";
                            vaSt += $",'B2event')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                        }
                        var cnteventNB2 = lte_rrc.ToString().Contains("lte-rrc.eventB2-NR_element");
                        if (cnteventNB2)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event) ";
                            vaSt += $",'B2Nrevent')";
                            FullQuery += $"{qrSt} {vaSt};\n";
                        }
                    }
                    var rrcUL = item.SelectTokens("_source.layers.['rrc.UL_DCCH_Message_element']");
                    if (rrcUL != null)
                    {
                        var cnteventA1 = rrcUL.Contains("rrc.rrcConnectionReleaseComplete_element");
                        if (cnteventA1)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event ";
                            vaSt += $",'RRC Release Complete'";
                            //if (v1 != null)
                            //{
                            //    qrSt += ",V1";
                            //    vaSt += $",'{v1.Value<string>()}'";
                            //}
                            FullQuery += $"{qrSt}) {vaSt});\n";
                        }
                        cnteventA1 = rrcUL.Contains("rrc.rrcConnectionRelease_tree");
                        if (cnteventA1)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event ";
                            vaSt += $",'RRC Normal Connection Release'";
                            //if (v1 != null)
                            //{
                            //    qrSt += ",V1";
                            //    vaSt += $",'{v1.Value<string>()}'";
                            //}
                            FullQuery += $"{qrSt}) {vaSt});\n";
                        }
                        cnteventA1 = rrcUL.Contains("gsm_a.dtap.msg_cc_type");
                        if (cnteventA1)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event ";
                            vaSt += $",'DownlinkDirectTransfer(cs-domain)(DTAP) (CC) Disconnect'";
                            //if (v1 != null)
                            //{
                            //    qrSt += ",V1";
                            //    vaSt += $",'{v1.Value<string>()}'";
                            //}
                            FullQuery += $"{qrSt}) {vaSt});\n";
                        }
                        cnteventA1 = rrcUL.Contains("rrc.rrcConnectionSetupComplete_element");
                        if (cnteventA1)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event ";
                            vaSt += $",'RRCConnectionSetupComplete(cs-domain)(ps-domain)'";
                            //if (v1 != null)
                            //{
                            //    qrSt += ",V1";
                            //    vaSt += $",'{v1.Value<string>()}'";
                            //}
                            FullQuery += $"{qrSt}) {vaSt});\n";
                        }
                        cnteventA1 = rrcUL.Contains("rrc.rrcConnectionSetup_r3_element");
                        if (cnteventA1)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event ";
                            vaSt += $",'RRCConnectionSetup'";
                            //if (v1 != null)
                            //{
                            //    qrSt += ",V1";
                            //    vaSt += $",'{v1.Value<string>()}'";
                            //}
                            FullQuery += $"{qrSt}) {vaSt});\n";
                        }
                        cnteventA1 = rrcUL.Contains("rrc.rrcConnectionRequest_element");
                        if (cnteventA1)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event ";
                            vaSt += $",'RRC Connection Request'";
                            //if (v1 != null)
                            //{
                            //    qrSt += ",V1";
                            //    vaSt += $",'{v1.Value<string>()}'";
                            //}
                            FullQuery += $"{qrSt}) {vaSt});\n";
                        }
                        cnteventA1 = rrcUL.Contains("Codec Bitmap for SysID 1");
                        if (cnteventA1)
                        {
                            showQuery = true;
                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
                            qrSt += $",Event ";
                            vaSt += $",'Supported Codec List'";
                            //if (v1 != null)
                            //{
                            //    qrSt += ",V1";
                            //    vaSt += $",'{v1.Value<string>()}'";
                            //}
                            FullQuery += $"{qrSt}) {vaSt});\n";
                        }
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
            using (SqlConnection cnn = new SqlConnection("Server = 185.192.112.74, 1561; Database = TmpKap400; User Id = sa; Password = Pr0b2001@ct1VE"))
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
    public Layer2 layers { get; set; }
}
public class Layer
{
    public JToken frame { get; set; }
    public JToken ip { get; set; }
    public JToken upd { get; set; }
    public JToken gsmtap { get; set; }
    public JToken lte_rrc { get; set; }

    public JToken nas_eps { get; set; }
    public JToken gsm_a_dtap { get; set; }

    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
public class Layer2
{
    public JsonElement? frame { get; set; }
    public JsonElement? ip { get; set; }
    public JsonElement? upd { get; set; }
    public JsonElement? gsmtap { get; set; }
    public LTErrc? lte_rrc { get; set; }

    public JsonElement? nas_eps { get; set; }
    public JsonElement? gsm_a_dtap { get; set; }
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public class LTErrc
{
    string lte_rrc { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("lte-rrc.UL_DCCH_Message_element")]
    public JsonElement? ul_dcch_msg_element { get; set; }
}
public static class JTokenExtention
{
    public static IEnumerable<T> SelectTokensWithRegex<T>(this JToken jsonReader, Regex regex)
    {
        Newtonsoft.Json.JsonSerializer serializer = new();
        while (jsonReader.HasValues)
        {
            if (regex.IsMatch(jsonReader.Value<String>()))
            {
                yield return JsonConvert.DeserializeObject<T>(jsonReader.Path);
            }
        }
    }

}

