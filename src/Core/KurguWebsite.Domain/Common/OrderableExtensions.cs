using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Common
{
    public static class OrderableExtensions
    {
        public static void Reorder<T>(this List<T> list) where T : IOrderable
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].SetDisplayOrder(i);
            }
        }
    }
}