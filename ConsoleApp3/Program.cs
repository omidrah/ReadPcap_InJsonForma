

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
            bool showQuery = false;
            string jsonString = File.ReadAllText(filePath);//.Replace("\n","").Replace("\r","");


            ReadOnlySpan<byte> jsonReadOnlySpan = File.ReadAllBytes(filePath);

            // Read past the UTF-8 BOM bytes if a BOM exists.
            if (jsonReadOnlySpan.StartsWith(Utf8Bom))
            {
                jsonReadOnlySpan = jsonReadOnlySpan.Slice(Utf8Bom.Length);
            }
            var reader = new Utf8JsonReader(jsonReadOnlySpan);
            int total = 0;
            int nextToken = 0;
            DateTime Tokendt = DateTime.Now;
            byte[] sCodecL2Event = Encoding.UTF8.GetBytes("Codec Bitmap for SysID 2"); 
            byte[] lteRrcCons = Encoding.UTF8.GetBytes("lte-rrc.rrcConnectionSetup_element");/*ناتمام*/

            Dictionary<string, string> dd = new Dictionary<string, string>(); ;
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
                        if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("_index")))
                        {
                                nextToken++;
                                Console.WriteLine(nextToken);
                                if (dd.Count > 1)
                                    setQuery(dd);
                                dd = new Dictionary<string, string>();                           
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
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a1_Threshold"))) //event1
                            {
                                var key = reader.GetString();
                                reader.Read(); //value of lte-rrc.a1_Threshold                           
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.threshold_RSRQ"))) //event1,2
                            {
                                var key = reader.GetString();
                                reader.Read(); //value of lte-rrc.threshold_RSRQ                           
                                dd.Add(key, reader.GetString());
                                break;
                            }

                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a2_Threshold"))) //event2
                            {
                                var key = reader.GetString();
                                reader.Read(); //value of lte-rrc.a2_Threshold                           
                                dd.Add(key, reader.GetString());
                                break;
                            }

                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a3_Offset"))) //event3
                            {
                                var key = reader.GetString();
                                reader.Read(); //value of lte-rrc.a3_Threshold                           
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.reportOnLeave"))) //event3
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventA4_element"))) //event4
                            {
                                var key = reader.GetString();
                                dd.Add(key, "RRC event A4");
                                break;
                            }
                            //event5
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a5_Threshold1")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a5_Threshold2")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a5_Threshold1_tree")))
                            {
                                reader.Read();
                                ReadOnlySpan<byte> jsonElement = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                                string v1Str = string.Empty;
                                while (reader.TokenType != JsonTokenType.EndObject)
                                {
                                    reader.Read();
                                    if (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        if (reader.GetString().Equals("lte-rrc.threshold_RSRQ"))
                                        {
                                            var key = "lte-rrc.a5_Threshold1_tree" + "." + reader.GetString();
                                            reader.Read();
                                            dd.Add(key, reader.GetString());
                                            break;
                                        }
                                    }
                                }

                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a5_Threshold2_tree")))
                            {
                                reader.Read();
                                ReadOnlySpan<byte> jsonElement = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                                string v1Str = string.Empty;
                                while (reader.TokenType != JsonTokenType.EndObject)
                                {
                                    reader.Read();
                                    if (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        if (reader.GetString().Equals("lte-rrc.threshold_RSRQ"))
                                        {
                                            var key = "lte-rrc.a5_Threshold2_tree" + "." + reader.GetString();
                                            reader.Read();
                                            dd.Add(key, reader.GetString());
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                            //event 6 
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a6_Offset_r10")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.a6_ReportOnLeave_r10")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            //LTE RRC Connection Setup
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.maxRetxThreshold")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.maxHARQ_Tx")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_ACK_Index")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_RI_Index")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_CQI_Index")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.allowedMeasBandwidth")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            //
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB1_element"))) //lte-rrc.eventB1_element     
                            {
                                dd.Add(reader.GetString(), "RRC event B1-NR");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB1-NR_element"))) //lte-rrc.eventB1-NR_element   
                            {
                                dd.Add(reader.GetString(), "RRC event B1");
                                break;

                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB2_element"))) //lte-rrc.eventB2_element     
                            {
                                dd.Add(reader.GetString(), "RRC event B2");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.eventB2-NR_element"))) //lte-rrc.eventB2-NR_element   
                            {
                                dd.Add(reader.GetString(), "'RRC event B2-NR");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rb_InformationReconfigList"))) //rrc.rb_InformationReconfigList  
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.nas_Message"))) //rrc.nas_Message
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.timing_adv"))) //gsm_a.rr.timing_adv
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.srb_ToAddModList"))) //lte-rrc.srb_ToAddModList
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("nas_eps.security_header_type"))) //nas_eps.security_header_type
                            {
                                var key = reader.GetString();
                                reader.Read();
                                var ddd = reader.GetString();
                                if (ddd == "12")
                                {
                                    dd.Add(key, "LTE RRC SRB");
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.cellUpdate_element"))) //rrc.cellUpdate_element
                            {
                                dd.Add(reader.GetString(), "WCDMA RRC Cell Update");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rb_InformationReconfigList"))) //rrc.rb_InformationReconfigList
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.cellUpdateConfirm_tree"))) //rrc.cellUpdateConfirm_tree
                            {
                                dd.Add(reader.GetString(), "WCDMA RRC Cell Update Confirm");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_ReportModeAperiodic"))) //lte-rrc.cqi_ReportModeAperiodic
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_PUCCH_ResourceIndex"))) //lte-rrc.cqi_PUCCH_ResourceIndex
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_ReportPeriodic"))) //lte-rrc.cqi_ReportPeriodic
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.transmissionMode"))) //lte-rrc.transmissionMode
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("nas_eps.nas_msg_emm_type"))) //nas_eps.nas_msg_emm_type
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("Codec Bitmap for SysID 1"))) //Codec Bitmap for SysID 1 , Codec Bitmap for SysID 2
                            {
                                reader.Read();
                                ReadOnlySpan<byte> jsonElement = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                                string v1Str = string.Empty;
                                while (reader.TokenType != JsonTokenType.EndObject)
                                {
                                    reader.Read();
                                    if (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        var key = reader.GetString();
                                        reader.Read();
                                        var valuek = reader.GetString();
                                        dd.Add(key, valuek);
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
                                    qrSt += $",Event ,V1";
                                    vaSt += $",'WCDMA Supported Codec List','{v1Str}'";
                                }
                                if (!string.IsNullOrEmpty(v2Str))
                                {
                                    qrSt += $",V2)";
                                    vaSt += $",'{v2Str}')";
                                }
                                else
                                {
                                    qrSt += $")";
                                    vaSt += $")";
                                }
                                FullQuery += $"{qrSt} {vaSt};\n";
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionRequest_element"))) //rrc.rrcConnectionRequest_element
                            {
                                dd.Add(reader.GetString(), "RRC Connection Request");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.rrcConnectionRequest_element"))) //lte-rrc.rrcConnectionRequest_element
                            {
                                dd.Add(reader.GetString(), "LTE RRC Connection Request");
                                break; ;

                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionSetup_r3_element"))) //rrc.rrcConnectionSetup_r3_element
                            {
                                dd.Add(reader.GetString(), "RCC Connection Setup");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionSetupComplete_element"))) //rrc.rrcConnectionSetupComplete_element
                            {
                                dd.Add(reader.GetString(), "RRC Connection Setup Complete");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.maxRetxThreshold")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.maxHARQ_Tx")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_ACK_Index")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_RI_Index")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.betaOffset_CQI_Index")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }

                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.dtap.msg_rr_type")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                var ddd = reader.GetString();
                                if (ddd == "0x3f")
                                {
                                    dd.Add(key, "(DTAP) (RR) Immediate Assignment");
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.packet_channel_type")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.timeslot")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            //gsm_a.dtap→Measurement Results
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.rxlev_full_serv_cell")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.rxlev_sub_serv_cell")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.rxqual_full_serv_cell")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.rr.rxqual_sub_serv_cell")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("Dtx_used")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            //RRC CQI SCC Report
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_ReportAperiodic_r10")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_ReportPeriodic_r10")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_PUCCH_ResourceIndex_r10")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_pmi_ConfigIndex")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            //RRC CQI Report
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.cqi_ReportModeAperiodic")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_PUCCH_ResourceIndex")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.cqi_pmi_ConfigIndex")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.ri_ConfigIndex")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            //RRC Event hystersis
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.hysteresis")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.timeToTrigger")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.triggerQuantity")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.reportQuantity")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.reportInterval")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.reportAmount")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.triggerType")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            //LTE RRC Scell 
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.antennaPortsCount")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.referenceSignalPower")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.transmissionMode_r10")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.ue_TransmitAntennaSelection")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.antennaPortsCount")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            //LTE RRC Antenna Info
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.transmissionmode")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.codebookSubsetRestriction")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.n2TxAntenna_tm4")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }

                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("gsm_a.dtap.msg_cc_type")))
                            {
                                var key = reader.GetString();
                                reader.Read();
                                var ddd = reader.GetString();
                                switch (ddd)
                                {
                                    case "0x25":
                                        dd.Add(key, "(CC) Disconnect");
                                        break;
                                    case "0x01":
                                        dd.Add(key, "(CC) Alerting");
                                        break;
                                    case "0x02":
                                        dd.Add(key, "(CC) Call Proceeding");
                                        break;
                                    case "0x05":
                                        dd.Add(key, "(CC) Setup");
                                        break;

                                    /*Call End*/
                                    case "0x2a":
                                        dd.Add(key, $"{ddd}");
                                        break;
                                    /*Call End*/
                                    case "0x2d":
                                        dd.Add(key, "(CC) Release");
                                        break;
                                    /*Call End*/
                                    case "0x24":
                                        dd.Add(key, "(DTAP) (MM) CM Service Request");
                                        break;
                                }
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionReleaseComplete_element"))) //WCDMA RRC Release Complete
                            {
                                dd.Add(reader.GetString(), "WCDMA RRC Release Complete");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.releaseCause"))) //rrc.releaseCause in rrc.rrcConnectionRelease_r3_element 
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("rrc.rrcConnectionRelease_tree"))) //rrc.rrcConnectionRelease_tree
                            {
                                dd.Add(reader.GetString(), "WCDMA RRC Connection Release");
                                break;
                            }
                            if (reader.ValueTextEquals(Encoding.UTF8.GetBytes("lte-rrc.releaseCause"))) //lte-rrc.releaseCause
                            {
                                var key = reader.GetString();
                                reader.Read();
                                dd.Add(key, reader.GetString());
                                break;
                            }
                        }                     
                       break;
                }
            }           
            return FullQuery;
        }

        private static void setQuery(Dictionary<string, string> dd)
        {
            foreach (var item in dd)
            {
                Console.WriteLine(item);
            }
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
