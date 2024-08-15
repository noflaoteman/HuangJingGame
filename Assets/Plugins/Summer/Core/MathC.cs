using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Summer.Core
{
    public class MathC
    {
        public static bool Equals(float a, float b)
        {
            return MathF.Abs(a - b) < 10e-6; //0.00001
        }
    }
}
