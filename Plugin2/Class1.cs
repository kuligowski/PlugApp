using System;
using PluginShared;

namespace Plugin2
{
    public class Class1 : IPlugin
    {
        public string Execute(string input)
        {   
            var charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
