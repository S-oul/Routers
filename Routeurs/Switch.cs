using System;
using System.Collections.Generic;

namespace Routeurs
{

    public class Switch
    {

        IpAddress ipAddress;

        Dictionary<uint, Computer> connectedPCs = new Dictionary<uint, Computer>();

        Dictionary<Switch, int> connectedSwitch = new Dictionary<Switch, int>();

        //les uint correspondent a des ranges d'adresse IP.
        //Chaque routeur dessevira uniquement des machines situées dans la même
        //range que lui même, on n'a pas besoin de stocker une entrée par adresse
        //mais uniquement une entrée par switch de destination (entrée dont la valeur
        //correspondra au range de son ip). La Value enregistrée à chaque Key
        //sera le switch du voisinage (connectedSwitch) par lequel transiter
        //pour arriver à destination.
        Dictionary<uint, Switch> routeTable = new Dictionary<uint, Switch>();

        uint nextMachineIp;


        public Dictionary<Switch, int> ConnectedSwitch => connectedSwitch;
        public IpAddress IpAddress => ipAddress;

        public Switch(IpAddress ipAddress)
        {
            this.ipAddress = ipAddress;
            nextMachineIp = 100;
        }

        public void ConnectSwitch(Switch neighbor, int latency)
        {
            if (connectedSwitch.ContainsKey(neighbor))
            {
                connectedSwitch[neighbor] = latency;
                neighbor.connectedSwitch[this] = latency;
            }
            else
            {
                connectedSwitch.Add(neighbor, latency);
                neighbor.connectedSwitch.Add(this, latency);
            }
        }

        public void ConnectPC(Computer pc)
        {
            uint pcIp;
            do
            {
                pcIp = ipAddress.GetAddress() + nextMachineIp;
                ++nextMachineIp;
                if (nextMachineIp > 255)
                    nextMachineIp = 100;
            } while (connectedPCs.ContainsKey(pcIp));
            pc.SetConnectedSwitch(this);
            pc.SetIpAddress(new IpAddress(pcIp));
            connectedPCs.Add(pcIp, pc);
        }

        public void FillRouteTable(List<Switch> allSwitches)
        {
            routeTable.Clear();

            foreach(var sw in connectedSwitch)
            {
                routeTable.Add(sw.Key.ipAddress.GetAdressRange(), sw.Key);
            }
        }

        public void SendMessage(IpAddress destination, string message, int latency = 0)
        {
            Console.WriteLine("Transiting Through {0} (latency={1}ms).", ipAddress.GetStringAddress(), latency);
            uint range = destination.GetAdressRange();
            if(range == ipAddress.GetAdressRange())
            {
                uint address = destination.GetAddress();
                if (!connectedPCs.ContainsKey(address))
                {
                    throw new Exception(String.Format("({0}) No PC has this adress: {1}",
                        ipAddress.GetStringAddress(), destination.GetStringAddress()));
                }
                connectedPCs[address].ReceiveMessage(message);
            }
            else if(routeTable.ContainsKey(range))
            {
                Switch nextSwitch = routeTable[range];
                if (connectedSwitch.ContainsKey(nextSwitch))
                {
                    nextSwitch.SendMessage(destination, message, latency + connectedSwitch[nextSwitch]);
                }else
                {
                    throw new Exception(String.Format("({0}) Trying to send a message to a switch we're not connected to: {1}",
                            ipAddress.GetStringAddress(), nextSwitch.ipAddress.GetStringAddress()));
                }
            }
            else
            {
                throw new Exception(String.Format("({0}) No known route to reach destination: {1}",
                        ipAddress.GetStringAddress(), destination.GetStringAddress()));
            }
        }
    }
}
