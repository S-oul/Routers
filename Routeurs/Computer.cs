using System;

namespace Routeurs
{
    public class Computer
    {
        IpAddress ipAddress;
        Switch connectedSwitch;

        public void SetIpAddress(IpAddress ipAddress)
        {
            this.ipAddress = ipAddress;
        }

        public IpAddress GetIpAddress()
        {
            return this.ipAddress;
        }

        public void SetConnectedSwitch(Switch connectedSwitch)
        {
            this.connectedSwitch = connectedSwitch;
        }

        public Switch GetConnectedSwitch()
        {
            return this.connectedSwitch;
        }

        public void SendMessage(Computer destination, string message)
        {
            this.connectedSwitch.SendMessage(destination.GetIpAddress(), message);
        }

        public void ReceiveMessage(string message)
        {
            Console.WriteLine("({0}) - I received the message: {1}", this.ipAddress.GetStringAddress(), message);
        }
        public bool IsConnectedTo(Switch sw)
        {
            return sw.IpAddress.GetAdressRange() == this.ipAddress.GetAdressRange();
        }
    }
}
