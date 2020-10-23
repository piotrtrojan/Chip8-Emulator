using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Chip8
{
    public class Cpu
    {
        public byte[] RAM = new byte[4096];
        public byte[] V = new byte[16]; // Registers
        public ushort PC = 0; // Program Counter
        public ushort I = 0; // Memory address.
        public Stack<ushort> Stack = new Stack<ushort>(); // 24 should be max capacity.
        public byte DelayTimer;
        public byte SoundTimer;
        public byte Keyboard;

        public byte[] Display = new byte[64 * 32]; // Could be made easier, color is monochromatic;
        
        public bool WaitForKeyPress = false;

        private Random generator = new Random(Environment.TickCount);

        public void Step(ushort opcode)
        {
            if (WaitForKeyPress)
            {
                V[(opcode & 0x0F00) >> 8] = Keyboard;
                return;
            }

            ExecuteOpcode(opcode);
        }

        public void ExecuteOpcode(ushort opcode)
        {
            

            var nibble = (ushort)(opcode & 0b1111000000000000); // 0xf000

            switch (nibble)
            {
                case 0x0000:
                    Opcode1(opcode);
                    break;
                case 0x1000:
                    PC = (ushort)(opcode & 0x0fff);
                    break;
                case 0x2000:
                    Stack.Push(PC);
                    PC = (ushort)(opcode & 0x0fff);
                    break;
                case 0x3000:
                    if (V[opcode & 0x0f00 >> 8] == (opcode & 0x00ff))
                        PC += 2;
                    break;
                case 0x4000:
                    if (V[opcode & 0x0f00 >> 8] != (opcode & 0x00ff))
                        PC += 2;
                    break;
                case 0x5000:
                    if (V[opcode & 0x0f00 >> 8] == V[(opcode & 0x00f0) >> 4])
                        PC += 2;
                    break;
                case 0x6000:
                    V[opcode & 0x0f00 >> 8] = (byte)(opcode & 0x00ff);
                    break;
                case 0x7000:
                    V[opcode & 0x0f00 >> 8] += (byte)(opcode & 0x00ff);
                    break;
                case 0x8000:
                    Opcode8(opcode);
                    break;
                case 0x9000:
                    if (V[opcode & 0x0f00 >> 8] != V[(opcode & 0x00f0) >> 4])
                        PC += 2;
                    break;
                case 0xA000:
                    I = (ushort)(opcode & 0x0fff);
                    break;
                case 0xB000:
                    PC = (ushort)((opcode & 0x0fff) + V[0]);
                    break;
                case 0xC000:
                    V[(opcode & 0xF00 >> 8)] = (byte)(generator.Next() & (opcode & 0x00ff));
                    break;
                case 0xD000:
                    OpcodeD(opcode);
                    break;
                case 0xE000:
                    OpcodeE(opcode);
                    break;
                case (0xF000):
                    OpcodeF(opcode);
                    break;
                default:
                    throw new NotSupportedException($"Unknown operation {opcode:X4}");
            }

        }

        private void OpcodeF(ushort opcode)
        {
            var x = opcode & 0x0F00 >> 8;
            switch (opcode >> 0x00ff)
            {
                case 0x07:
                    V[x] = DelayTimer;
                    break;
                case 0x0A:
                    WaitForKeyPress = true;
                    break;
                case 0x15:
                    DelayTimer = V[x];
                    break;
                case 0x18:
                    SoundTimer = V[x];
                    break;
                case 0x1e:
                    I += V[x];
                    break;
                case 0x29:
                    throw new NotImplementedException($"Not implemented yet {opcode:X4}");
                    break;
                case 0x33:
                    throw new NotImplementedException($"Not implemented yet {opcode:X4}");
                    break;
                case 0x55:
                    throw new NotImplementedException($"Not implemented yet {opcode:X4}");
                    break;
                case 0x65:
                    throw new NotImplementedException($"Not implemented yet {opcode:X4}");
                    break;
                default:
                    throw new NotSupportedException($"Not implemented yet {opcode:X4}");
            }
        }

        private void OpcodeE(ushort opcode)
        {
            var x = opcode & 0x0F00 >> 8;
            switch (opcode & 0x00FF)
            {
                case 0x9e:
                    if (((Keyboard >> V[x]) & 0x01) == 0x01)
                        PC += 2;
                    break;
                case 0xa1:
                    if (((Keyboard >> V[x]) & 0x01) != 0x01)
                        PC += 2;
                    break;
                default:
                    throw new NotSupportedException($"Not supported opcode {opcode:X4}.");
            }
        }

        private void OpcodeD(ushort opcode)
        {
            var x = V[(opcode & 0x0F00) >> 8];
            var y = V[(opcode & 0x00F0) >> 4];
            var n = (opcode & 0x000F);
            V[15] = 0;
            for (int i = 0; i < n; i++)
            {
                byte mem = RAM[I];
                for (int j = 0; j < 8; j++)
                {
                    byte pixel = (byte)((mem >> 7 - j) & 0x01);
                    var index = x + j + (y + i) * 64;
                    if (pixel == 1 && Display[index] == 1)
                        V[15] = 1;
                    Display[index] = (byte)(Display[index] ^ pixel);
                }
            }
        }

        private void Opcode8(ushort opcode)
        {
            int vx = (opcode & 0x0F00) >> 8;
            int vy = (opcode & 0x00F0) >> 4;
            switch (opcode & 0x000f)
            {
                case 0:
                    V[vx] = V[vy];
                    break;
                case 1:
                    V[vx] = (byte)(V[vx] | V[vy]);
                    break;
                case 2:
                    V[vx] = (byte)(V[vx] & V[vy]);
                    break;
                case 3:
                    V[vx] = (byte)(V[vx] ^ V[vy]);
                    break;
                case 4:
                    V[15] = (byte)(V[vx] + V[vy] > 255 ? 1 : 0);
                    V[vx] = (byte)((V[vx] + V[vy]) & 0x00FF);
                    break;
                case 5:
                    V[15] = (byte)(V[vx] > V[vy] ? 1 : 0);
                    V[vx] = (byte)((V[vx] - V[vy]) & 0x00FF);
                    break;
                case 6:
                    V[15] = (byte)(V[vx] & 0x0001);
                    V[vx] = (byte)(V[vx] >> 1);
                    break;
                case 7:
                    V[15] = (byte)(V[vy] > V[vx] ? 1 : 0);
                    V[vx] = (byte)((V[vy] - V[vx]) & 0x00FF);
                    break;
                case 14:
                    V[15] = (byte)(((V[vx] & 0x80) == 0x80) ? 1 : 0);
                    V[vx] = (byte)(V[vx] << 1);
                    break;
                default:
                    throw new NotSupportedException($"Not supported opcode {opcode:X4}.");
            }
        }

        private void Opcode1(ushort opcode)
        {
            if (opcode == 0x00e0) // Clear the screen
            {
                for (int i = 0; i < Display.Length; i++)
                    Display[i] = 0;
            }
            else if (opcode == 0x00ee) // Returns from a subroutine
            {
                PC = Stack.Pop();
            }
            else
            {
                // 0NNN could be implemented here
                throw new NotSupportedException($"Not supported opcode {opcode:X4}.");
            }
        }
    }
}
