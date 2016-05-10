using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace forEach
{
    public class queueType<type>: List<type>
    {
        public int pointer;
               
        public queueType():base()
        {
            pointer = 0;
        }
        public void queue(type[] data)
        {
            base.AddRange(data);
        }
        public type dequeue()
        {
            if (pointer >= base.Count)
            {
                return default(type);
            }
            return base[pointer++];
        }

    }
}
