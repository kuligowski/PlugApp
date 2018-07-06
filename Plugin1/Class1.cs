using System;
using PluginShared;

namespace Plugin1
{
    public class Class1 : IPlugin
    {
        public string Execute(string input)
        {
            return input.ToUpper();
        }
    }
}
