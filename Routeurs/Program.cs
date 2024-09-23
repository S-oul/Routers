using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Routeurs
{
    class Program
    {
        static List<Switch> switches = new List<Switch>();
        static List<Computer> computers = new List<Computer>();

        public static void Main(string[] args)
        {

            Console.WriteLine("Construction du réseau...");
            switches.Add(new Switch(new IpAddress("192.168.1.1")));
            switches.Add(new Switch(new IpAddress("192.168.4.1")));
            switches.Add(new Switch(new IpAddress("192.168.12.1")));
            switches.Add(new Switch(new IpAddress("192.168.11.1")));
            switches.Add(new Switch(new IpAddress("192.168.6.1")));
            switches.Add(new Switch(new IpAddress("240.27.71.1")));
            switches.Add(new Switch(new IpAddress("11.44.22.1")));
            switches.Add(new Switch(new IpAddress("1.1.1.1")));
            switches.Add(new Switch(new IpAddress("18.16.69.1")));
            switches.Add(new Switch(new IpAddress("194.200.73.1")));
            switches.Add(new Switch(new IpAddress("12.12.6.1")));
            //Maillage
            int[] maillage =
            {
                0, 1, 24,
                1, 2, 31,
                1, 5, 17,
                2, 5, 8,
                2, 8, 51,
                3, 5, 28,
                3, 6, 14,
                3, 7, 30,
                5, 6, 12,
                5, 10, 17,
                6, 7, 10,
                7, 8, 11,
                7, 10, 4,
                8, 9, 30,
                8, 10, 21,
            };

            int curIndex = 0;
            while (curIndex < maillage.Length)
            {
                switches[maillage[curIndex]].ConnectSwitch(switches[maillage[curIndex + 1]], maillage[curIndex + 2]);
                curIndex += 3;
            }
            //PC
            for (int i = 0; i < 50; ++i)
            {
                Computer computer = new Computer();
                computers.Add(computer);
                switches[i % switches.Count].ConnectPC(computer);
            }

            Console.WriteLine("Création des tables d'addressage...");
            foreach (Switch sw in switches)
            {
                //Fonction manquante....
                sw.FillRouteTable(switches);
            }

            Console.WriteLine("Envoi de messages...");
            Random random = new Random();
            for (int i = 0; i < 25; ++i)
            {
                try
                {
                    int source = random.Next(computers.Count);
                    int dest = random.Next(computers.Count);
                    Console.WriteLine("Message de {0} à {1}", computers[source].GetIpAddress().GetStringAddress(), computers[dest].GetIpAddress().GetStringAddress());
                    var path = ComputePath(computers[source], computers[dest]);
                    if(path == null)
                    {
                        Console.WriteLine("Chemin non trouvé");
                    }
                    else
                    {
                        Console.Write("Chemin: ");
                        foreach(Switch sw in switches)
                        {
                            Console.Write(sw.IpAddress.GetStringAddress() + " - ");
                        }
                        Console.WriteLine();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.ReadKey();
        }
        class WayInfo
        {
            public Switch _switch;
            public int cout;
            public WayInfo parent;
         
            public WayInfo(Switch newSwitch, int cout, WayInfo parent)
            {
                this._switch = newSwitch;
                this.cout = cout;
                this.parent = parent;
            }
            
        }
        static List<Switch> ComputePath(Computer start, Computer end)
        {
            if (end.GetConnectedSwitch().ConnectedSwitch.Count == 0 || start.GetConnectedSwitch().ConnectedSwitch.Count == 0)
            {
                Console.WriteLine("NORWAY");
                return null;
            }

            Switch sw = start.GetConnectedSwitch();
            Switch dest = end.GetConnectedSwitch();


            if (sw == dest)
            {
                Console.WriteLine("SAME");
                List<Switch> sortie = new List<Switch>
                {
                    sw
                };
                return sortie;
            }

            List<WayInfo> Ways = new List<WayInfo>
            {
                new WayInfo(sw, 0, null)
            };

            while (sw != dest)
            {
                foreach (Switch newSw in sw.ConnectedSwitch.Keys)
                {

                    //check si newsw est déjà dans la liste
                    int index = Ways.FindIndex(way => way._switch == newSw);
                    if(index != -1)
                    {
                        //si oui: on met à jour les données si la valeur est plus petite
                        if (Ways[index].cout < Ways.Aggregate((way, otherway) => way.cout < otherway.cout ? way : otherway).()
                        {
                            //et on passe à la suite: sw = plus petit cout parmi les non traités

                            sw = newSw;
                            Ways[index].cout += sw.ConnectedSwitch[newSw];
                            break;
                        }
                    }else
                    {
                        //s'il y est pas, on l'ajoute
                        WayInfo temp = Ways.Find(w => w._switch == sw);
                        Ways.Add(new WayInfo(newSw, temp.cout + sw.ConnectedSwitch[newSw], temp));
                    }
                }
            }
            WayInfo TheWay = Ways.FindAll(way => way._switch == dest).Aggregate((way, otherway) => way.cout < otherway.cout ? way: otherway);
            Console.WriteLine(TheWay._switch + " " + TheWay.cout);


            List<Switch> result = new List<Switch>();
            while (result[result.Count - 1] != dest)
            {
                result.Add(TheWay._switch);
                TheWay = TheWay.parent;
            }

            return result;
        }

    }
}
