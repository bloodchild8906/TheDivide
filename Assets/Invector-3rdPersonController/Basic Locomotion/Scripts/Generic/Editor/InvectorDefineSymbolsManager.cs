#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Invector.DefineSymbolsManager
{
    [InitializeOnLoad]
    public class InvectorDefineSymbolsManager
    {
        static InvectorDefineSymbolsManager()
        {
            CreateDefinitions();
        }

        public static void CreateDefinitions()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            List<string> allInvectorDefines = new List<string>();

            var denitionsType = GetAllDefinitions();
            foreach (var t in denitionsType)
            {
                var value = t.InvokeMember(null, BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null, null);

                List<string> list = null;
                try
                {
                    list = (List<string>)t.InvokeMember("GetSymbols", BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.GetProperty, null, value, null);
                    if (list != null)
                    {
                        allInvectorDefines.AddRange(list.Except(allInvectorDefines));
                    }
                }
                catch
                {

                }
            }

            var allDefiniesToRemove = allDefines.FindAll(s => s.ToUpper().Contains("INVECTOR") && !allInvectorDefines.Contains(s));
            var allDefiniesToAdd = allInvectorDefines.FindAll(s => !allDefines.Contains(s));
            var needUpdate = allDefiniesToRemove.Count > 0 || allDefiniesToAdd.Count > 0;
            if (needUpdate)
            {
                for (int i = 0; i < allDefiniesToRemove.Count; i++)
                    if (allDefines.Contains(allDefiniesToRemove[i])) allDefines.Remove(allDefiniesToRemove[i]);

                AddDefinitionSymbols(allDefiniesToAdd, allDefines);
            }
        }        

    static void AddDefinitionSymbols(List<string> targetDefineSymbols, List<string> currentDefineSymbols)
        {
            bool needUpdate = false;
            for (int i = 0; i < targetDefineSymbols.Count; i++)
            {
                if (!currentDefineSymbols.Contains(targetDefineSymbols[i]))
                {
                    needUpdate = true; break;
                }
            }
            currentDefineSymbols.AddRange(targetDefineSymbols.Except(currentDefineSymbols));
            if (needUpdate)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup,
                    string.Join(";", currentDefineSymbols.ToArray()));
        }

        static List<System.Type> GetAllDefinitions()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                 .Where(x => typeof(InvectorDefineSymbols).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();
        }
    }
}

#endif
namespace Invector.DefineSymbolsManager
{
    public abstract class InvectorDefineSymbols
    {
        public abstract List<string> GetSymbols
        {
            get;           
        }
    }
}