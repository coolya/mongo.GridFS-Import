using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridFSImport
{
    class CmdLineParser
    {
        private List<Tuple<Func<string, bool>, Action<string>>> selectors 
            = new List<Tuple<Func<string,bool>,Action<string>>>();

        public void Regster(Func<string, bool> selector, Action<string> setter)
        {
            selectors.Add(Tuple.Create(selector, setter));
        }


    }
}
