using System;
using XInput.Wrapper;
using System.IO.Ports;
using System.Threading;

namespace JoystickController
{
    class Program
    {
        
        static void Main(string[] args)
        {
            bool running = true;

            SerialPort port = new SerialPort("COM10", 19200);

            X.UpdatesPerSecond = 60;
            Console.CancelKeyPress += delegate {
                running = false;
            };

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
                Thread.Sleep(33);

                if(!X.IsAvailable)
                {
                    continue;
                }

                int i = 0;
                foreach (X.Gamepad pad in pads)
                {
                    if (pad.Update() && pad.IsConnected)
                    {
                        Console.SetCursorPosition(0, i++);
                        //Console.Write("(" + pad.LStick.X.ToString("D06") + ", " + pad.LStick.Y.ToString("D06") + ")");
                        int steer = pad.LStick.X;
                        int speed = pad.LStick.Y;

                        if(steer < deadzone && steer > -deadzone)
                        {
                            steer = 0;
                        }
                        else
                        {
                            steer -= Math.Sign(steer) * deadzone;
                            steer /= divider;
                        }

                        if(speed < deadzone && speed > -deadzone)
                        {
                            speed = 0;
                        }
                        else
                        {
                            speed -= Math.Sign(speed) * deadzone;
                            speed /= divider;
                        }

                        Console.Write("(" + steer.ToString("D04") + ", " + speed.ToString("D04") + ")");

                        if(!port.IsOpen)
                        {
                            port.Open();
                        }

                        if(port.IsOpen && (old_speed != speed || old_steer != steer))
                        {
                            old_speed = speed;
                            old_steer = steer;

                            short steer16 = (short)steer;
                            steer16 *= 10;
                            short speed16 = (short)speed;
                            speed16 *= 10;

                            byte[] steerBytes = BitConverter.GetBytes(steer16);
                            byte[] speedBytes = BitConverter.GetBytes(speed16);

                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(steerBytes);
                                Array.Reverse(speedBytes);
                            }

                            port.Write(steerBytes, 0, steerBytes.Length);
                            port.Write(speedBytes, 0, speedBytes.Length);

                            Console.Write("W");
                        }
                    }
                }
            }
        }
    }
}
