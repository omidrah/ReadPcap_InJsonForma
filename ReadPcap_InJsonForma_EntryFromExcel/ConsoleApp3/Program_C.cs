

//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Buffers;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace ReadingCaptureFile
//{
//    public class Program_C
//    {
//        public static void Main(string[] args)
//        {
//            //List<Pcap> o;
//            //using (FileStream s = File.Open("a.json", FileMode.Open))
//            //using (StreamReader sr = new StreamReader(s))
//            //using (JsonReader reader = new JsonTextReader(sr))
//            //{
//            //    reader.SupportMultipleContent = true;

//            //    JsonSerializer serializer = new();

//            //    while (reader.Read())
//            //    {
//            //        //    // deserialize only when there's "{" character in the stream
//            //        //    if (reader.TokenType == JsonToken.StartObject)
//            //        //    {
//            //        o = serializer.Deserialize<List<Pcap>>(reader);
//            //        //    }
//            //    }
//            //}

//            //string json = File.ReadAllText(jsonFilePath);
//            //Dictionary<string, object> json_Dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

//            //foreach (var item in json_Dictionary)
//            //{
//            //    // parse here
//            //}

//            //using (StreamReader r = new StreamReader("a.json"))
//            //{
//            //    string json = r.ReadToEnd();
//            //    dynamic array = JsonConvert.DeserializeObject(json);
//            //    foreach (var item in array)
//            //    {                    
//            //        Console.WriteLine("{0} {1}", item._source.layers.frame, item._source.layers.ip);
//            //    }
//            //}
//            //List<Pcap> items;
//            //using (StreamReader r = new StreamReader("a.json"))
//            //{
//            //    string json = r.ReadToEnd();
//            //     items = JsonConvert.DeserializeObject<List<Pcap>>(json); //Use a lot of memeory
//            //}
//            StringBuilder querystr = new StringBuilder();

//            string outputFolder = "L3Mess/";
//            var jsonDir = Directory.EnumerateFiles(outputFolder).Where(x => x.Contains(".json")).ToList();
//            foreach (var jsonFile in jsonDir)
//            {
//                var item = ExtractJsonFileByJSonSerializer(jsonFile);
//                //ExtractJsonFile(jsonFile); 
//                if (!string.IsNullOrEmpty(item))
//                    querystr.Append(item + Environment.NewLine);
//            }
//            if (querystr != null)
//            {
//                AdoCommand(querystr.ToString());
//                Console.WriteLine(querystr.ToString());
//            }


//        }
//        private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };
//        public static string ExtractJsonFileByJSonSerializer(string filePath)
//        {
//            var filename = Path.GetFileName(filePath);
//            var fileWExt = Path.GetFileNameWithoutExtension(filePath);
//            var TestId = fileWExt.Split("_")[1];
//            string FullQuery = string.Empty;
//            bool showQuery = false;
//            string jsonString = File.ReadAllText(filePath);//.Replace("\n","").Replace("\r","");


//            ReadOnlySpan<byte> jsonReadOnlySpan = File.ReadAllBytes(filePath);

//            // Read past the UTF-8 BOM bytes if a BOM exists.
//            if (jsonReadOnlySpan.StartsWith(Utf8Bom))
//            {
//                jsonReadOnlySpan = jsonReadOnlySpan.Slice(Utf8Bom.Length);
//            }
//            var reader = new Utf8JsonReader(jsonReadOnlySpan);
//            int total = 0;
//            DateTime Tokendt = DateTime.Now;
//            int TokenNo = 0;
        
//            byte[] a1Event = Encoding.UTF8.GetBytes("lte-rrc.a1_Threshold");
//            byte[] a2Event = Encoding.UTF8.GetBytes("lte-rrc.a2_Threshold");
//            byte[] a3Event = Encoding.UTF8.GetBytes("lte-rrc.a3_Threshold");
//            byte[] a4Event = Encoding.UTF8.GetBytes("lte-rrc.a4_Threshold"); 
//            byte[] sCodecL2Event = Encoding.UTF8.GetBytes("Codec Bitmap for SysID 2");           
//            byte[] msgmmTyp = Encoding.UTF8.GetBytes("gsm_a.dtap.msg_rr_type");//gsm_a.dtap.msg_rr_type , ناتمام           
//            byte[] lteRrcCons = Encoding.UTF8.GetBytes("lte-rrc.rrcConnectionSetup_element");/*ناتمام*/

//            Dictionary<string, string> dd=new Dictionary<string, string>();
//            while (reader.Read())
//            {
//                string qrSt = string.Empty; string vaSt = string.Empty;
//                JsonTokenType tokenType = reader.TokenType;
//                switch (tokenType)
//                {
//                    case JsonTokenType.StartObject:
//                        total++;
//                        break;
//                    case JsonTokenType.Null:
//                        break;
//                    case JsonTokenType.PropertyName:
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("frame.time_epoch")))
//                        {
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            //var ddd = reader.GetString();
//                            var time = Convert.ToDouble(reader.GetString());
//                            Tokendt = FromUnixTime((long)time);
//                            //dd.Add("Tokendt", reader.GetString());
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("frame.number")))
//                        {
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            //dd.Add("TokenNo", reader.GetString());
//                            TokenNo = Convert.ToInt32(reader.GetString());
//                            break;
//                        }
//                        if (reader.ValueTextEquals(a1Event)) //event1
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            qrSt += $",Event";
//                            vaSt += $",'RRC event A1'";
//                            reader.Read(); //value of lte-rrc.a1_Threshold
//                            var ddd = reader.GetString();
//                            if (!string.IsNullOrEmpty(ddd))
//                            {
//                                qrSt += $",v1";
//                                vaSt += $",'{Encoding.UTF8.GetString(a1Event)}:{ddd}'";
//                                reader.Read();reader.Read();reader.Read();
//                                var v2Title = reader.GetString();
//                                if (!string.IsNullOrEmpty(v2Title))
//                                {
//                                    reader.Read();
//                                    var v2Value = reader.GetString();
//                                    qrSt += $",v2 )";
//                                    vaSt += $",'{v2Title}:{v2Value}')";
//                                }
//                                else
//                                {
//                                    qrSt += $")";
//                                    vaSt += $")";
//                                }
//                                FullQuery += $"{qrSt} {vaSt};\n";
//                            }                            
//                            break;
//                        }
//                        if (reader.ValueTextEquals(a2Event)) //event2
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            qrSt += $",Event";
//                            vaSt += $",'RRC event A2'";
//                            reader.Read(); //value of lte-rrc.a1_Threshold
//                            var ddd = reader.GetString();
//                            if (!string.IsNullOrEmpty(ddd))
//                            {
//                                qrSt += $",v1";
//                                vaSt += $",'{Encoding.UTF8.GetString(a1Event)}:{ddd}'";
//                                reader.Read(); reader.Read(); reader.Read();
//                                var v2Title = reader.GetString();
//                                if (!string.IsNullOrEmpty(v2Title))
//                                {
//                                    reader.Read();
//                                    var v2Value = reader.GetString();
//                                    qrSt += $",v2 )";
//                                    vaSt += $",'{v2Title}:{v2Value}')";
//                                }
//                                else
//                                {
//                                    qrSt += $")";
//                                    vaSt += $")";
//                                }
//                                FullQuery += $"{qrSt} {vaSt};\n";
//                            }
//                            break;
//                        }
                      
//                        if (reader.ValueTextEquals(a3Event)) //event3
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            qrSt += $",Event,v3 )";
//                            vaSt += $",'A3event','{Encoding.UTF8.GetString(a3Event)}:{ddd}')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(a4Event)) //event4
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            qrSt += $",Event,v4 )";
//                            vaSt += $",'A4event','{Encoding.UTF8.GetString(a4Event)}:{ddd}')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB1_element"))) //lte-rrc.eventB1_element     
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            qrSt += $",Event )";
//                            vaSt += $",'RRC event B1')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB1-NR_element"))) //lte-rrc.eventB1-NR_element   
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            qrSt += $",Event )";
//                            vaSt += $",'RRC event B1-NR')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB2_element"))) //lte-rrc.eventB2_element     
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            qrSt += $",Event )";
//                            vaSt += $",'RRC event B2')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB2-NR_element"))) //lte-rrc.eventB2-NR_element   
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            qrSt += $",Event )";
//                            vaSt += $",'RRC event B2-NR')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rb_InformationReconfigList"))) //rrc.rb_InformationReconfigList  
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            qrSt += $",Event,v1 )";
//                            vaSt += $",'WCDMA RB Info','rrc.rb_InformationReconfigList:{ddd}')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.nas_Message"))) //rrc.nas_Message
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            qrSt += $",Event,v3 )";
//                            vaSt += $",'NAS Message','rrc.nas_Message:{ddd}')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.timing_adv"))) //gsm_a.rr.timing_adv
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            qrSt += $",Event,v3 )";
//                            vaSt += $",'Timing Advance','gsm_a.rr.timing_adv:{ddd}')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.srb_ToAddModList"))) //lte-rrc.srb_ToAddModList
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            qrSt += $",Event,v3 )";
//                            vaSt += $",'LTE RRC SRB ','lte-rrc.srb_ToAddModList:{ddd}')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("nas_eps.security_header_type"))) //nas_eps.security_header_type
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            if (ddd == "12")
//                            {
//                                qrSt += $",Event )";
//                                vaSt += $",'Service request')";
//                                FullQuery += $"{qrSt} {vaSt};\n";
//                            }
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.cellUpdate_element"))) //rrc.cellUpdate_element
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema                            
//                            qrSt += $",Event )";
//                            vaSt += $",'WCDMA RRC Cell Update')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rb_InformationReconfigList"))) //rrc.rb_InformationReconfigList
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema                            
//                            var ddd = reader.GetString(); //value rrc.rb_InformationReconfigList
//                            qrSt += $",Event ,v1)";
//                            vaSt += $",'WCDMA RB Infoe','rrc.rb_InformationReconfigList:{ddd}')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.cellUpdateConfirm_tree"))) //rrc.cellUpdateConfirm_tree
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema                           
//                            qrSt += $",Event )";
//                            vaSt += $",'WCDMA RRC Cell Update Confirm')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_ReportModeAperiodic"))) //lte-rrc.cqi_ReportModeAperiodic
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            qrSt += $",Event";
//                            vaSt += $",'RRC CQI Report'";
//                            reader.Read();
//                            string rmqVal = reader.GetString();                            
//                            if(!string.IsNullOrEmpty(rmqVal))
//                            {
//                                qrSt += $",V1";
//                                vaSt += $",'lte-rrc.cqi_ReportModeAperiodic:{rmqVal}'";
//                            }
//                            while(reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_ReportPeriodic")))
//                            {
//                                reader.Read();
//                            }
//                            reader.Read(); //value of lte-rrc.cqi_ReportPeriodic
//                            string rpcVal = reader.GetString();
//                            if (!string.IsNullOrEmpty(rpcVal))
//                            {                                
//                                qrSt += $",V2";
//                                vaSt += $",'lte-rrc.cqi_ReportPeriodic:{rpcVal}'";
//                            }
//                            while(reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_PUCCH_ResourceIndex")))
//                            {
//                                reader.Read();
//                            }
//                            reader.Read(); //value of lte-rrc.cqi_PUCCH_ResourceIndex
//                            string puccVal = reader.GetString();
//                            if (!string.IsNullOrEmpty(puccVal))
//                            {
//                                qrSt += $",V3";
//                                vaSt += $",'lte-rrc.cqi_PUCCH_ResourceIndex:{puccVal}'";
//                            }
//                            qrSt += $")";
//                            vaSt += $")";                            
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.transmissionMode"))) //lte-rrc.transmissionMode
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            qrSt += $",Event";
//                            vaSt += $",'RRC Antenna Info'";
//                            reader.Read(); //value of lte-rrc.a1_Thresholdlte-rrc.transmissionMode
//                            var ddd = reader.GetString();
//                            if (!string.IsNullOrEmpty(ddd))
//                            {
//                                qrSt += $",v1)";
//                                vaSt += $",'lte-rrc.transmissionMode:{ddd}')";                                                            
//                            }
//                            else
//                            {
//                                qrSt += $")";
//                                vaSt += $")";
//                            }
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("nas_eps.nas_msg_emm_type"))) //nas_eps.nas_msg_emm_type
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema                            

//                            reader.Read(); //get value of nas_eps.nas_msg_emm_type
//                            var ddd = reader.GetString();
//                            if(ddd=="0x48")
//                            {
//                                qrSt += $",Event)";
//                                vaSt += $",'Tracking area update request')"; FullQuery += $"{qrSt} {vaSt};\n";
//                            }
//                            if (ddd == "0x49")
//                            {
//                                qrSt += $",Event)";
//                                vaSt += $",'Tracking area update accept')"; FullQuery += $"{qrSt} {vaSt};\n";
//                            }
//                            if (ddd == "0x4a")
//                            {
//                                qrSt += $",Event)";
//                                vaSt += $",'Tracking area update complete')";FullQuery += $"{qrSt} {vaSt};\n";
//                            }                            
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("Codec Bitmap for SysID 1"))) //Codec Bitmap for SysID 1 , Codec Bitmap for SysID 2
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            reader.Read();
//                            ReadOnlySpan<byte> jsonElement = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
//                            //رسیدن به شروع آبجکت 
//                            var dd = Encoding.UTF8.GetString(jsonElement); //JsonToken.StartObject for SysId1
//                            string v1Str = string.Empty;
//                            while (reader.TokenType != JsonTokenType.EndObject)
//                            {
//                                reader.Read();
//                                if (reader.TokenType != JsonTokenType.EndObject)
//                                {
//                                    var vvv = reader.GetString();
//                                    if (vvv.StartsWith("gsm_a"))
//                                    {
//                                        v1Str += vvv + ":";
//                                    }
//                                    else
//                                    {
//                                        v1Str += vvv + ",";
//                                    }
//                                }
//                            }
//                            reader.Read(); reader.Read(); reader.Read(); reader.Read(); reader.Read();
//                            string v2Str = string.Empty;
//                            if (reader.ValueTextEquals(sCodecL2Event))
//                            {
//                                reader.Read();
//                                ReadOnlySpan<byte> jsonElement2 = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
//                                //رسیدن به شروع آبجکت 
//                                var dd2 = Encoding.UTF8.GetString(jsonElement2); //JsonToken.StartObject for SysId2                                
//                                while (reader.TokenType != JsonTokenType.EndObject)
//                                {
//                                    reader.Read();
//                                    if (reader.TokenType != JsonTokenType.EndObject)
//                                    {
//                                        var vvv = reader.GetString();
//                                        if (vvv.StartsWith("gsm_a"))
//                                        {
//                                            v2Str += vvv + ":";
//                                        }
//                                        else
//                                        {
//                                            v2Str += vvv + ",";
//                                        }
//                                    }
//                                }
//                            }
//                            // Assume valid JSON, known schema
//                            if (!string.IsNullOrEmpty(v1Str))
//                            {
//                                qrSt += $",Event ,V1";
//                                vaSt += $",'WCDMA Supported Codec List','{v1Str}'";
//                            }
//                            if (!string.IsNullOrEmpty(v2Str))
//                            {
//                                qrSt += $",V2)";
//                                vaSt += $",'{v2Str}')";
//                            }
//                            else
//                            {
//                                qrSt += $")";
//                                vaSt += $")";
//                            }
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionRequest_element"))) //rrc.rrcConnectionRequest_element
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
                          
//                            qrSt += $",Event )";
//                            vaSt += $",'RRC Connection Request')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.rrcConnectionRequest_element"))) //lte-rrc.rrcConnectionRequest_element
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
                            
//                            qrSt += $",Event )";
//                            vaSt += $",'LTE RRC Connection Request')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionSetup_r3_element"))) //rrc.rrcConnectionSetup_r3_element
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
                                                
//                            qrSt += $",Event )";
//                            vaSt += $",'RCC Connection Setup')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }                       
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionSetupComplete_element"))) //rrc.rrcConnectionSetupComplete_element
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
                           
//                            qrSt += $",Event )";
//                            vaSt += $",'RRC Connection Setup Complete')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(msgmmTyp))
//                        {
//                            //(DTAP) (MM) CM Service Request
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            if (ddd == "0x24")
//                            {
//                                qrSt += $",Event )";
//                                vaSt += $",'(DTAP) (MM) CM Service Request')";

//                                FullQuery += $"{qrSt} {vaSt};\n";
//                            }
//                            break;
//                        }
//                        if (reader.ValueTextEquals(lteRrcCons))
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            qrSt += $",Event ";
//                            vaSt += $",'LTE RRC Connection Setup'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            while (
//                                    reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.maxRetxThreshold"))
//                                    ||
//                                    reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.maxHARQ_Tx"))
//                                    ||
//                                    reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_ACK_Index"))
//                                    ||
//                                    reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_RI_Index"))
//                                    ||
//                                    reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_CQI_Index"))
//                                    ||
//                                    reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.allowedMeasBandwidth"))
//                                    )
//                            {
//                                qrSt += $",Event ";
//                                vaSt += $",'LTE RRC Connection Setup'";
//                            }
//                            //var ddd = reader.GetString();

//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.dtap.msg_mm_type")))
//                        {
//                            //(DTAP) (RR) Immediate Assignment
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            if (ddd == "0x3f")
//                            {
//                                qrSt += $",Event ";
//                                vaSt += $",'(DTAP) (RR) Immediate Assignment'";
//                                //while (!reader.ValueTextEquals("gsm_a.rr.packet_channel_type") )
//                                //{
//                                //    reader.Read();                                    
//                                //    if(reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.EndObject)
//                                //    {
//                                //        reader.Read();
//                                //    }
//                                //}
//                                //var d = $"{{{reader.GetString()}:"; //gsm_a.rr.packet_channel_type key
//                                //reader.Read();
//                                //d += $"{reader.GetString()},"; //gsm_a.rr.packet_channel_type value
//                                //var V1alue = d;

//                                //while (!reader.ValueTextEquals("gsm_a.rr.timeslot"))
//                                //{
//                                //    reader.Read();
//                                //    if (reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.EndObject)
//                                //    {
//                                //        reader.Read();
//                                //    }
//                                //}                               
//                                //d += $"{reader.GetString()}:"; //gsm_a.rr.timeslot key
//                                //var V2alue = reader.GetString();
//                                //reader.Read();
//                                //d += $"{reader.GetString()}}}"; //gsm_a.rr.timeslot value
//                                //V2alue += $":{reader.GetString()}";

//                                //qrSt += ",v1,v2,v3";
//                                //vaSt += $",'{V1alue}','{V2alue}','{d}'";

//                                qrSt += ")";
//                                vaSt += ")";
//                                FullQuery += $"{qrSt} {vaSt};\n";
//                            }   
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.dtap.msg_cc_type")))
//                        {
//                            //DownlinkDirectTransfer(cs-domain)(DTAP) (CC) Disconnect,UplinkDirectTransfer(cs-domain)(DTAP) (CC) Alerting
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            switch (ddd)
//                            {
//                                case "0x25":
//                                    qrSt += $",Event )";
//                                    vaSt += $",'(CC) Disconnect')";
//                                    break;
//                                case "0x01":
//                                    qrSt += $",Event )";
//                                    vaSt += $",'(CC) Alerting')";
//                                    break;
//                                case "0x02":
//                                    qrSt += $",Event )";
//                                    vaSt += $",'(CC) Call Proceeding')";
//                                    break;
//                                case "0x05":
//                                    qrSt += $",Event )";
//                                    vaSt += $",'(CC) Setup')";
//                                    break;

//                                /*Call End*/
//                                case "0x2a":
//                                    qrSt += $",Event,V1 )";
//                                    reader.Read();
//                                    var getCause = reader.GetString();
//                                    vaSt += $",' (CC) Release Complete','{getCause}')";
//                                    break;
//                                /*Call End*/
//                                case "0x2d":
//                                    qrSt += $",Event )";
//                                    vaSt += $",'(CC) Release')";
//                                    break;                               
//                                /*Call End*/
//                                case "0x24":
//                                    qrSt += $",Event )";
//                                    vaSt += $",'(DTAP) (MM) CM Service Request')";
//                                    break;
//                            }

//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionReleaseComplete_element"))) //WCDMA RRC Release Complete
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime,Event)";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}','WCDMA RRC Release Complete')";
                            
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionRelease_r3_element"))) //rrc.releaseCause in rrc.rrcConnectionRelease_r3_element 
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            reader.Read();
//                            ReadOnlySpan<byte> jsonElement = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
//                            //رسیدن به شروع آبجکت 
//                            var dd = Encoding.UTF8.GetString(jsonElement); //JsonToken.StartObject for rrc.rrcConnectionRelease_r3_element 

//                            while (reader.TokenType != JsonTokenType.EndObject)
//                            {
//                                reader.Read();
//                                if (reader.ValueTextEquals("rrc.releaseCause"))
//                                {
//                                    reader.Read();
//                                    var vvv = reader.GetString();
//                                    qrSt += ",V1";
//                                    vaSt += $",'rrc.releaseCause:{vvv}'";
//                                    break;
//                                }
//                            } 
//                            qrSt += $")";
//                            vaSt += $")";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionRelease_tree"))) //rrc.rrcConnectionRelease_tree
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime,Event)";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}','WCDMA RRC Connection Release')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }
//                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.releaseCause"))) //lte-rrc.releaseCause
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,FileName,TokenNo,TokenTime";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{filename}',{TokenNo},'{Tokendt}'";
//                            // Assume valid JSON, known schema
//                            reader.Read();
//                            var ddd = reader.GetString();
//                            qrSt += $",Event,V1 )";
//                            vaSt += $",'LTE RRC Connection Release','lte-rrc.releaseCause:{ddd}')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                            break;
//                        }                              
//                        break;
//                }

//            }

//            //var ddd  =  JsonDocument.Parse(jsonString);
//            //var topic = ddd.RootElement.EnumerateArray();

//            //foreach (var item in topic)
//            //{
//            //  var source=  item.EnumerateObject().FirstOrDefault(it => it.Name.Contains("_source") && it.Value.ValueKind == JsonValueKind.Object).Value;
//            //  var layer = source.EnumerateObject().FirstOrDefault(it => it.Name.Contains("layers") && it.Value.ValueKind == JsonValueKind.Object).Value;
//            //    //   .Where(it => it.Value.ValueKind == JsonValueKind.Array && it.Name == "frame.time_epoch")
//            //    //.SelectMany(it => it.Value.EnumerateArray().Select(that => that.GetString()))
//            //}


//            // FullQuery = anotherMethod(TestId, FullQuery, jsonString);
//            // if (showQuery)
//            return FullQuery;
//        }

//        public static DateTime FromUnixTime(long unixTime)
//        {
//            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
//            return epoch.AddSeconds(unixTime);
//        }
//        private static int AdoCommand(string testResult)
//        {
//            int res = -250;
//            using (SqlConnection cnn = new SqlConnection("Server = 185.192.112.74, 1561; Database = TmpKap400; User Id = sa; Password = Pr0b2001@ct1VE"))
//            {
//                using var cmm = new SqlCommand();
//                try
//                {
//                    cmm.Connection = cnn;
//                    cmm.CommandType = CommandType.Text;
//                    cmm.CommandText = testResult;
//                    cmm.CommandTimeout = 600;
//                    cnn.Open();
//                    res = cmm.ExecuteNonQuery();
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"{ex.Message} \n ReadingPcapJson@ {DateTime.Now} ,sql={testResult}");
//                }
//                finally
//                {
//                    cnn.Close();
//                }
//            }
//            return res;
//        }


//        /*
//           private static string anotherMethod(string TestId, string FullQuery, string jsonString)
//        {
//            var items = System.Text.Json.JsonSerializer.Deserialize<List<Pcap>>(jsonString);
//            foreach (var item in items)
//            {
//                string qrSt = string.Empty; string vaSt = string.Empty;
//                var time = Convert.ToDouble(item._source.layers.frame?.GetProperty("frame.time_epoch").ToString());
//                var Tokendt = FromUnixTime((long)time);
//                var TokenNo = item._source.layers.frame?.GetProperty("frame.number");

//                if (item._source.layers.lte_rrc != null)
//                {
//                    var lte_rrc = item._source.layers.lte_rrc.ul_dcch_msg_element;
//                    var cnteventA1 = lte_rrc.ToString().Contains("lte-rrc.eventA1_element");
//                    if (cnteventA1)
//                    {
//                        var v1 = lte_rrc.Value.GetProperty("lte-rrc.eventA1_element.lte-rrc.a1_Threshold").ToString();
//                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                        qrSt += $",Event ";
//                        vaSt += $",'A1event'";
//                        if (v1 != null)
//                        {
//                            qrSt += ",V1";
//                            vaSt += $",'{v1}'";
//                        }
//                        FullQuery += $"{qrSt}) {vaSt});\n";
//                    }
//                    var cnteventA2 = lte_rrc.ToString().Contains("lte-rrc.eventA2_element");
//                    if (cnteventA2)
//                    {
//                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                        qrSt += $",Event  ";
//                        vaSt += $",'A2event'";

//                        var dd = lte_rrc.Value.ToString().Contains("lte-rrc.a2_Threshold");
//                        var v2 = lte_rrc.Value.GetProperty("lte-rrc.a2_Threshold").ToString();
//                        if (v2 != null)
//                        {
//                            qrSt += ",V2";
//                            vaSt += $",'{v2}'";
//                        }
//                        FullQuery += $"{qrSt}) {vaSt});\n";
//                    }
//                    var cnteventA3 = lte_rrc.ToString().Contains("lte-rrc.eventA3_element");
//                    if (cnteventA3)
//                    {
//                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                        qrSt += $",Event ";
//                        vaSt += $",'A3event'";
//                        var v3 = lte_rrc.Value.GetProperty("lte-rrc.eventA3_element.lte-rrc.a2_Threshold").ToString();
//                        if (v3 != null)
//                        {
//                            qrSt += ",V3";
//                            vaSt += $",'{v3}'";
//                        }
//                        FullQuery += $"{qrSt}) {vaSt});\n";
//                    }
//                    var cnteventA4 = lte_rrc.ToString().Contains("lte-rrc.eventA4_element");
//                    if (cnteventA4)
//                    {
//                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                        qrSt += $",Event) ";
//                        vaSt += $",'A4event')";
//                        FullQuery += $"{qrSt} {vaSt};\n";
//                    }
//                    var cnteventA5 = lte_rrc.ToString().Contains("lte-rrc.eventA5_element");
//                    if (cnteventA5)
//                    {
//                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                        qrSt += $",Event) ";
//                        vaSt += $",'A5event')";
//                        FullQuery += $"{qrSt} {vaSt};\n";

//                    }
//                    var cnteventA6 = lte_rrc.ToString().Contains("lte-rrc.eventA6_element");
//                    if (cnteventA6)
//                    {
//                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                        qrSt += $",Event ) ";
//                        vaSt += $",'A6event')";
//                        FullQuery += $" {qrSt} {vaSt};\n";
//                    }
//                    var cnteventB1 = lte_rrc.ToString().Contains("lte-rrc.eventB1_element");
//                    if (cnteventB1)
//                    {
//                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                        qrSt += $",Event) ";
//                        vaSt += $",'B1event')";
//                        FullQuery += $"{qrSt} {vaSt};\n";
//                    }
//                    var cnteventNB1 = lte_rrc.ToString().Contains("lte-rrc.eventB1-NR_element");
//                    if (cnteventNB1)
//                    {
//                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                        qrSt += $",Event)";
//                        vaSt += $",'B1NRevent')";
//                        FullQuery += $"{qrSt} {vaSt};\n";
//                    }
//                    var cnteventB2 = lte_rrc.ToString().Contains("lte-rrc.eventB2_element");
//                    if (cnteventB2)
//                    {
//                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                        qrSt += $",Event) ";
//                        vaSt += $",'B2event')";
//                        FullQuery += $"{qrSt} {vaSt};\n";
//                    }
//                    var cnteventNB2 = lte_rrc.ToString().Contains("lte-rrc.eventB2-NR_element");
//                    if (cnteventNB2)
//                    {
//                        qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                        vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                        qrSt += $",Event) ";
//                        vaSt += $",'B2Nrevent')";
//                        FullQuery += $"{qrSt} {vaSt};\n";
//                    }
//                }
//            }

//            return FullQuery;
//        }

//        private static string ExtractJsonFile(string filePath)
//        {
//            Console.WriteLine($"***********Read {Path.GetFileName(filePath)}*************");
//            var fileWExt = Path.GetFileNameWithoutExtension(filePath);
//            var TestId = fileWExt.Split("_")[1];
//            string FullQuery = string.Empty;
//            string json;
//            using (StreamReader r = new StreamReader(filePath))
//            {
//                json = r.ReadToEnd();
//                var rw = JArray.Parse(json);


//                var jsonData = rw.Children();
//                var jsonData2 = jsonData.Select(u => u != null && u.Type == JTokenType.Property);

//                List<JToken> tokens = jsonData.Children().Children().ToList();
//                foreach (var item in rw)
//                {
//                    bool showQuery = false;
//                    string qrSt = string.Empty; string vaSt = string.Empty;
//                    //var dd =  item.ToObject<Pcap>();                   
//                    var Frame = item.SelectTokens("_source.layers.frame").FirstOrDefault();
//                    var time = Convert.ToDouble(Frame["frame.time_epoch"].ToString());//Frame["frame.time"];
//                                                                                      //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)time);
//                    var Tokendt = FromUnixTime((long)time);
//                    var TokenNo = Frame["frame.number"];

//                    var ddddd = item.SelectTokens("_source.layers.frame").FirstOrDefault()["frame.protocols"].ToString();
//                    if (ddddd.Contains("lte_rrc"))
//                    {

//                        var lte_rrc = item.SelectToken("_source.layers.lte_rrc");

//                        var cnteventA1 = lte_rrc.ToString().Contains("lte-rrc.eventA1_element");
//                        if (cnteventA1)
//                        {
//                            var v1 = lte_rrc.SelectToken("lte-rrc.eventA1_element.lte-rrc.a1_Threshold");

//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event ";
//                            vaSt += $",'A1event'";
//                            if (v1 != null)
//                            {
//                                qrSt += ",V1";
//                                vaSt += $",'{v1.Value<string>()}'";
//                            }
//                            FullQuery += $"{qrSt}) {vaSt});\n";
//                            // Console.WriteLine(lte_rrc);
//                            // Console.WriteLine(cnteventA1);
//                        }
//                        var cnteventA2 = lte_rrc.ToString().Contains("lte-rrc.eventA2_element");
//                        if (cnteventA2)
//                        {
//                            //foreach (var sourcePair in lte_rrc)
//                            //{
//                            //    if (sourcePair.Value<string>() == "eventA2_element")
//                            //    {
//                            //        var arole = "l";
//                            //    }
//                            //}
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event  ";
//                            vaSt += $",'A2event'";
//                            var v2 = lte_rrc.SelectToken("lte-rrc.DL_DCCH_Message_element.lte-rrc.eventA2_element.lte-rrc.a2_Threshold");
//                            if (v2 != null)
//                            {
//                                qrSt += ",V2";
//                                vaSt += $",'{v2.Value<string>()}'";
//                            }
//                            FullQuery += $"{qrSt}) {vaSt});\n";
//                        }
//                        var cnteventA3 = lte_rrc.ToString().Contains("lte-rrc.eventA3_element");
//                        if (cnteventA3)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event ";
//                            vaSt += $",'A3event'";
//                            var v3 = lte_rrc.SelectToken("lte-rrc.eventA3_element.lte-rrc.a2_Threshold");
//                            if (v3 != null)
//                            {
//                                qrSt += ",V3";
//                                vaSt += $",'{v3.Value<string>()}'";
//                            }
//                            FullQuery += $"{qrSt}) {vaSt});\n";
//                        }
//                        var cnteventA4 = lte_rrc.ToString().Contains("lte-rrc.eventA4_element");
//                        if (cnteventA4)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event) ";
//                            vaSt += $",'A4event')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                        }
//                        var cnteventA5 = lte_rrc.ToString().Contains("lte-rrc.eventA5_element");
//                        if (cnteventA5)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event) ";
//                            vaSt += $",'A5event')";
//                            FullQuery += $"{qrSt} {vaSt};\n";

//                        }
//                        var cnteventA6 = lte_rrc.ToString().Contains("lte-rrc.eventA6_element");
//                        if (cnteventA6)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event ) ";
//                            vaSt += $",'A6event')";
//                            FullQuery += $" {qrSt} {vaSt};\n";
//                        }
//                        var cnteventB1 = lte_rrc.ToString().Contains("lte-rrc.eventB1_element");
//                        if (cnteventB1)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event) ";
//                            vaSt += $",'B1event')";
//                            FullQuery += $"{qrSt} {vaSt};\n";

//                        }
//                        var cnteventNB1 = lte_rrc.ToString().Contains("lte-rrc.eventB1-NR_element");
//                        if (cnteventNB1)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event)";
//                            vaSt += $",'B1NRevent')";
//                            FullQuery += $"{qrSt} {vaSt};\n";

//                        }
//                        var cnteventB2 = lte_rrc.ToString().Contains("lte-rrc.eventB2_element");
//                        if (cnteventB2)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event) ";
//                            vaSt += $",'B2event')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                        }
//                        var cnteventNB2 = lte_rrc.ToString().Contains("lte-rrc.eventB2-NR_element");
//                        if (cnteventNB2)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event) ";
//                            vaSt += $",'B2Nrevent')";
//                            FullQuery += $"{qrSt} {vaSt};\n";
//                        }
//                    }
//                    var rrcUL = item.SelectTokens("_source.layers.['rrc.UL_DCCH_Message_element']");
//                    if (rrcUL != null)
//                    {
//                        var cnteventA1 = rrcUL.Contains("rrc.rrcConnectionReleaseComplete_element");
//                        if (cnteventA1)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event ";
//                            vaSt += $",'RRC Release Complete'";
//                            //if (v1 != null)
//                            //{
//                            //    qrSt += ",V1";
//                            //    vaSt += $",'{v1.Value<string>()}'";
//                            //}
//                            FullQuery += $"{qrSt}) {vaSt});\n";
//                        }
//                        cnteventA1 = rrcUL.Contains("rrc.rrcConnectionRelease_tree");
//                        if (cnteventA1)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event ";
//                            vaSt += $",'RRC Normal Connection Release'";
//                            //if (v1 != null)
//                            //{
//                            //    qrSt += ",V1";
//                            //    vaSt += $",'{v1.Value<string>()}'";
//                            //}
//                            FullQuery += $"{qrSt}) {vaSt});\n";
//                        }
//                        cnteventA1 = rrcUL.Contains("gsm_a.dtap.msg_cc_type");
//                        if (cnteventA1)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event ";
//                            vaSt += $",'DownlinkDirectTransfer(cs-domain)(DTAP) (CC) Disconnect'";
//                            //if (v1 != null)
//                            //{
//                            //    qrSt += ",V1";
//                            //    vaSt += $",'{v1.Value<string>()}'";
//                            //}
//                            FullQuery += $"{qrSt}) {vaSt});\n";
//                        }
//                        cnteventA1 = rrcUL.Contains("rrc.rrcConnectionSetupComplete_element");
//                        if (cnteventA1)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event ";
//                            vaSt += $",'RRCConnectionSetupComplete(cs-domain)(ps-domain)'";
//                            //if (v1 != null)
//                            //{
//                            //    qrSt += ",V1";
//                            //    vaSt += $",'{v1.Value<string>()}'";
//                            //}
//                            FullQuery += $"{qrSt}) {vaSt});\n";
//                        }
//                        cnteventA1 = rrcUL.Contains("rrc.rrcConnectionSetup_r3_element");
//                        if (cnteventA1)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event ";
//                            vaSt += $",'RRCConnectionSetup'";
//                            //if (v1 != null)
//                            //{
//                            //    qrSt += ",V1";
//                            //    vaSt += $",'{v1.Value<string>()}'";
//                            //}
//                            FullQuery += $"{qrSt}) {vaSt});\n";
//                        }
//                        cnteventA1 = rrcUL.Contains("rrc.rrcConnectionRequest_element");
//                        if (cnteventA1)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event ";
//                            vaSt += $",'RRC Connection Request'";
//                            //if (v1 != null)
//                            //{
//                            //    qrSt += ",V1";
//                            //    vaSt += $",'{v1.Value<string>()}'";
//                            //}
//                            FullQuery += $"{qrSt}) {vaSt});\n";
//                        }
//                        cnteventA1 = rrcUL.Contains("Codec Bitmap for SysID 1");
//                        if (cnteventA1)
//                        {
//                            showQuery = true;
//                            qrSt = "insert into TestresultEvent (Id,TestId,RegisterDate,TokenTime,TokenNo";
//                            vaSt = $"values ('{Guid.NewGuid()}',{TestId},'{DateTime.Now}','{Tokendt}',{TokenNo}";
//                            qrSt += $",Event ";
//                            vaSt += $",'Supported Codec List'";
//                            //if (v1 != null)
//                            //{
//                            //    qrSt += ",V1";
//                            //    vaSt += $",'{v1.Value<string>()}'";
//                            //}
//                            FullQuery += $"{qrSt}) {vaSt});\n";
//                        }
//                    }

//                }
//            }
//            return FullQuery;
//        }

//         */
//    }
//}
//public static class DatetTimeExtention
//{
//    public static DateTime FromUnixTime222(this long unixTime)
//    {
//        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
//        return epoch.AddSeconds(unixTime);
//    }
//}
//public class Pcap
//{
//    public string _index { get; set; }
//    public string _type { get; set; }
//    public string _score { get; set; }
//    public Source _source { get; set; }
//}

//public class Source
//{
//    public Layer2 layers { get; set; }
//}
//public class Layer
//{
//    public JToken frame { get; set; }
//    public JToken ip { get; set; }
//    public JToken upd { get; set; }
//    public JToken gsmtap { get; set; }
//    public JToken lte_rrc { get; set; }

//    public JToken nas_eps { get; set; }
//    public JToken gsm_a_dtap { get; set; }

//    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
//}
//public class Layer2
//{
//    public JsonElement? frame { get; set; }
//    public JsonElement? ip { get; set; }
//    public JsonElement? upd { get; set; }
//    public JsonElement? gsmtap { get; set; }
//    public LTErrc? lte_rrc { get; set; }

//    public JsonElement? nas_eps { get; set; }
//    public JsonElement? gsm_a_dtap { get; set; }
//    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
//}

//public class LTErrc
//{
//    string lte_rrc { get; set; }
//    [System.Text.Json.Serialization.JsonPropertyName("lte-rrc.UL_DCCH_Message_element")]
//    public JsonElement? ul_dcch_msg_element { get; set; }
//}
//public static class JTokenExtention
//{
//    public static IEnumerable<T> SelectTokensWithRegex<T>(this JToken jsonReader, Regex regex)
//    {
//        Newtonsoft.Json.JsonSerializer serializer = new();
//        while (jsonReader.HasValues)
//        {
//            if (regex.IsMatch(jsonReader.Value<String>()))
//            {
//                yield return JsonConvert.DeserializeObject<T>(jsonReader.Path);
//            }
//        }
//    }

//}

