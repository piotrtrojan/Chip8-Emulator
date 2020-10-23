using System;
using System.IO;

namespace Chip8.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var cpu = new Cpu();
            var ok = 0;
            var notSupported = 0;
            var notImplemented = 0;
            using (var reader = new BinaryReader(new FileStream("IBM Logo.ch8", FileMode.Open)))
            {
                while(reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var opcode = (ushort)((reader.ReadByte() << 8) | reader.ReadByte());
                    // Console.WriteLine(opcode.ToString("X4"));
                    try
                    {
                        // cpu.ExecuteOpcode(opcode);
                        cpu.Step(opcode);
                        ok++;
                    }
                    catch(NotSupportedException ex)
                    {
                        Console.WriteLine(ex.Message);
                        notSupported++;
                    }
                    catch (NotImplementedException ex)
                    {
                        Console.WriteLine(ex.Message);
                        notImplemented++;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Exception: {ex.Message}");
                    }
                }
            }
            Console.WriteLine($"Ok: {ok}, Invalid: {notSupported}, NotImplemented: {notImplemented}");
            Console.ReadKey();
        }
    }
}
