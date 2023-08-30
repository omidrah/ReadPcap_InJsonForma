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

            Dictionary<string, string> dd = new Dictionary<string, string>(); ;
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
                        //reader.Read();
                        break;
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



                            //LTE RRC Connection Setup

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
                            #region LTE RRC Connection Setup
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.maxRetxThreshold")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "LTE RRC Connection Setup"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "LTE RRC Connection Setup");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "LTE RRC Connection Setup";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.maxHARQ_Tx")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "LTE RRC Connection Setup"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "LTE RRC Connection Setup");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "LTE RRC Connection Setup";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_ACK_Index")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "LTE RRC Connection Setup"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "LTE RRC Connection Setup");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "LTE RRC Connection Setup";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_RI_Index")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "LTE RRC Connection Setup"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "LTE RRC Connection Setup");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "LTE RRC Connection Setup";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_CQI_Index")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "LTE RRC Connection Setup"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "LTE RRC Connection Setup");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "LTE RRC Connection Setup";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.allowedMeasBandwidth")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "LTE RRC Connection Setup"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "LTE RRC Connection Setup");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                       // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "LTE RRC Connection Setup";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            #endregion
                            #region WCDMA RB Info
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rb_InformationReconfigList"))) //rrc.rb_InformationReconfigList  
                            {
                                var key = "WCDMA RB Info";//EventName
                                var parm = "\"" + reader.GetString() + "\"";
                                reader.Read();
                                parm += $":\"{reader.GetString()}\"";
                                dd.Add(key, parm);
                                break;
                            }
                            #endregion

                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.nas_Message")))
                            {
                                var key = "NAS Message";//EventName
                                var parm = "\"" + reader.GetString() + "\"";
                                reader.Read();
                                parm += $":\"{reader.GetString()}\"";
                                dd.Add(key, parm);
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("Timing Advance")))
                            //if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.timing_adv"))) //gsm_a.rr.timing_adv
                            {
                                bool ReadUnitl = false;
                                while (!ReadUnitl)
                                {
                                    if (reader.TokenType == JsonTokenType.Null ||
                                        reader.TokenType == JsonTokenType.StartObject ||
                                        reader.TokenType == JsonTokenType.EndObject ||
                                        reader.TokenType == JsonTokenType.EndArray ||
                                        reader.TokenType == JsonTokenType.StartArray)
                                    {
                                        reader.Read();
                                    }
                                    else
                                    {
                                        if (!reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.timing_adv")))
                                        {
                                            reader.Read();
                                        }
                                        else
                                        {
                                            var key1 = "Timing Advance";
                                            var parm = "\"" + reader.GetString()+ "\"";
                                            reader.Read();
                                            parm += $":\"{reader.GetString()}\"";
                                            dd.Add(key1, parm);
                                            ReadUnitl = true;
                                            break;
                                        }

                                    }
                                }
                                //var key = reader.GetString();
                                //reader.Read();
                                //dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.srb_ToAddModList"))) //lte-rrc.srb_ToAddModList
                            {
                                var key = "LTE RRC SRB";
                                var parm = "\"" + reader.GetString() + "\"";
                                reader.Read();
                                parm += $":\"{reader.GetString()}\"";
                                dd.Add(key, parm);
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("nas_eps.security_header_type"))) //nas_eps.security_header_type
                            {
                                var key = "Service request";
                                var parm = "\"" + reader.GetString() + "\"";
                                reader.Read();
                                var ddd = reader.GetString();
                                if (ddd == "12")
                                {
                                    parm += $":\"{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.cellUpdate_element"))) //rrc.cellUpdate_element
                            {
                                dd.Add("WCDMA RRC Cell Update", $"\"{reader.GetString()}\"");
                                break;
                            }
                            #region WCDMA RB Info
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rb_InformationReconfigList"))) //rrc.rb_InformationReconfigList
                            {
                                var key = "WCDMA RB Info";
                                var parm = "\"" + reader.GetString() + "\"";
                                reader.Read();
                                parm += $":\"{reader.GetString()}\"";
                                dd.Add(key, parm);
                                break;
                            }
                            #endregion 
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.cellUpdateConfirm_tree")))
                            {
                                dd.Add("WCDMA RRC Cell Update Confirm", $"\"{reader.GetString()}\"");
                                break;
                            }
                            #region rrc cqi report
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.cqi_ReportModeAperiodic")))
                            {
                                if (dd.Any(x => x.Key == "RRC CQI Report"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "RRC CQI Report");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "RRC CQI Report";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_ReportPeriodic")))
                            {
                                if (dd.Any(x => x.Key == "RRC CQI Report"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "RRC CQI Report");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "RRC CQI Report";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_PUCCH_ResourceIndex")))
                            {
                                if (dd.Any(x => x.Key == "RRC CQI Report"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "RRC CQI Report");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "RRC CQI Report";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_pmi_ConfigIndex")))
                            {
                                if (dd.Any(x => x.Key == "RRC CQI Report"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "RRC CQI Report");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "RRC CQI Report";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.ri_ConfigIndex")))
                            {
                                if (dd.Any(x => x.Key == "RRC CQI Report"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "RRC CQI Report");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "RRC CQI Report";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            #endregion
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("nas_eps.nas_msg_esm_type")))
                            {
                                reader.Read();
                                var ddd = reader.GetString();
                                if (ddd == "0xc1")
                                {
                                    var key = "ESM Activate default EPS bearer context request";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xc2")
                                {
                                    var key = "ESM Activate default EPS bearer context accept";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xc3")
                                {
                                    var key = "ESM Activate default EPS bearer context reject";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xc5")
                                {
                                    var key = "ESM Activate dedicated EPS bearer context request";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xc6")
                                {
                                    var key = "ESM Activate dedicated EPS bearer context accept";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xc7")
                                {
                                    var key = "ESM Activate dedicated EPS bearer context reject";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xc9")
                                {
                                    var key = "ESM Modify EPS bearer context request";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xca")
                                {
                                    var key = "ESM Modify EPS bearer context accept";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xcb")
                                {
                                    var key = "ESM Modify EPS bearer context reject";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xcd")
                                {
                                    var key = "ESM Deactivate EPS bearer context request";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xce")
                                {
                                    var key = "ESM Deactivate EPS bearer context accept";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xd0")
                                {
                                    var key = "ESM PDN connectivity request";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xd1")
                                {
                                    var key = "ESM PDN connectivity reject";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xd2")
                                {
                                    var key = "ESM PDN disconnect request";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xd3")
                                {
                                    var key = "PDN disconnect reject";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xd4")
                                {
                                    var key = "ESM Bearer resource allocation request";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xd5")
                                {
                                    var key = "ESM Bearer resource allocation reject";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xd6")
                                {
                                    var key = "ESM Bearer resource modification request";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xd7")
                                {
                                    var key = "ESM Bearer resource modification reject";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xd9")
                                {
                                    var key = "ESM information request";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0xda")
                                {
                                    var key = "ESM information response";
                                    var parm = $"\"nas_eps.nas_msg_esm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }

                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("nas_eps.nas_msg_emm_type"))) //nas_eps.nas_msg_emm_type
                            {
                                reader.Read();
                                var ddd = reader.GetString();
                                if (ddd == "0x41")
                                {
                                    var key = "EMM Attach request";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x42")
                                {
                                    var key = "EMM Attach accept";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x43")
                                {
                                    var key = "EMM Attach complete";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x44")
                                {
                                    var key = "EMM Attach reject";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x45")
                                {
                                    var key = "EMM Detach request";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x46")
                                {
                                    var key = "EMM Detach accept";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x48")
                                {
                                    var key = "EMM Tracking area update request";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }

                                if (ddd == "0x49")
                                {
                                    var key = "EMM Tracking area update accept";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\""; ;
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x4a")
                                {
                                    var key = "EMM Tracking area update complete";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x4b")
                                {
                                    var key = "EMM Tracking area update reject";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x4c")
                                {
                                    var key = "EMM Extended service request";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x4e")
                                {
                                    var key = "EMM Service reject";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x50")
                                {
                                    var key = "EMM GUTI reallocation command";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x51")
                                {
                                    var key = "EMM GUTI reallocation complete";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x52")
                                {
                                    var key = "EMM Authentication request";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x53")
                                {
                                    var key = "EMM Authentication response";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x54")
                                {
                                    var key = "EMM Authentication reject";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x55")
                                {
                                    var key = "EMM Identity request";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x56")
                                {
                                    var key = "EMM Identity response";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x5c")
                                {
                                    var key = "EMM Authentication failure";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x5d")
                                {
                                    var key = "EMM Security mode command";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x5e")
                                {
                                    var key = "EMM Security mode complete";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }
                                if (ddd == "0x5f")
                                {
                                    var key = "EMM Security mode reject";
                                    var parm = $"\"nas_eps.nas_msg_emm_type:{ddd}\"";
                                    dd.Add(key, parm);
                                }                               
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("Codec Bitmap for SysID 1"))) //Codec Bitmap for SysID 1
                            {
                                var pitem = "\"" + reader.GetString() + "\" :{";
                                var chekcInobject = false;
                                reader.Read();
                                ReadOnlySpan<byte> jsonElement = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                                while (reader.TokenType != JsonTokenType.EndObject)
                                {
                                    reader.Read();
                                    if (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        chekcInobject = true;
                                        var key = reader.GetString();
                                        reader.Read();
                                        var valuek = reader.GetString();
                                        pitem += $"\"{key}\":\"{valuek}\",";
                                    }
                                }
                                if (chekcInobject)
                                {
                                    pitem = pitem.Remove(pitem.Length - 1);
                                    pitem += $" }}";
                                    if (dd.Any(x => x.Key == "Supported Codec List"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "Supported Codec List");
                                        var newval = exsitKey.Value + "," + pitem;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, newval);
                                    }
                                    else
                                    {
                                        var key = "Supported Codec List";
                                        dd.Add(key, pitem);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("Codec Bitmap for SysID 2"))) // Codec Bitmap for SysID 2
                            {
                                var pitem = "\"" + reader.GetString() + "\" :{";
                                //var pitem = string.Empty;
                                var chekcInobject = false;
                                reader.Read();
                                ReadOnlySpan<byte> jsonElement = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                                while (reader.TokenType != JsonTokenType.EndObject)
                                {
                                    reader.Read();
                                    if (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        chekcInobject = true;
                                        var key = reader.GetString();
                                        reader.Read();
                                        var valuek = reader.GetString();
                                        pitem += $"\"{key}\":\"{valuek}\",";
                                    }
                                }
                                if (chekcInobject)
                                {
                                    pitem = pitem.Remove(pitem.Length - 1);
                                    pitem += $"}}";
                                    if (dd.Any(x => x.Key == "Supported Codec List"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "Supported Codec List");
                                        var newval = exsitKey.Value + "," + pitem;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, newval);
                                    }
                                    else
                                    {
                                        var key = "Supported Codec List";
                                        dd.Add(key, pitem);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionRequest_element"))) //rrc.rrcConnectionRequest_element
                            {
                                dd.Add("RRC Connection Request", $"\"{reader.GetString()}\"");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.rrcConnectionRequest_element"))) //lte-rrc.rrcConnectionRequest_element
                            {
                                dd.Add("LTE RRC Connection Request", $"\"{reader.GetString()}\"");
                                break; ;

                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionSetup_r3_element"))) //rrc.rrcConnectionSetup_r3_element
                            {
                                dd.Add("RCC Connection Setup", $"\"{reader.GetString()}\"");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionSetupComplete_element"))) //rrc.rrcConnectionSetupComplete_element
                            {
                                dd.Add("RRC Connection Setup Complete", $"\"{reader.GetString()}\"");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.rrcConnectionSetupComplete_element"))) //lte-rrc.rrcConnectionSetupComplete_element
                            {
                                dd.Add("LTE RRC Connection Setup Complete", $"\"{reader.GetString()}\"");
                                break;
                            }

                            #region (DTAP)(RR)Immediate Assignment
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.dtap.msg_rr_type")))
                            {
                                //var key = reader.GetString();
                                reader.Read();
                                var ddd = reader.GetString();
                                if (ddd == "0x3f")
                                {
                                    Set3f = true;
                                    //dd.Add(key, "(DTAP) (RR) Immediate Assignment");
                                    //while (reader.TokenType == JsonTokenType.Null || reader.TokenType==JsonTokenType.StartObject || reader.TokenType == JsonTokenType.EndObject || reader.TokenType ==JsonTokenType.EndArray || reader.TokenType ==JsonTokenType.StartArray)
                                    //{

                                    //    reader.Read();
                                    //    if (!reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.packet_channel_type"))){
                                    //        reader.Read();
                                    //    }
                                    //    else
                                    //    {
                                    //        var key1 = reader.GetString();
                                    //        reader.Read();
                                    //        dd.Add(key1, reader.GetString());
                                    //    }
                                    //}

                                    //while (reader.TokenType == JsonTokenType.Null || reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.EndObject || reader.TokenType == JsonTokenType.EndArray || reader.TokenType == JsonTokenType.StartArray)                                        
                                    //{
                                    //    reader.Read();
                                    //    if (!reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.timeslot")))
                                    //    {
                                    //        reader.Read();
                                    //    }
                                    //    else
                                    //    {
                                    //        var key1 = reader.GetString();
                                    //        reader.Read();
                                    //        dd.Add(key1, reader.GetString());
                                    //    }
                                    //}

                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.packet_channel_type")))
                            {
                                if (Set3f)
                                {
                                    if (dd.Any(x => x.Key == "(DTAP) (RR) Immediate Assignment"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "(DTAP) (RR) Immediate Assignment");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + );
                                    }
                                    else
                                    {
                                        var key = "(DTAP) (RR) Immediate Assignment";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.timeslot")))
                            {
                                if (Set3f)
                                {
                                    if (dd.Any(x => x.Key == "(DTAP) (RR) Immediate Assignment"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "(DTAP) (RR) Immediate Assignment");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + );
                                    }
                                    else
                                    {
                                        var key = "(DTAP) (RR) Immediate Assignment";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            #endregion
                            #region gsm_a.dtap→Measurement Results
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.rxlev_full_serv_cell")))
                            {
                                if (dd.Any(x => x.Key == "GSM Measurement Results"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "GSM Measurement Results");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                   // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "GSM Measurement Results";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.rxlev_sub_serv_cell")))
                            {
                                if (dd.Any(x => x.Key == "GSM Measurement Results"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "GSM Measurement Results");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "GSM Measurement Results";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.rxqual_full_serv_cell")))
                            {
                                if (dd.Any(x => x.Key == "GSM Measurement Results"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "GSM Measurement Results");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "GSM Measurement Results";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.rxqual_sub_serv_cell")))
                            {
                                if (dd.Any(x => x.Key == "GSM Measurement Results"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "GSM Measurement Results");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "GSM Measurement Results";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("Dtx_used")))
                            {
                                if (dd.Any(x => x.Key == "GSM Measurement Results"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "GSM Measurement Results");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "GSM Measurement Results";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            #endregion
                            #region RRC CQI SCC Report
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_ReportAperiodic_r10")))
                            {
                                if (dd.Any(x => x.Key == "RRC CQI SCC Report"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "RRC CQI SCC Report");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "RRC CQI SCC Report";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}";
                                    reader.Read();
                                    itemparm += $":{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_ReportPeriodic_r10")))
                            {
                                if (dd.Any(x => x.Key == "RRC CQI SCC Report"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "RRC CQI SCC Report");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "RRC CQI SCC Report";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}";
                                    reader.Read();
                                    itemparm += $":{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_PUCCH_ResourceIndex_r10")))
                            {
                                if (dd.Any(x => x.Key == "RRC CQI SCC Report"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "RRC CQI SCC Report");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "RRC CQI SCC Report";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}";
                                    reader.Read();
                                    itemparm += $":{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_pmi_ConfigIndex")))
                            {
                                if (dd.Any(x => x.Key == "RRC CQI SCC Report"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "RRC CQI SCC Report");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "RRC CQI SCC Report";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}";
                                    reader.Read();
                                    itemparm += $":{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            #endregion
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

                            #region Hystersis
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.hysteresis")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC Event hystersis"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC Event hystersis");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                       // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC Event hystersis";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.timeToTrigger")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC Event hystersis"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC Event hystersis");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC Event hystersis";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.triggerQuantity")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC Event hystersis"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC Event hystersis");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC Event hystersis";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.reportQuantity")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC Event hystersis"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC Event hystersis");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC Event hystersis";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.reportInterval")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC Event hystersis"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC Event hystersis");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC Event hystersis";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.reportAmount")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC Event hystersis"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC Event hystersis");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC Event hystersis";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.triggerType")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC Event hystersis"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC Event hystersis");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC Event hystersis";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            #endregion

                            #region Event1
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a1_Threshold")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {

                                    if (dd.Any(x => x.Key == "RRC event A1"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC event A1");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC event A1";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }

                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a1_Threshold_tree")))
                            //if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.threshold_RSRQ"))) //event1
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    var pkey = $"{parentKey}{reader.GetString()}";
                                    string val = string.Empty;
                                    while (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        reader.Read();
                                        if (reader.TokenType != JsonTokenType.EndObject && reader.TokenType != JsonTokenType.StartObject && reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.threshold_RSRQ")))
                                        {
                                            val += reader.GetString();
                                            break;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        if (dd.Any(x => x.Key == "RRC event A1"))
                                        {
                                            var exsitKey = dd.First(x => x.Key == "RRC event A1");
                                            var itemparm = $"\"{pkey}.{val}\"";
                                            //var parm = $",{parentKey}{reader.GetString()}";
                                            reader.Read();
                                            itemparm += $":\"{reader.GetString()}\"";
                                            var newval = exsitKey.Value + "," + itemparm;
                                            dd[exsitKey.Key] = newval;
                                            //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                        }
                                        else
                                        {
                                            var key = "RRC event A1";
                                            var itemparm = $"\"{pkey}.{val}\"";
                                            reader.Read();
                                            itemparm += $":\"{reader.GetString()}\"";
                                            dd.Add(key, itemparm);
                                        }
                                    }
                                }
                                break;
                            }
                            #endregion
                            #region Event2
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a2_Threshold"))) //event2
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC event A2"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC event A2");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC event A2";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a2_Threshold_tree")))
                            //if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.threshold_RSRQ"))) //event2
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    var pkey = $"{parentKey}{reader.GetString()}";
                                    string val = string.Empty;
                                    while (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        reader.Read();
                                        if (reader.TokenType != JsonTokenType.EndObject && reader.TokenType != JsonTokenType.StartObject && reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.threshold_RSRQ")))
                                        {
                                            val += reader.GetString();
                                            break;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        if (dd.Any(x => x.Key == "RRC event A2"))
                                        {
                                            var exsitKey = dd.First(x => x.Key == "RRC event A2");
                                            var itemparm = $"\"{pkey}.{val}\"";
                                            //var parm = $",{parentKey}{reader.GetString()}";
                                            reader.Read();
                                            itemparm += $":\"{reader.GetString()}\"";
                                            var newval = exsitKey.Value + "," + itemparm;
                                            dd[exsitKey.Key] = newval;
                                           // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                        }
                                        else
                                        {
                                            var key = "RRC event A2";
                                            var itemparm = $"\"{pkey}.{val}\"";
                                            reader.Read();
                                            itemparm += $":\"{reader.GetString()}\"";
                                            dd.Add(key, itemparm);
                                        }
                                    }
                                }
                                break;
                            }
                            #endregion
                            #region Event3
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a3_Offset")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC event A3"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC event A3");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC event A3";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.reportOnLeave"))) //event3
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC event A3"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC event A3");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC event A3";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            #endregion
                            #region Event5
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a5_Threshold1")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC event A5"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC event A5");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                       //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC event A5";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a5_Threshold2")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC event A5"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC event A5");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC event A5";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a5_Threshold1_tree")))
                            {
                                //reader.Read();
                                //ReadOnlySpan<byte> jsonElement = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                                //string v1Str = string.Empty;
                                //while (reader.TokenType != JsonTokenType.EndObject)
                                //{
                                //    reader.Read();
                                //    if (reader.TokenType != JsonTokenType.EndObject)
                                //    {
                                //        if (reader.GetString().Equals("lte-rrc.threshold_RSRQ"))
                                //        {
                                //            var key = "lte-rrc.a5_Threshold1_tree" + "." + reader.GetString();
                                //            reader.Read();
                                //            dd.Add(key, reader.GetString());
                                //            break;
                                //        }
                                //    }
                                //}
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    var pkey = $"{parentKey}{reader.GetString()}";
                                    string val = string.Empty;
                                    while (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        reader.Read();
                                        if (reader.TokenType != JsonTokenType.EndObject && reader.TokenType != JsonTokenType.StartObject && reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.threshold_RSRQ")))
                                        {
                                            val += reader.GetString();
                                            break;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        if (dd.Any(x => x.Key == "RRC event A5"))
                                        {
                                            var exsitKey = dd.First(x => x.Key == "RRC event A5");
                                            var itemparm = $"\"{pkey}.{val}\"";
                                            //var parm = $",{parentKey}{reader.GetString()}";
                                            reader.Read();
                                            itemparm += $":\"{reader.GetString()}\"";
                                            var newval = exsitKey.Value + "," + itemparm;
                                            dd[exsitKey.Key] = newval;
                                            //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                        }
                                        else
                                        {
                                            var key = "RRC event A5";
                                            var itemparm = $"\"{pkey}.{val}\"";
                                            reader.Read();
                                            itemparm += $":\"{reader.GetString()}\"";
                                            dd.Add(key, itemparm);
                                        }
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a5_Threshold2_tree")))
                            {
                                //reader.Read();
                                //ReadOnlySpan<byte> jsonElement = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                                //string v1Str = string.Empty;
                                //while (reader.TokenType != JsonTokenType.EndObject)
                                //{
                                //    reader.Read();
                                //    if (reader.TokenType != JsonTokenType.EndObject)
                                //    {
                                //        if (reader.GetString().Equals("lte-rrc.threshold_RSRQ"))
                                //        {
                                //            var key = "lte-rrc.a5_Threshold2_tree" + "." + reader.GetString();
                                //            reader.Read();
                                //            dd.Add(key, reader.GetString());
                                //            break;
                                //        }
                                //    }
                                //}
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    var pkey = $"{parentKey}{reader.GetString()}";
                                    string val = string.Empty;
                                    while (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        reader.Read();
                                        if (reader.TokenType != JsonTokenType.EndObject && reader.TokenType != JsonTokenType.StartObject && reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.threshold_RSRQ")))
                                        {
                                            val += reader.GetString();
                                            break;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        if (dd.Any(x => x.Key == "RRC event A5"))
                                        {
                                            var exsitKey = dd.First(x => x.Key == "RRC event A5");
                                            var itemparm = $"\"{pkey}.{val}\"";
                                            //var parm = $",{parentKey}{reader.GetString()}";
                                            reader.Read();
                                            itemparm += $":\"{reader.GetString()}\"";
                                            var newval = exsitKey.Value + "," + itemparm;
                                            dd[exsitKey.Key] = newval;
                                            //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                        }
                                        else
                                        {
                                            var key = "RRC event A5";
                                            var itemparm = $"\"{pkey}.{val}\"";
                                            reader.Read();
                                            itemparm += $":\"{reader.GetString()}\"";
                                            dd.Add(key, itemparm);
                                        }
                                    }
                                }
                                break;
                            }
                            #endregion
                            #region Event 6                            
                            //event 6 
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a6_Offset_r10")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC event A6"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC event A6");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC event A6";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a6_ReportOnLeave_r10")))
                            {
                                if (!string.IsNullOrEmpty(parentKey))
                                {
                                    if (dd.Any(x => x.Key == "RRC event A6"))
                                    {
                                        var exsitKey = dd.First(x => x.Key == "RRC event A6");
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        var newval = exsitKey.Value + "," + itemparm;
                                        dd[exsitKey.Key] = newval;
                                        //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                    }
                                    else
                                    {
                                        var key = "RRC event A6";
                                        var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                        reader.Read();
                                        itemparm += $":\"{reader.GetString()}\"";
                                        dd.Add(key, itemparm);
                                    }
                                }
                                break;
                            }
                            #endregion
                            #region Event B1,B2,B1Nr,B2Nr
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB1_element"))) //lte-rrc.eventB1_element     
                            {
                                dd.Add("RRC event B1", $"\"{reader.GetString()}\"");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB1-NR_element"))) //lte-rrc.eventB1-NR_element   
                            {
                                dd.Add("RRC event B1-NR", $"\"{reader.GetString()}\"");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB2_element"))) //lte-rrc.eventB2_element     
                            {
                                dd.Add("RRC event B2", $"\"{reader.GetString()}\"");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB2-NR_element"))) //lte-rrc.eventB2-NR_element   
                            {
                                dd.Add("RRC event B2-NR", $"\"{reader.GetString()}\"");
                                break;
                            }
                            #endregion
                            #region LTE RRC Scell 
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.antennaPortsCount")))
                            {
                                if (dd.Any(x => x.Key == "LTE RRC Scell"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "LTE RRC Scell");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                   // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "LTE RRC Scell";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.referenceSignalPower")))
                            {
                                if (dd.Any(x => x.Key == "LTE RRC Scell"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "LTE RRC Scell");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "LTE RRC Scell";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.transmissionMode_r10")))
                            {
                                if (dd.Any(x => x.Key == "LTE RRC Scell"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "LTE RRC Scell");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "LTE RRC Scell";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.ue_TransmitAntennaSelection")))
                            {
                                if (dd.Any(x => x.Key == "LTE RRC Scell"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "LTE RRC Scell");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    // exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "LTE RRC Scell";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            #endregion
                            #region  LTE RRC Antenna Info
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.transmissionMode")))
                            {
                                if (dd.Any(x => x.Key == "LTE RRC Antenna Info"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "LTE RRC Antenna Info");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "LTE RRC Antenna Info";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.codebookSubsetRestriction")))
                            {
                                if (dd.Any(x => x.Key == "LTE RRC Antenna Info"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "LTE RRC Antenna Info");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "LTE RRC Antenna Info";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }                            
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.n4TxAntenna_tm4")))
                            {
                                if (dd.Any(x => x.Key == "LTE RRC Antenna Info"))
                                {
                                    var exsitKey = dd.First(x => x.Key == "LTE RRC Antenna Info");
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    var newval = exsitKey.Value + "," + itemparm;
                                    dd[exsitKey.Key] = newval;
                                    //exsitKey.Value.Replace(exsitKey.Value, exsitKey.Value + itemparm);
                                }
                                else
                                {
                                    var key = "LTE RRC Antenna Info";
                                    var itemparm = $"\"{parentKey}{reader.GetString()}\"";
                                    reader.Read();
                                    itemparm += $":\"{reader.GetString()}\"";
                                    dd.Add(key, itemparm);
                                }
                                break;
                            }
                            #endregion

                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.dtap.msg_cc_type")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                var ddd = reader.GetString();
                                switch (ddd)
                                {
                                                                     
                                    case "0x01":
                                        dd.Add("CC Alerting", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x02":
                                        dd.Add("CC Call Proceeding", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x03":
                                        dd.Add("CC Progress", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x04":
                                        dd.Add("CC Establishment", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x05":
                                        dd.Add("CC Setup", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x06":
                                        dd.Add("CC Establishment Confirmed", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x07":
                                        dd.Add("CC Call Connect", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x08":
                                        dd.Add("CC Call Confirmed", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x09":
                                        dd.Add("CC Start", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x10":
                                        dd.Add("CC User Information", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x13":
                                        dd.Add("CC Modify Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x17":
                                        dd.Add("CC Modify", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x18":
                                        dd.Add("CC Hold", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x19":
                                        dd.Add("CC Hold Acknowledge", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x24":
                                        dd.Add("(DTAP) (MM) CM Service Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x25":
                                        dd.Add("CC Disconnect", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x39":
                                        dd.Add("CC Congestion Control", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x1f":
                                        dd.Add("CC Modify Complete", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x2a":
                                        dd.Add("CC Release Complete", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x2d":
                                        dd.Add("CC Release", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x3d":
                                        dd.Add("CC Notify", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x1e":
                                        dd.Add("CC Retrieve Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x1d":
                                        dd.Add("CC Retrieve Acknowledge", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x1c":
                                        dd.Add("CC Retrieve", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x1a":
                                        dd.Add("CC Hold Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x1b":
                                        dd.Add("CC Recall", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x0e":
                                        dd.Add("CC Emergency Setup", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x0f":
                                        dd.Add("CC Connect Acknowledge", $"\"{key}:{ddd}\"");
                                        break;                                   
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.dtap.msg_sm_type")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                var ddd = reader.GetString();
                                switch (ddd)
                                {

                                    case "0x5c":
                                        dd.Add("SM Request Secondary PDP Context Activation Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x5b":
                                        dd.Add("SM Request Secondary PDP Context Activation", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x5a":
                                        dd.Add("SM Request MBMS Context Activation Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x56":
                                        dd.Add("SM Activate MBMS Context Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x57":
                                        dd.Add("SM Activate MBMS Context Accept", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x58":
                                        dd.Add("SM Activate MBMS Context Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x59":
                                        dd.Add("SM Request MBMS Context Activation", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x4f":
                                        dd.Add("SM Activate Secondary PDP Context Reject", $"\"{key}:{ddd}\"");
                                        break;                                    
                                    case "0x4e":
                                        dd.Add("SM Activate Secondary PDP Context Accept", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x4d":
                                        dd.Add("SM Activate Secondary PDP Context Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x4c":
                                        dd.Add("SM Modify PDP Context Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x4b":
                                        dd.Add("SM Modify PDP Context Accept (Network to MS direction)", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x4a":
                                        dd.Add("SM Modify PDP Context Request(MS to network direction)", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x49":
                                        dd.Add("SM Modify PDP Context Accept (MS to network direction)", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x48":
                                        dd.Add("SM Modify PDP Context Request(Network to MS direction)", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x47":
                                        dd.Add("SM Deactivate PDP Context Accept", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x46":
                                        dd.Add("SM Deactivate PDP Context Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x45":
                                        dd.Add("SM Request PDP Context Activation rej.", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x44":
                                        dd.Add("SM Request PDP Context Activation", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x43":
                                        dd.Add("SM Activate PDP Context Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x42":
                                        dd.Add("SM Activate PDP Context Accept", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x41":
                                        dd.Add("SM Activate PDP Context Request", $"\"{key}:{ddd}\"");
                                        break;
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.dtap.msg_ss_type")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                var ddd = reader.GetString();
                                switch (ddd)
                                {
                                    case "0x2a":
                                        dd.Add("SS Release Complete", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x3b":
                                        dd.Add("SS Register", $"\"{key}:{ddd}\"");
                                        break;                                   
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.dtap.msg_mm_type")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                var ddd = reader.GetString();
                                switch (ddd)
                                {

                                    case "0x29":
                                        dd.Add("MM Abort", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x28":
                                        dd.Add("MM CM Re-establishment Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x25":
                                        dd.Add("MM CM Service Prompt", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x24":
                                        dd.Add("MM CM Service Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x23":
                                        dd.Add("MM CM Service Abort", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x22":
                                        dd.Add("MM CM Service Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x21":
                                        dd.Add("MM CM Service Accept", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x19":
                                        dd.Add("MM Identity Response", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x18":
                                        dd.Add("MM Identity Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x1c":
                                        dd.Add("MM Authentication Failure", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x14":
                                        dd.Add("MM Authentication Response", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x12":
                                        dd.Add("MM Authentication Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x11":
                                        dd.Add("MM Authentication Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x08":
                                        dd.Add("MM Location Updating Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x04":
                                        dd.Add("MM Location Updating Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x02":
                                        dd.Add("MM Location Updating Accept", $"\"{key}:{ddd}\"");
                                        break;
                                   
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.dtap.msg_gmm_type")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                var ddd = reader.GetString();
                                switch (ddd)
                                {

                                    case "0x1c":
                                        dd.Add("GMM Authentication and Ciphering Failure", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x14":
                                        dd.Add("GMM Authentication and Ciphering Rej", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x13":
                                        dd.Add("GMM Authentication and Ciphering Resp", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x12":
                                        dd.Add("GMM Authentication and Ciphering Req", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x0e":
                                        dd.Add("GMM Service Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x0d":
                                        dd.Add("GMM Service Accept", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x0c":
                                        dd.Add("GMM Service Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x0b":
                                        dd.Add("GMM Routing Area Update Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x0a":
                                        dd.Add("GMM Routing Area Update Complete", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x09":
                                        dd.Add("GMM Routing Area Update Accept", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x08":
                                        dd.Add("GMM Routing Area Update Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x06":
                                        dd.Add("GMM Detach Accept", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x05":
                                        dd.Add("GMM Detach Request", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x04":
                                        dd.Add("GMM Attach Reject", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x03":
                                        dd.Add("GMM Attach Complete", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x02":
                                        dd.Add("GMM Attach Accept", $"\"{key}:{ddd}\"");
                                        break;
                                    case "0x01":
                                        dd.Add("GMM Attach Request", $"\"{key}:{ddd}\"");
                                        break;
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionReleaseComplete_element")))
                            {
                                dd.Add("WCDMA RRC Release Complete", $"\"{reader.GetString()}\"");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.releaseCause")))
                            {
                                var key = "WCDMA RRC Connection Release";
                                var parm = "\"" + reader.GetString() + "\"";
                                reader.Read();
                                parm += $":\"{reader.GetString()}\"";
                                dd.Add(key, parm);
                                break;
                            }

                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.releaseCause")))
                            {
                                var key = "LTE RRC Connection Release";
                                var parm = "\"" + reader.GetString() + "\"";
                                reader.Read();
                                parm += $":\"{reader.GetString()}\n";
                                dd.Add(key, parm);
                                break;
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
        public static DateTime FromUnixTime(string  unixTime) 
        {
            double.TryParse(unixTime, out double res);
            DateTime dateTime2 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            //dateTime2 = dateTime2.AddSeconds(res);//incorrect 
            /*برای بدست آوردن میکروثانیه ها  عدد دریافتی در 1000 ضرب شده و از تابع موردنظر استفاده شده است .*/
            dateTime2 = dateTime2.AddMilliseconds(res * 1000).ToLocalTime();
            return  dateTime2;
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
