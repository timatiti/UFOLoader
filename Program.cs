using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Management;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;
using MInject;

namespace UFO
{
    class Program
    {
        public static string GetHWID()
        {
            string str = "";
            {
                try
                {
                    string str2 = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 1);
                    ManagementObject obj1 = new ManagementObject("win32_logicaldisk.deviceid=\"" + str2 + ":\"");
                    obj1.Get();
                    str = obj1["VolumeSerialNumber"].ToString();
                }
                catch (Exception)
                {
                }
            }
            return str;
        }

        static string Crypt(string text)
        {
            byte[] hash = Encoding.ASCII.GetBytes(text);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hashenc = md5.ComputeHash(hash);
            string result = "";
            foreach (var b in hashenc)
            {
                result += b.ToString("x2");
            }
            return result;
        }

        static void Main(string[] args)
        {
            Console.Title = "UFO Loader";

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Загрузка данных..");
            string Namespace = "Cheats_Class";
            string Class = "Loader";
            string Method = "InitCheats";
            string Proc = "rustclient";

            Console.ForegroundColor = ConsoleColor.Yellow;
            string data = new WebClient().DownloadString("http://azp2033.000webhostapp.com/hwid.txt");
            if (data.Contains(Crypt(GetHWID())))
            {
                try
                {
                    Console.Beep(400, 100);
                    Console.WriteLine("Получение доступа к процессу..");

                    Process targetProcess = Process.GetProcessesByName(Proc)[0];
                    try
                    {
                        MonoProcess monoProcess;

                        if (MonoProcess.Attach(targetProcess, out monoProcess))
                        {
                            byte[] assemblyBytes = new WebClient().DownloadData("http://azp2033.000webhostapp.com/Facepunch.Mono.dll");
                            Console.WriteLine("Загрузка DLL с сервера..");

                            IntPtr monoDomain = monoProcess.GetRootDomain();
                            monoProcess.ThreadAttach(monoDomain);
                            monoProcess.SecuritySetMode(0);
                            monoProcess.DisableAssemblyLoadCallback();

                            IntPtr rawAssemblyImage = monoProcess.ImageOpenFromDataFull(assemblyBytes);
                            IntPtr assemblyPointer = monoProcess.AssemblyLoadFromFull(rawAssemblyImage);
                            IntPtr assemblyImage = monoProcess.AssemblyGetImage(assemblyPointer);
                            IntPtr classPointer = monoProcess.ClassFromName(assemblyImage, Namespace, Class);
                            IntPtr methodPointer = monoProcess.ClassGetMethodFromName(classPointer, Method);
                            monoProcess.RuntimeInvoke(methodPointer);

                            monoProcess.EnableAssemblyLoadCallback();

                            monoProcess.Dispose();
                        }
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ошибка! Возможно процесс не запущен...");
                        Console.ReadKey();
                    }

                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка! Возможно процесс не запущен...");
                    Console.ReadKey();
                }
            } else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка! У вас отсуствует лицензия..");
                Console.WriteLine("");
                Console.WriteLine("Меню: ");
                Console.WriteLine("1. Копировать HWID");
                Console.WriteLine("2. Перейти на сайт");
                Console.WriteLine("3. Группа ВК");
                Console.WriteLine("4. Выход");
                Console.Write("Меню: ");
                string line = Console.ReadLine();
                if(line == "1")
                {
                    Console.Clear();
                    Console.WriteLine("HWID: {0}", Crypt(GetHWID()));
                    Console.ReadKey();
                    Environment.Exit(0);
                } else
                {
                    if (line == "2")
                    {
                        Process.Start("http://ufohack.cf");
                        Console.Clear();
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    else
                    {
                        if (line == "3")
                        {
                            Process.Start("https://vk.com/ufohacks");
                            Console.Clear();
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        else
                        {
                            if (line == "4")
                            {
                                Environment.Exit(0);
                            }
                        }
                    }
                }
            }
        }
    }
}
