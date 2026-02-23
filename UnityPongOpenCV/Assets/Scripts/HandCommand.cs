using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class HandCommand : MonoBehaviour
{
    public static string p1Command = "P1_DOWN";
    public static string p2Command = "P2_DOWN";

    TcpListener server;
    Thread serverThread;

    void Start()
    {
        serverThread = new Thread(StartServer);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void StartServer()
    {
        server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5050);
        server.Start();
        Debug.Log("Hand Control Server started on port 5050");

        TcpClient client = server.AcceptTcpClient();
        Debug.Log("Python client connected!");
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[256];
        string leftover = "";

        while (true)
        {
            int bytes = stream.Read(buffer, 0, buffer.Length);
            if (bytes == 0) break;
            
            string data = leftover + Encoding.ASCII.GetString(buffer, 0, bytes);
            string[] lines = data.Split('\n');
            
            // Process all complete lines
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string msg = lines[i].Trim();
                if (msg.StartsWith("P1"))
                {
                    p1Command = msg;
                    Debug.Log("P1: " + msg);
                }
                else if (msg.StartsWith("P2"))
                {
                    p2Command = msg;
                    Debug.Log("P2: " + msg);
                }
            }
            
            // Keep the last incomplete line for next read
            leftover = lines[lines.Length - 1];
        }
    }

    void OnApplicationQuit()
    {
        server?.Stop();
        serverThread?.Abort();
    }
}
