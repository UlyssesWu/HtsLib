using System;
using System.Text;
using static VOICeVIO.HtsLib.HtsStreamType;
using static VOICeVIO.HtsLib.HtsConst;

namespace VOICeVIO.HtsLib.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("by Ulysses, wdwxy12345@gmail.com");

            HtsVoice voice = new HtsVoice("f001.htsvoice");

            var tree = Encoding.ASCII.GetString(voice.Streams[LPF].Tree);
            Console.WriteLine(tree);

            voice.Streams.Remove(LPF);
            voice.Save("f001_remix.htsvoice");

            Console.WriteLine("VOICeVIO (c) 2018");
            Console.ReadLine();
        }
    }
}
