using System;
using System.CodeDom.Compiler;
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
            for (int i = 0; i < 2; ++i)
            {
                try
                {
                    int source = random.Next(computers.Count);
                    int dest = random.Next(computers.Count);
                    Console.WriteLine("\n \n \n");
                    Console.WriteLine("Message de {0} à {1}", computers[source].GetIpAddress().GetStringAddress(), computers[dest].GetIpAddress().GetStringAddress());

                    var path = ComputePath(computers[source], computers[dest]);
                    if (path == null)
                    {
                        Console.WriteLine("Chemin non trouvé");
                    }
                    else
                    {
                        Console.Write("Chemin: ");
                        foreach (Switch sw in path)
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
            public bool alreadyVerified = false;


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
                Console.WriteLine("Switch are not Connected");
                return null;
            }

            Switch startSwitch = start.GetConnectedSwitch();
            Switch endSwitch = end.GetConnectedSwitch();

            if (startSwitch == endSwitch)
            {
                Console.WriteLine("Start and End are the Same");
                return new List<Switch> { startSwitch };
            }

            List<WayInfo> Ways = new List<WayInfo>();
            Ways.Add(new WayInfo(startSwitch, 0, null));

            Dictionary<Switch, int> leastCost = new Dictionary<Switch, int>();
            leastCost[startSwitch] = 0;

            Dictionary<Switch, WayInfo> shortestPaths = new Dictionary<Switch, WayInfo>();

            while (Ways.Count > 0)
            {
                WayInfo current = Ways.OrderBy(way => way.cout).First();
                Ways.Remove(current);
                Console.WriteLine("\n");
                Console.WriteLine($"Exploring switch: {current._switch.IpAddress.GetStringAddress()} with cost: {current.cout}");

                if (current._switch == endSwitch)
                {
                    Console.WriteLine("Destination reached!");
                    List<Switch> path = new List<Switch>();
                    WayInfo temp = current;
                    while (temp != null)
                    {
                        path.Insert(0, temp._switch);
                        temp = temp.parent;
                    }
                    return path;
                }
                if (!leastCost.ContainsKey(current._switch) || current.cout <= leastCost[current._switch])
                {
                    leastCost[current._switch] = current.cout;
                    shortestPaths[current._switch] = current;
                    Console.WriteLine($"Updating cost of : {current._switch.IpAddress.GetStringAddress()}");

                    foreach (var newSw in current._switch.ConnectedSwitch)
                    {
                        
                        int newCost = current.cout + newSw.Value;

                        Console.WriteLine($"Checking neighborg: {newSw.Key.IpAddress.GetStringAddress()} : new cost: {newCost}");

                        if (!leastCost.ContainsKey(newSw.Key) || newCost <= leastCost[newSw.Key])
                        {
                            Console.WriteLine($"Adding {newSw.Key.IpAddress.GetStringAddress()} to the list with cost: {newCost}");
                            Ways.Add(new WayInfo(newSw.Key, newCost, current));
                            leastCost[newSw.Key] = newCost;
                        }
                    }
                }
            }

            Console.WriteLine("No path's found");
            return null;
        }

    }
}
