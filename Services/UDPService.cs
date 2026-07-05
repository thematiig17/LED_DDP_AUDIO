using LED_DDP_DRIVER.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace LED_DDP_DRIVER.Services
{
    public class UDPService
    {
        private readonly UdpClient _udpClient;
        private readonly string _destinationIp;
        private readonly int _destinationPort;
        public UDPService(string ipAddress, int port)
        {
            _udpClient = new UdpClient();
            _destinationIp = ipAddress;
            _destinationPort = port;
        }
        public void SendDdpPacket(byte r, byte g, byte b)
        {
            byte[] ddpPacket = new byte[13];

            ddpPacket[0] = 0x41;
            ddpPacket[1] = 0x00;
            ddpPacket[2] = 0x01;
            ddpPacket[3] = 0x01;
            ddpPacket[4] = 0x00;
            ddpPacket[5] = 0x00;
            ddpPacket[6] = 0x00;
            ddpPacket[7] = 0x00;
            ddpPacket[8] = 0x00;
            ddpPacket[9] = 0x03;

            ddpPacket[10] = r;
            ddpPacket[11] = g;
            ddpPacket[12] = b;

            try
            {
                _udpClient.Send(ddpPacket, ddpPacket.Length, _destinationIp, _destinationPort);
                Logger.Ddp("[UDP]: Sent: "+ BitConverter.ToString(ddpPacket, 10, 3));
            }
            catch
            {
                Logger.Ddp("[UDP]: Failed to send DDP packet.");
            }
        }
    }
}
