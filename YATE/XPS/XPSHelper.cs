using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Xps.Packaging;
using System.IO;

namespace DocEdLib.XPS
{
    public static class XPSHelper
    {
        public static string StipAttributes(this string srs, params string[] attributes)
        {
            return System.Text.RegularExpressions.Regex.Replace(srs,
                string.Format(@"{0}(?:\s*=\s*(""[^""]*""|[^\s>]*))?",
                string.Join("|", attributes)),
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        }

        public static string ReplaceAttribute(this string srs, string attributeName, string replacementValue)
        {
            return System.Text.RegularExpressions.Regex.Replace(srs,
                string.Format(@"{0}(?:\s*=\s*(""[^""]*""|[^\s>]*))?", attributeName),
                string.Format("{0}=\"{1}\"", attributeName, replacementValue),
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        }

        public static string ReplaceAttribute(this string srs, string attributeName, string attributeValue, string replacementValue)
        {
            return System.Text.RegularExpressions.Regex.Replace(srs,
                string.Format(@"{0}(?:\s*=\s*(""[^""]{1}""|[^\s>]*))?", attributeName, attributeValue),
                string.Format("{0}=\"{1}\"", attributeName, replacementValue),
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        }

        public static string GetFileName(this Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                string[] chunks = uri.OriginalString.Split('/');
                return chunks[chunks.Length - 1];
            }
            else
            {
                return uri.Segments[uri.Segments.Length - 1];
            }
        }

        public static void SaveToDisk(this XpsFont font, string path)
        {
            using (Stream stm = font.GetStream())
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    byte[] dta = new byte[stm.Length];
                    stm.Read(dta, 0, dta.Length);
                    if (font.IsObfuscated)
                    {
                        string guid = new Guid(font.Uri.GetFileName().Split('.')[0]).ToString("N");
                        byte[] guidBytes = new byte[16];
                        for (int i = 0; i < guidBytes.Length; i++)
                        {
                            guidBytes[i] = Convert.ToByte(guid.Substring(i * 2, 2), 16);
                        }

                        for (int i = 0; i < 32; i++)
                        {
                            int gi = guidBytes.Length - (i % guidBytes.Length) - 1;
                            dta[i] ^= guidBytes[gi];
                        }
                    }
                    fs.Write(dta, 0, dta.Length);
                }
            }
        }



    }
}
