
using System.Collections.Generic;

namespace Invector.DefineSymbolsManager
{
    public class MeleeDefineSymbols : InvectorDefineSymbols
    {
        public override List<string> GetSymbols
        {
            get
            {
               return new List<string>() { "INVECTOR_MELEE" };
            }
        }
    }
}
