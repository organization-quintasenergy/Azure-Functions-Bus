using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public interface ISerializeMessages
    {
        object Deserialize(string input);

        object Deserialize(string input, Type type);

        string Serialize<T>(T input);
    }
}
