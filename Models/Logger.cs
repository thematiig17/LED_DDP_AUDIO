using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;

namespace LED_DDP_DRIVER.Models
{
    public enum LogType { Info, Warn, Error, DDP, UDP }
    public record LogMessage(string Message, LogType Type);
    public record DdpColorMessage(byte R, byte G, byte B);

    public static class Logger
    {
        public static void Info(string message)
        {
            SendMessage(message, LogType.Info);
        }
        public static void Warn(string message)
        {
            SendMessage(message, LogType.Warn);
        }

        public static void Error(string message)
        {
            SendMessage(message, LogType.Error);
        }


        public static void Ddp(string message)
        {
            SendMessage(message, LogType.DDP);
        }

        public static void Udp(string message)
        {
            SendMessage(message, LogType.UDP);
        }

        private static void SendMessage(string message, LogType type)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string formattedMessage = $"[{timestamp}] [{type}]: {message}";

            WeakReferenceMessenger.Default.Send(new LogMessage(formattedMessage, type));
        }
    }
}
