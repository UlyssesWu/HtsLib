# VOICeVIO.HtsLib
A lib to read, modify and write htsvoices.

## Example

            HtsVoice voice = new HtsVoice("fx991.htsvoice");
            voice.Streams.Remove(LPF);
            voice.Save("fx82.htsvoice");

---

by Ulysses (wdwxy12345@gmail.com) from [VOICeVIO](https://github.com/VOICeVIO)