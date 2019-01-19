namespace game.IL
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.IO;
    using System;
    using System.Linq;
    using System.Xml;

    class GenLinkXml
    {
        /// <summary>
        /// 打包IOS时，防止IL2Cpp剥离需要用到的库
        /// </summary>
        [MenuItem("IL/生成link.xml",false,5)]
        public static void Gen()
        {
            List<String> DllTypeWhiteList = new List<String>
            {
                "Assembly-CSharp.dll",
                "Base.dll",
                "Hotfix.dll",
            };

            //收集需要输出xml的dll信息
            var assemblies = new List<Assembly>()
            {
                //搜索本程序集下的全部DLL
            };
            var AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);//Assembly-CSharp.dll 生成路径
            Debug.Log(string.Format("<color=yellow>Load Assembly Path:{0}</color>", AssemblyPath));
            int index = 0;
            foreach (var dll in Directory.GetFiles(AssemblyPath, "*.dll"))
            {
                string dllName = dll.Substring(dll.LastIndexOf('\\') + 1);
                Debug.Log("Include Dll："+dllName);
                if (!DllTypeWhiteList.Contains(dllName))
                    continue;
                Assembly assembly =  Assembly.LoadFile(dll);
                assemblies.Add(assembly);
                Debug.LogError("Exist in DllTypeWhiteList,Need Reflect DllType:" + assemblies[index].ManifestModule.Name);
                index++;
            }

            var HotfixDllPath = @"D:\UnityPro\Unity2018.3\ILR\Assets\StreamingAssets\Hotfix";//热更DLL路径
            Debug.Log(string.Format("<color=yellow>Load HotfixDll Path:{0}</color>",HotfixDllPath));
            index = 0;
            foreach (var dll in Directory.GetFiles(HotfixDllPath, "*.dll"))
            {
                string dllName = dll.Substring(dll.LastIndexOf('\\') + 1);
                Debug.Log("Include Dll：" + dllName);
                if (!DllTypeWhiteList.Contains(dllName))
                    continue;
                Assembly assembly = Assembly.LoadFile(dll);
                assemblies.Add(assembly);

                Debug.LogError("Exist in DllTypeWhiteList,Need Reflect DllType:" + assemblies[index].ManifestModule.Name);
                index++;
            }

            assemblies = assemblies.Distinct().ToList();

            var xml = new XmlDocument();
            var rootElement = xml.CreateElement("linker");
            
            foreach(var ass in assemblies)
            {
                var assElement = xml.CreateElement("assembly");
                assElement.SetAttribute("fullname", ass.GetName().Name);
                var types = ass.GetTypes();
                foreach(var type in types)
                {
                    if (type.FullName.Equals("Win32"))
                        continue;
                    var typeElement = xml.CreateElement("type");
                    typeElement.SetAttribute("fullname", type.FullName);
                    typeElement.SetAttribute("preserve", "all");
                    assElement.AppendChild(typeElement);
                }
                rootElement.AppendChild(assElement);
            }
            xml.AppendChild(rootElement);

            var path = Application.dataPath + "/" + "link.xml";
            if (File.Exists(path))
                File.Delete(path);
            xml.Save(path);
        }
    }
}
