using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static VOICeVIO.HtsLib.HtsConst;

namespace VOICeVIO.HtsLib
{
    /// <summary>
    /// HTS Model
    /// </summary>
    public class HtsVoice
    {
        public string FullContextFormat { get; set; } = "HTS_TTS";
        public string HtsVoiceVersion { get; set; } = "1.0";
        public string FullContextVersion { get; set; } = "1.0";
        public int NumStates { get; set; }
        public int NumStreams => Streams.Count;

        public bool UseGv => Streams.Values.Any(s => s.UseGv);
        private CompressMethod Compression { get; set; } = CompressMethod.None;
        private bool HeaderCompress { get; set; } = false;
        private bool BodyCompress { get; set; } = false;

        // var
        public int SamplingFrequency { get; set; } = 48000;
        public int FramePeriod { get; set; } = 240;
        public float Alpha { get; set; } = 0.55f;
        public string GvOffContext { get; set; }
        public string Comment { get; set; }

        /// <summary>
        /// Streams
        /// </summary>
        public Dictionary<HtsStreamType, HtsStream> Streams { get; set; } = new Dictionary<HtsStreamType, HtsStream>();
        /// <summary>
        /// Duration Stream
        /// </summary>
        public HtsStream Duration { get; set; } = new HtsStream() { Type = HtsStreamType.DUR };

        public List<string> AdditionalGlobal { get; set; } = new List<string>();
        public List<string> AdditionalStream { get; set; } = new List<string>();
        public List<string> AdditionalPosition { get; set; } = new List<string>();

        public HtsVoice(string path)
        {
            Load(path);
        }

        public HtsVoice()
        { }

        private bool IsRange(string val)
        {
            return val.Contains("-") && !val.Contains("=") && !val.Contains(",");
        }

        private string ToRange(long start, long end)
        {
            return $"{start}-{end}";
        }

        public void Load(string htsPath)
        {
            if (Directory.Exists(htsPath))
            {
                //TODO: Load from def files
                return;
            }
            //Data
            var bin = File.ReadAllBytes(htsPath);
            var globalBin = Encoding.ASCII.GetBytes(LABEL_GLOBAL);
            string text;

            if (bin.Take(globalBin.Length).SequenceEqual(globalBin))
            {
                HeaderCompress = false;
                text = File.ReadAllText(htsPath);
                var labelTag = Encoding.ASCII.GetBytes("[DATA]\n");
                var index = Helper.FindIndex(bin, labelTag);
                bin = bin.Skip(index + labelTag.Length).ToArray();
            }
            else
            {
                HeaderCompress = true;
                //if (bin.Take(globalBin.Length).SequenceEqual(Helper.GetHeader(globalBin)))
                //{
                //    Compression = (CompressMethod)2;
                //    ...
                //}
                //else
                //{
                //    Compression = (CompressMethod)1;
                //    ...
                //}
                throw new FormatException("Not a valid HTS Model");
            }

            var probe = BitConverter.ToInt32(bin.Take(4).ToArray(), 0);
            if (probe < 0 || probe > 65535)
            {
                BodyCompress = true;
            }
            else
            {
                BodyCompress = false;
            }
            //Label
            var globalMark = text.IndexOf("[STREAM]\n", StringComparison.InvariantCulture);
            var positionMark = text.IndexOf("[POSITION]\n", StringComparison.InvariantCulture);
            var dataMark = text.IndexOf("[DATA]\n", StringComparison.InvariantCulture);
            var globalText = text.Substring(0, globalMark);
            var streamText = text.Substring(globalMark, positionMark - globalMark);
            var positionText = text.Substring(positionMark, dataMark - positionMark);

            foreach (var line in globalText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    continue;
                }
                var p = line.Trim().Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var val = p.Length > 1 ? p[1] : "";

                switch (p[0])
                {
                    case HTS_VOICE_VERSION:
                        HtsVoiceVersion = val;
                        break;
                    case SAMPLING_FREQUENCY:
                        SamplingFrequency = int.Parse(val);
                        break;
                    case FRAME_PERIOD:
                        FramePeriod = int.Parse(val);
                        break;
                    case NUM_STATES:
                        NumStates = int.Parse(val);
                        break;
                    case NUM_STREAMS:
                        Streams = new Dictionary<HtsStreamType, HtsStream>(int.Parse(val));
                        break;
                    case STREAM_TYPE:
                        foreach (var type in val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var sType = (HtsStreamType)Enum.Parse(typeof(HtsStreamType), type, true);
                            Streams.Add(sType, new HtsStream() { Type = sType });
                        }
                        break;
                    case FULLCONTEXT_FORMAT:
                        FullContextFormat = val;
                        break;
                    case FULLCONTEXT_VERSION:
                        FullContextVersion = val;
                        break;
                    case GV_OFF_CONTEXT:
                        GvOffContext = val;
                        break;
                    case COMMENT:
                        Comment = val;
                        break;
                    default:
                        AdditionalGlobal.Add(line);
                        break;
                }
            }
            //STREAM
            foreach (var line in streamText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    continue;
                }
                var type = GetStreamType(line);
                if (type == null || (!Streams.ContainsKey(type.Value) && type != HtsStreamType.DUR))
                {
                    AdditionalStream.Add(line);
                    continue;
                }

                HtsStream stream;
                if (type == HtsStreamType.DUR)
                {
                    stream = Duration;
                }
                //else if (type == HtsStreamType.PDR)
                //{
                //    stream = PdrStream;
                //}
                else
                {
                    stream = Streams[type.Value];
                }
                var p = line.Trim().Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var val = p.Length > 1 ? p[1] : "";
                var label = p[0].Remove(p[0].IndexOf("[", StringComparison.Ordinal));
                switch (label)
                {
                    case VECTOR_LENGTH:
                        stream.Pdf.VectorLength = int.Parse(val);
                        break;
                    case IS_MSD:
                        stream.Pdf.IsMsd = int.Parse(val);
                        break;
                    case NUM_WINDOWS:
                        stream.Windows = new List<byte[]>(int.Parse(val));
                        break;
                    case USE_GV:
                        stream.UseGv = int.Parse(val) == 1;
                        break;
                    //case UAR:
                    //    break;
                    case OPTION:
                        stream.Option = val;
                        break;
                    default:
                        AdditionalStream.Add(line);
                        break;
                }

            }
            //POSITION
            foreach (var line in positionText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    continue;
                }
                var p = line.Trim().Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var val = p.Length > 1 ? p[1] : "";
                var type = GetStreamType(line);
                if (type != null && Streams.ContainsKey(type.Value))
                {
                    var stream = Streams[type.Value];
                    var label = p[0].Remove(p[0].IndexOf("[", StringComparison.Ordinal));
                    switch (label)
                    {
                        case STREAM_WIN:
                            foreach (var range in val.Split(','))
                            {
                                stream.Windows.Add(bin.GetReadRange(range));
                            }
                            break;
                        case STREAM_PDF:
                            stream.Pdf.Read(bin.GetReadRange(val), NumStates);
                            break;
                        case STREAM_TREE:
                            stream.Tree = bin.GetReadRange(val);
                            break;
                        case GV_PDF:
                            stream.GvPdf = new Pdf();
                            stream.GvPdf.Read(bin.GetReadRange(val), NumStates);
                            break;
                        case GV_TREE:
                            stream.GvTree = bin.GetReadRange(val);
                            break;
                        //case ARP:
                        //    break;
                        default:
                            if (IsRange(label))
                            {
                                Console.WriteLine("[WARN] Unknown Label: " + label);
                            }
                            AdditionalPosition.Add(line);
                            break;
                    }
                }
                else if (p[0] == DURATION_PDF)
                {
                    Duration.Pdf = new Pdf();
                    Duration.Pdf.Read(bin.GetReadRange(val), 1);
                }
                else if (p[0] == DURATION_TREE)
                {
                    Duration.Tree = bin.GetReadRange(val);
                }
                /*else if (p[0] == PP){}else if (p[0] == PT){}else if (p[0] == RCP){}else if (p[0] == RCT){}else if (p[0] == RSP){}else if (p[0] == RST){}*/
                else
                {
                    AdditionalPosition.Add(line);
                }
            }
        }

        private HtsStreamType? GetStreamType(string line)
        {
            foreach (var type in Enum.GetNames(typeof(HtsStreamType)))
            {
                if (line.Contains($"[{type}]"))
                {
                    return (HtsStreamType)Enum.Parse(typeof(HtsStreamType), type, true);
                }
            }

            return null;
        }

        public void Save(string path)
        {
            byte[] bin;
            Dictionary<string, string> ranges = new Dictionary<string, string>();
            //DATA
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms, Encoding.ASCII);
                //DUR
                var s = bw.BaseStream.Position;
                Duration.Pdf.WriteTo(bw);
                var e = bw.BaseStream.Position - 1;
                ranges[DURATION_PDF] = ToRange(s, e);
                s = e + 1;
                bw.Write(Duration.Tree.GetWriteBytes());
                e = bw.BaseStream.Position - 1;
                ranges[DURATION_TREE] = ToRange(s, e);

                #region deprecated SFE feature

                //if (PdrStream != null)
                //{
                //    s = e + 1;
                //    PdrStream.Pdf.WriteTo(bw);
                //    e = bw.BaseStream.Position - 1;
                //    ranges[PP] = ToRange(s, e);
                //    s = e + 1;
                //    bw.Write(PdrStream.Tree.GetWriteBytes());
                //    e = bw.BaseStream.Position - 1;
                //    ranges[PT] = ToRange(s, e);
                //}
                //if (RcStream != null)
                //{
                //    s = e + 1;
                //    RcStream.Pdf.WriteTo(bw);
                //    e = bw.BaseStream.Position - 1;
                //    ranges[RCP] = ToRange(s, e);
                //    s = e + 1;
                //    bw.Write(RcStream.Tree.GetWriteBytes());
                //    e = bw.BaseStream.Position - 1;
                //    ranges[RCT] = ToRange(s, e);
                //}
                //if (RsStream != null)
                //{
                //    s = e + 1;
                //    RsStream.Pdf.WriteTo(bw);
                //    e = bw.BaseStream.Position - 1;
                //    ranges[RSP] = ToRange(s, e);
                //    s = e + 1;
                //    bw.Write(RsStream.Tree.GetWriteBytes());
                //    e = bw.BaseStream.Position - 1;
                //    ranges[RST] = ToRange(s, e);
                //}

                #endregion


                //WIN
                foreach (var htsStream in Streams.Values)
                {
                    List<string> rangeList = new List<string>();
                    foreach (var window in htsStream.Windows)
                    {
                        s = e + 1;
                        bw.Write(window.GetWriteBytes());
                        e = bw.BaseStream.Position - 1;
                        rangeList.Add(ToRange(s, e));
                    }

                    ranges[$"{STREAM_WIN}[{htsStream.Type}]"] = string.Join(",", rangeList);
                }

                //PDF
                foreach (var htsStream in Streams.Values)
                {
                    s = e + 1;
                    htsStream.Pdf.WriteTo(bw);
                    e = bw.BaseStream.Position - 1;
                    ranges[$"{STREAM_PDF}[{htsStream.Type}]"] = ToRange(s, e);
                }

                //TREE
                foreach (var htsStream in Streams.Values)
                {
                    s = e + 1;
                    bw.Write(htsStream.Tree.GetWriteBytes());
                    e = bw.BaseStream.Position - 1;
                    ranges[$"{STREAM_TREE}[{htsStream.Type}]"] = ToRange(s, e);
                }

                //GV
                foreach (var htsStream in Streams.Values.Where(gv => gv.UseGv && gv.GvPdf != null))
                {
                    s = e + 1;
                    htsStream.GvPdf.WriteTo(bw);
                    e = bw.BaseStream.Position - 1;
                    ranges[$"{GV_PDF}[{htsStream.Type}]"] = ToRange(s, e);
                }
                foreach (var htsStream in Streams.Values.Where(gv => gv.UseGv && gv.GvTree != null))
                {
                    s = e + 1;
                    bw.Write(htsStream.GvTree.GetWriteBytes());
                    e = bw.BaseStream.Position - 1;
                    ranges[$"{GV_TREE}[{htsStream.Type}]"] = ToRange(s, e);
                }
                //foreach (var htsStream in Streams.Values.Where(gv => gv...))
                //{
                //    s = e + 1;
                //    bw.Write(htsStream.ARP.GetWriteBytes());
                //    e = bw.BaseStream.Position - 1;
                //    ranges[$"{ARP}[{htsStream.Type}]"] = ToRange(s, e);
                //}
                bw.Flush();
                bin = ms.ToArray();
            }

            var fs = File.Create(path);
            MemoryStream mss = new MemoryStream();
            StreamWriter sw = HeaderCompress
                ? new StreamWriter(mss, Encoding.ASCII, 2048, true)
                : new StreamWriter(fs, Encoding.ASCII, 2048, true);
            sw.NewLine = "\n";
            //GLOBAL
            sw.WriteLine("[GLOBAL]");
            sw.WriteLine($"{HTS_VOICE_VERSION}:{HtsVoiceVersion}");
            sw.WriteLine($"{SAMPLING_FREQUENCY}:{SamplingFrequency}");
            sw.WriteLine($"{FRAME_PERIOD}:{FramePeriod}");
            sw.WriteLine($"{NUM_STATES}:{NumStates}");
            sw.WriteLine($"{NUM_STREAMS}:{Streams.Count}");
            sw.WriteLine($"{STREAM_TYPE}:{string.Join(",", Streams.Keys.Select(t => t.ToString()))}");
            sw.WriteLine($"{FULLCONTEXT_FORMAT}:{FullContextFormat}");
            sw.WriteLine($"{FULLCONTEXT_VERSION}:{FullContextVersion}");
            if (!string.IsNullOrWhiteSpace(GvOffContext))
            {
                sw.WriteLine($"{GV_OFF_CONTEXT}:{GvOffContext}");
            }
            sw.WriteLine($"{COMMENT}:{Comment}");
            if (AdditionalGlobal.Count > 0)
            {
                AdditionalGlobal.ForEach(l => sw.WriteLine(l));
            }
            //STREAM
            sw.WriteLine("[STREAM]");
            foreach (var htsStream in Streams.Values)
            {
                sw.WriteLine($"{VECTOR_LENGTH}[{htsStream.Type}]:{htsStream.Pdf.VectorLength}");
            }
            foreach (var htsStream in Streams.Values)
            {
                sw.WriteLine($"{IS_MSD}[{htsStream.Type}]:{htsStream.Pdf.IsMsd}");
            }
            foreach (var htsStream in Streams.Values)
            {
                sw.WriteLine($"{NUM_WINDOWS}[{htsStream.Type}]:{htsStream.Windows.Count}");
            }
            //if (...)
            //{
            //    foreach (var htsStream in Streams.Values)
            //    {
            //        sw.WriteLine($"{UAR}[{htsStream.Type}]:{(htsStream.UAR ? 1 : 0)}");
            //    }
            //}

            if (UseGv)
            {
                foreach (var htsStream in Streams.Values)
                {
                    sw.WriteLine($"{USE_GV}[{htsStream.Type}]:{(htsStream.UseGv ? 1 : 0)}");
                }
            }
            foreach (var htsStream in Streams.Values)
            {
                sw.WriteLine($"{OPTION}[{htsStream.Type}]:{htsStream.Option}");
            }

            if (AdditionalStream.Count > 0)
            {
                AdditionalStream.ForEach(l => sw.WriteLine(l));
            }

            //POSITION
            sw.WriteLine("[POSITION]");
            foreach (var range in ranges)
            {
                sw.WriteLine($"{range.Key}:{range.Value}");
            }

            foreach (var add in AdditionalPosition)
            {
                sw.WriteLine(add);
            }

            sw.WriteLine("[DATA]");
            sw.Flush();
            sw.Dispose();

            BinaryWriter fbw = new BinaryWriter(fs);
            if (HeaderCompress)
            {
                //if (Compression == 1)
                //{
                //}
                //else if (Compression == 2)
                //{
                //}
                throw new NotImplementedException("This is an obsoleted SFE feature");
            }
            mss.Dispose();
            fbw.Write(bin);
            fbw.Flush();
            fbw.Close();

        }

    }
}
