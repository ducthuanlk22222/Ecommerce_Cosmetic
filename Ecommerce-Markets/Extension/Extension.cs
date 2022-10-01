using System.Text.RegularExpressions;

namespace Ecommerce_Markets.Extension
{
    public static class Extension
    {
        public static string toVnd (this double donGia)
        {
            return donGia.ToString("#,##0");
        }
        public static string toTitleCase (string str)
        {
            string result = str;
            if (!string.IsNullOrEmpty(str))
            {
                var words = str.Split(' ');
                for (int index=0; index<words.Length; index++)
                {
                    var s = words[index];
                    if(s.Length > 0)
                    {
                        words[index] = s[0].ToString().ToUpper() + s.Substring(1);
                    }
                }
                result = string.Join(" ", words);   
            }
            return result;
        }
       
    }
}
