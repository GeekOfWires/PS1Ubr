using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmenityExtractor.Extensions
{
    public static class ListIntExtensions
    {
        public static string ToRangeString(this List<int> list, bool withSort = true)
        {
            list = list.Distinct().ToList();
            if (withSort) list.Sort();

            StringBuilder result = new StringBuilder();
            int temp;

            for (int i = 0; i < list.Count(); i++)
            {
                temp = list[i];

                //add a number
                result.Append(list[i]);

                //skip number(s) between a range
                while (i < list.Count() - 1 && list[i + 1] == list[i] + 1)
                    i++;

                //add the range
                if (temp != list[i])
                    result.Append("-").Append(list[i]);

                //add comma
                if (i != list.Count() - 1)
                    result.Append(", ");

            }
            return result.ToString();
        }
    }
}
