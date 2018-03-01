using System;
using System.Collections.Generic;

namespace VOICeVIO.HtsLib
{
    public enum HtsStreamType
    {
        /// <summary>
        ///	Mel CePstral
        /// </summary>
        MCP,
        /// <summary>
        ///	Mel Generalized Cepstral
        /// </summary>
        MGC,
        /// <summary>
        /// Log F0
        /// </summary>
        LF0,
        /// <summary>
        /// Low-Pass Filter
        /// </summary>
        LPF,
        /// <summary>
        /// Band-APeriodicity
        /// </summary>
        BAP,
        /// <summary>
        /// VIBrato
        /// </summary>		
        VIB,
        /// <summary>
        /// DURation
        /// </summary>		
        DUR,
        /// <summary>
        /// Unknown
        /// </summary>
        RC,
        /// <summary>
        /// Unknown
        /// </summary>
        RS,
        /// <summary>
        /// Unknown
        /// </summary>
        PDR,
    }

    /// <summary>
    /// HTS Stream
    /// </summary>
    public class HtsStream
    {
        public HtsStreamType Type { get; set; }
        /// <summary>
        /// Text format
        /// </summary>
        public List<byte[]> Windows { get; set; } = new List<byte[]>();
        public int NumWindows => Windows.Count;

        /// <summary>
        /// Binary format
        /// </summary>
        public Pdf Pdf { get; set; } = new Pdf();
        /// <summary>
        /// Text format
        /// </summary>
        public byte[] Tree { get; set; }
        public bool UseGv { get; set; } = false;
        public Pdf GvPdf { get; set; }
        public byte[] GvTree { get; set; }

        public Dictionary<string, float> Options { get; set; } = new Dictionary<string, float>();

        public string Option
        {
            get
            {
                List<string> opts = new List<string>();
                foreach (var option in Options)
                {
                    opts.Add($"{option.Key}={option.Value}");
                }
                return string.Join(",", opts);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Options.Clear();
                    return;
                }

                foreach (var pair in value.Split(','))
                {
                    var kv = pair.Split('=');
                    Options.Add(kv[0], float.Parse(kv[1]));
                }
            }
        }
    }
}
