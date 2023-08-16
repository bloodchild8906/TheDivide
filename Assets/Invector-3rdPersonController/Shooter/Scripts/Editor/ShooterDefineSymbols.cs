using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.DefineSymbolsManager
{
    public class ShooterDefineSymbols : InvectorDefineSymbols
    {
        public override List<string> GetSymbols
        {
            get
            {
              return  new List<string>() { "INVECTOR_SHOOTER" };
            }
        }
      
    }
}