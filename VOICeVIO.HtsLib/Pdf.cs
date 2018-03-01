using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VOICeVIO.HtsLib
{
    public class Pdf
    {
        public int IsMsd { get; set; }
        public int NTree { get; set; }
        public int VectorLength { get; set; }
        public List<int> NPdf { get; set; } = new List<int>();
        public List<float> Data { get; set; } = new List<float>();

        public void WriteTo(BinaryWriter bw, bool ec = false, bool em = false)
        {
            if (ec || em)
            {
                throw new NotImplementedException("This is an obsoleted SFE feature");
            }
            else
            {
                foreach (var d in NPdf)
                {
                    bw.Write(d);
                }
                foreach (float f in Data)
                {
                    bw.Write(f);
                }
            }
        }

        public void Read(byte[] bytes, int numStates, bool de = false)
        {
            NTree = numStates;
            using (var ms = new MemoryStream(bytes))
            {
                BinaryReader br = new BinaryReader(ms);
                for (int i = 0; i < numStates; i++)
                {
                    NPdf.Add(br.ReadInt32());
                }

                while (br.BaseStream.Length - br.BaseStream.Position >= 4)
                {
                    if (de)
                    {
                        throw new NotImplementedException("This is an obsoleted SFE feature");
                    }
                    else
                    {
                        Data.Add(br.ReadSingle());
                    }
                }
            }
        }

        public void Save(string dirPath, string pdfName, string treeName, int windowsNum)
        {
            throw new NotImplementedException("TODO feature");
        }

        public static Pdf Load(string dirPath, string pdfName, string treeName, int windowsNum)
        {
            throw new NotImplementedException("TODO feature");
        }

    }
}
