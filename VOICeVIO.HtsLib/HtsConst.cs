using System;
using System.Collections.Generic;
using System.Text;

namespace VOICeVIO.HtsLib
{
    public enum CompressMethod
    {
        None
    }

    public static class HtsConst
    {
        //GLOBAL
        public const string LABEL_GLOBAL = "[GLOBAL]";
        public const string HTS_VOICE_VERSION = "HTS_VOICE_VERSION";
        public const string SAMPLING_FREQUENCY = "SAMPLING_FREQUENCY";
        public const string FRAME_PERIOD = "FRAME_PERIOD";
        public const string NUM_STATES = "NUM_STATES";
        public const string NUM_STREAMS = "NUM_STREAMS";
        public const string STREAM_TYPE = "STREAM_TYPE";
        public const string FULLCONTEXT_FORMAT = "FULLCONTEXT_FORMAT";
        public const string FULLCONTEXT_VERSION = "FULLCONTEXT_VERSION";
        public const string GV_OFF_CONTEXT = "GV_OFF_CONTEXT";
        public const string COMMENT = "COMMENT";
        //STREAM
        public const string VECTOR_LENGTH = "VECTOR_LENGTH";
        public const string IS_MSD = "IS_MSD";
        public const string NUM_WINDOWS = "NUM_WINDOWS";
        public const string USE_GV = "USE_GV";
        public const string OPTION = "OPTION";

        //POSITION
        public const string DURATION_PDF = "DURATION_PDF";
        public const string DURATION_TREE = "DURATION_TREE";
        public const string STREAM_WIN = "STREAM_WIN";
        public const string STREAM_PDF = "STREAM_PDF";
        public const string STREAM_TREE = "STREAM_TREE";
        public const string GV_PDF = "GV_PDF";
        public const string GV_TREE = "GV_TREE";

        //DEFAUTL
        public const string DEFAULT_GV_OFF_CONTEXT = "\"*-sil+*\",\"*-pau+*\"";
        public const string DEFAULT_FULLCONTEXT_FORMAT_TALK = "HTS_TTS_JPN";
    }
}
