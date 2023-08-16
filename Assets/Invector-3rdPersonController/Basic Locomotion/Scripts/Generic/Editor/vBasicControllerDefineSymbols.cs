using System.Collections.Generic;
namespace Invector.DefineSymbolsManager
{
    public class vBasicControllerDefineSymbols : InvectorDefineSymbols
    {
        public override List<string> GetSymbols
        {
            get
            {
                return new List<string>() { "INVECTOR_BASIC" };
            }
        }
    }
}