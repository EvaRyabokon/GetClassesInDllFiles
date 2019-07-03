using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TestTask
{
    class Program
    {
        public struct ClassInfo
        {
            public string Name;
            public List<string> PublicMethods;
            public List<string> ProtectedMethods;

            public ClassInfo(string name, List<string> publicMethods, List<string> protectedMethods)
            {
                Name = name;
                PublicMethods = publicMethods;
                ProtectedMethods = protectedMethods;
            }
        }
        public struct DllsInfo
        {
            public List<ClassInfo> Classes;
            public List<string> FailedDlls;

            public DllsInfo(List<ClassInfo> classes, List<string> failedDlls)
            {
                Classes = classes;
                FailedDlls = failedDlls;
            }
        }

        public static ClassInfo GetMethodsInType(Type type)
        {
            List<string> publicMethods = new List<string>();
            List<string> protectedMethods = new List<string>();

            foreach (MethodInfo m in type.GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (m.IsPublic)
                {
                    publicMethods.Add(m.Name);
                }

                if (m.IsFamily)
                {
                    protectedMethods.Add(m.Name);
                }
            }

            return new ClassInfo(type.Name, publicMethods, protectedMethods);
        }

        public static List<ClassInfo> GetClassesInFile(string path)
        {
            try
            {
                List<ClassInfo> classes = new List<ClassInfo>();

                Assembly a = Assembly.LoadFrom(path);
                Type[] types = a.GetTypes();
                foreach (Type type in types)
                {
                    classes.Add(GetMethodsInType(type));
                }
                return classes;
            }

            catch (Exception)
            {
                return null;
            }
        }

        public static DllsInfo GetMethodsFromDllFiles(string path)
        {
            string[] filePaths = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);

            List<ClassInfo> classesInfo = new List<ClassInfo>();
            List<string> failedDlls = new List<string>();

            foreach (var file in filePaths)
            {
                List<ClassInfo> classes = GetClassesInFile(file);

                if (classes == null)
                {
                    failedDlls.Add(file);
                }
                else
                {
                    foreach (var c in classes)
                    {
                        classesInfo.Add(c);
                    }
                }
            }

            return new DllsInfo(classesInfo, failedDlls);
        }

        public static void PrintResult(DllsInfo dllsInfo)
        {
            foreach (var c in dllsInfo.Classes)
            {
                Console.WriteLine("CLASS: " + c.Name);
                foreach (var protectedMethod in c.ProtectedMethods)
                {
                    Console.WriteLine("PROTECTED: " + protectedMethod);
                }

                foreach (var publicMethod in c.PublicMethods)
                {
                    Console.WriteLine("PUBLIC: " + publicMethod);
                }
            }

            if (dllsInfo.FailedDlls.Any())
            {
                Console.WriteLine("Failed dlls:");
                foreach (var dll in dllsInfo.FailedDlls)
                {
                    Console.WriteLine(dll);
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Write the file directory:");
            string path = Console.ReadLine();
            DllsInfo files = GetMethodsFromDllFiles(path);
            PrintResult(files);
        }
    }
}
