using System.Linq;

namespace VOICeVIO.HtsLib
{
    internal static class Helper
    {
        internal static int FindIndex(byte[] array, byte[] array2)
        {
            int i, j;

            for (i = 0; i < array.Length; i++)
            {
                if (i + array2.Length <= array.Length)
                {
                    for (j = 0; j < array2.Length; j++)
                    {
                        if (array[i + j] != array2[j]) break;
                    }

                    if (j == array2.Length) return i;
                }
                else
                    break;
            }

            return -1;
        }

        internal static byte[] GetWriteBytes(this byte[] bin, bool ec = false, bool em = false)
        {
            return bin;
        }
        internal static byte[] GetReadRange(this byte[] bin, string range, bool dc = false, bool dm = false)
        {
            var rs = range.Split('-');
            var start = int.Parse(rs[0]);
            var end = int.Parse(rs[1]);
            var rangeBin = bin.Skip(start).Take(end + 1 - start).ToArray();
            return rangeBin;
        }
    }
}
