using System;
using XInput.Wrapper;
using System.Threading;
using System.Net.Sockets;

namespace JoystickController
{
    class Program
    {
        static bool running;

        static void Controller()
        {
            const int PORT_NO = 23;
            const string SERVER_IP = "ESP-15DD91";

            TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
            NetworkStream port = client.GetStream();
            client.NoDelay = true;

            X.UpdatesPerSecond = 60;

            X.Gamepad[] pads = {
                X.Gamepad_1,
                X.Gamepad_2,
                X.Gamepad_3,
                X.Gamepad_4
            };

            const int deadzone = 6000;
            const int max = 32768;
            const int max_corrected = max - deadzone;
            const int divider = max_corrected / 100 + 1;

            int old_speed = 0;
            int old_steer = 0;

            while (running)
            {
                Thread.Sleep(50);

                if (!X.IsAvailable)
                {
                    continue;
                }

                int i = 0;
                foreach (X.Gamepad pad in pads)
                {
                    if (pad.Update() && pad.IsConnected)
                    {
                        //Console.SetCursorPosition(0, i++);
                        //Console.Write("(" + pad.LStick.X.ToString("D06") + ", " + pad.LStick.Y.ToString("D06") + ")");
                        int steer = pad.RStick.X;
                        int speed = pad.LStick.Y;

                        if (steer < deadzone && steer > -deadzone)
                        {
                            steer = 0;
                        }
                        else
                        {
                            steer -= Math.Sign(steer) * deadzone;
                            steer /= divider;
                        }

                        if (speed < deadzone && speed > -deadzone)
                        {
                            speed = 0;
                        }
                        else
                        {
                            speed -= Math.Sign(speed) * deadzone;
                            speed /= divider;
                        }

                        //Console.Write("(" + steer.ToString("D04") + ", " + speed.ToString("D04") + ")");

                        if (!client.Connected)
                        {
                            client.Connect(SERVER_IP, PORT_NO);
                            port = client.GetStream();
                            client.NoDelay = true;
                        }
                        else if (client.Connected)
                        {
                            if ((old_speed != speed || old_steer != steer))
                            {
                                old_speed = speed;
                                old_steer = steer;

                                short steer16 = (short)steer;
                                steer16 *= 10;
                                short speed16 = (short)speed;
                                speed16 *= 10;

                                byte[] steerBytes = BitConverter.GetBytes(steer16);
                                byte[] speedBytes = BitConverter.GetBytes(speed16);

                                /*if (BitConverter.IsLittleEndian)
                                {
                                    Array.Reverse(steerBytes);
                                    Array.Reverse(speedBytes);
                                }*/

                                byte[] total = new byte[5]
                                {
                                    (byte)';',
                                    steerBytes[0],
                                    steerBytes[1],
                                    speedBytes[0],
                                    speedBytes[1]
                                };

                                port.Write(total, 0, 5);

                                Console.WriteLine(BitConverter.ToString(total));
                            }

                            if (client.Available > 0)
                            {
                                byte[] bytes = new byte[client.Available];
                                port.Read(bytes, 0, bytes.Length);
                                Console.Write(System.Text.Encoding.ASCII.GetString(bytes));
                            }
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            running = true;

            Console.CancelKeyPress += delegate
            {
                running = false;
            };

            while (running)
            {
                try
                {
                    Controller();
                }
                catch (Exception e)
                {

                }
            }
        }
    }
}
