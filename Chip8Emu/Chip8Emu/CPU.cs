using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class CPU
{

    Random random = new Random();

    byte[] chip8_fontset = new byte[] {
        0xF0, 0x90, 0x90, 0x90, 0xF0, //0
        0x20, 0x60, 0x20, 0x20, 0x70, //1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, //2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, //3
        0x90, 0x90, 0xF0, 0x10, 0x10, //4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, //5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, //6
        0xF0, 0x10, 0x20, 0x40, 0x40, //7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, //8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, //9
        0xF0, 0x90, 0xF0, 0x90, 0x90, //A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, //B
        0xF0, 0x80, 0x80, 0x80, 0xF0, //C
        0xE0, 0x90, 0x90, 0x90, 0xE0, //D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, //E
        0xF0, 0x80, 0xF0, 0x80, 0x80  //F
    };

    byte[] memory = new byte[0xFFF];
    byte[] registers = new byte[16];

    /*
        0x000-0x1FF - Chip 8 interpreter (contains font set in emu)
        0x050-0x0A0 - Used for the built in 4x5 pixel font set (0-F)
        0x200-0xFFF - Program ROM and work RAM
    */

    //value from 0x000 0xFFF
    ushort indexRegister;
    ushort pointer;

    public byte[] pixels = new byte[32* 64]; 

    byte delayTimer;
    byte soundTimer;


    //points to one above currently executing 
    Stack<ushort> pointerStack = new Stack<ushort>(16);
    OpCodes opCodes;

    public CPU() => opCodes = new OpCodes(this);


    //intialize
    void Initialzie()
    {
        pointer = 0x200;
        indexRegister = 0;
        pointerStack.Clear();
        soundTimer = 0;
        delayTimer = 0;
        opCodes.opcode = 0;

        memory = new byte[0xFFF];
        registers = new byte[16];
        pixels = new byte[32 * 64];

        chip8_fontset.CopyTo(memory, 0);
    }

    //load game
    public void LoadGame(string game)
    {
        Initialzie();

        var programData = File.ReadAllBytes($"games/{game}");
        programData.CopyTo(memory, 0x200);
    }
    //set keys

    public void EmulateCycle()
    {
        // Fetch Opcode
        ushort opcode = (ushort)(memory[pointer] << 8 | memory[pointer+1]);
        // Decode Opcode and Execute Opcode
        opCodes.RunOpCode(opcode);
        // Update timers
        if (delayTimer > 0)
            delayTimer--;

        if (soundTimer > 0)
            soundTimer--;
    }

    class OpCodes
    {
        CPU cpu;

        public ushort opcode;
        public OpCodes(CPU cpu) => this.cpu = cpu;

        public void RunOpCode(ushort _opCode)
        {
            opcode = _opCode;
            Console.WriteLine("optcode = {0:X}, pointer = {1:X}", opcode, cpu.pointer);
            //decode opcode
            //kollar första byten
            switch (opcode & 0xF000)
            {
                //0NNN eller 00E0 eller 00EE
                case 0x0000:
                    _0x0();
                    break;

                //1NNN
                case 0x1000:
                    _0x1();
                    break;

                //2NNN
                case 0x2000:
                    _0x2();
                    break;

                //3XNN
                case 0x3000:
                    _0x3();
                    break;


                //4XNN
                case 0x4000:
                    _0x4();
                    break;

                //5XY0
                case 0x5000:
                    _0x5();
                    break;

                //6XNN
                case 0x6000:
                    _0x6();
                    break;

                //7XNN
                case 0x7000:
                    _0x7();
                    break;

                //8XY0
                case 0x8000:
                    _0x8();
                    break;

                //9XY0
                case 0x9000:
                    _0x9XY0();
                    break;


                //ANNN
                case 0xA000:
                    _0xANNN();
                    break;

                //BNNN
                case 0xB000:
                    _0xBNNN();
                    break;
                //CXNN
                case 0xC000:
                    _0xCXNN();
                    break;
                //DXYN
                case 0xD000:
                    _0xDXYN();
                    break;
                //EX9E
                case 0xE000:
                    _0xE();
                    break;

                //FX07
                case 0xF000:
                    _0xF();
                    break;


            }
            cpu.pointer += 2;

        }




        void _0x0()
        {
            if ((opcode & 0x00FF) == 0x0E0)
                _0x0E0();
            else if ((opcode & 0x00FF) == 0x0EE)
                _0x0EE();
            else
                _0xNNN();
        }

        //no fucking clue
        void _0xNNN() => throw new NotImplementedException("No fucking clue what its supposed to do");
        void _0x0E0() => Array.Clear(cpu.pixels, 0, cpu.pixels.Length);
        //Return from subroutine
        void _0x0EE() => cpu.pointer = cpu.pointerStack.Pop();


        /// <summary>
        /// Jumps to adress 0x0NNN
        /// </summary>
        void _0x1()
        {
            cpu.pointer = (ushort)(opcode & 0x0FFF);
            
            cpu.pointer -= 2;

        }

        /// <summary>
        /// call subroutine
        /// </summary>
        void _0x2()
        {
            //sparar pointern i stacken och sätter pointer till  den address som anropats
            cpu.pointerStack.Push(cpu.pointer);
            //effektivt 1NNN
            _0x1();
        }

        /// <summary>
        /// Skips the next instruction if Register[0x0X00] == 0x00NN
        /// if != to
        /// </summary>
        void _0x3()
        {
            if (cpu.registers[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))
                cpu.pointer += 2;
        }

        /// <summary>
        /// Skips the next instruction if Register[0x0X00] != 0x00NN
        /// if == to
        /// </summary>
        void _0x4()
        {
            if (cpu.registers[(opcode & 0x0F00) >> 8] != (opcode & 0x00FF))
                cpu.pointer += 2;
        }

        /// <summary>
        /// Skips the next instruction if Register[0x0X00] == Register[0x00Y0]
        /// if != to
        /// </summary>
        void _0x5()
        {
            if (cpu.registers[(opcode & 0x0F00) >> 8] == cpu.registers[(opcode & 0x00F0) >> 4])
                cpu.pointer += 2;
        }

        /// <summary>
        /// Register[0x0X00] = 0x00NN;
        /// </summary>
        void _0x6() => cpu.registers[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF);

        /// <summary>
        /// Register[0x0X00] += 0x00NN;
        /// </summary>
        void _0x7() => cpu.registers[(opcode & 0x0F00) >> 8] += (byte)(opcode & 0x00FF);


        void _0x8()
        {
            switch (opcode & 0x000F)
            {
                case 0x0000:
                    _0x8XY0();
                    break;
                case 0x0001:
                    _0x8XY1();
                    break;
                case 0x0002:
                    _0x8XY2();
                    break;
                case 0x0003:
                    _0x8XY3();
                    break;
                case 0x0004:
                    _0x8XY4();
                    break;
                case 0x0005:
                    _0x8XY5();
                    break;
                case 0x0006:
                    _0x8XY6();
                    break;
                case 0x0007:
                    _0x8XY7();
                    break;
                case 0x000E:
                    _0x8XYE();
                    break;


            }


        }

        void _0x8XY0() => cpu.registers[(opcode & 0x0F00) >> 8] = cpu.registers[(opcode & 0x00F0) >> 4];
        void _0x8XY1() => cpu.registers[(opcode & 0x0F00) >> 8] |= cpu.registers[(opcode & 0x00F0) >> 4];
        void _0x8XY2() => cpu.registers[(opcode & 0x0F00) >> 8] &= cpu.registers[(opcode & 0x00F0) >> 4];
        void _0x8XY3() => cpu.registers[(opcode & 0x0F00) >> 8] ^= cpu.registers[(opcode & 0x00F0) >> 4];

        /// <summary>
        /// Register[0x0F00] += Register[0x00F0];
        /// VF is set to one when theres a carry and 0 when there isnt
        /// </summary>
        void _0x8XY4()
        {
            int reg = cpu.registers[(opcode & 0x0F00) >> 8] + cpu.registers[(opcode & 0x00F0) >> 4];

            byte carryValue = 0;
            if (reg > byte.MaxValue)
                carryValue = 1;

            cpu.registers[cpu.registers.Length - 1] = carryValue;
            cpu.registers[(opcode & 0x0F00) >> 8] += cpu.registers[(opcode & 0x00F0) >> 4];
        }
        /// <summary>
        /// Register[0x0F00] += Register[0x00F0];
        /// VF is set to one when theres a carry and 0 when there isnt
        /// </summary>
        void _0x8XY5()
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            var yReg = cpu.registers[y];


            byte notUnderflow = 1;
            if (yReg > cpu.registers[x])
                notUnderflow = 0;

            cpu.registers[cpu.registers.Length - 1] = notUnderflow;

            cpu.registers[x] -= yReg;

        }

        void _0x8XY6()
        {
            var x = (opcode & 0x0F00) >> 8;
            cpu.registers[cpu.registers.Length - 1] = (byte)(cpu.registers[x] & 0x0001);
            cpu.registers[x] >>= 1;


        }

        void _0x8XY7()
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            var yReg = cpu.registers[y];
            var xReg = cpu.registers[x];


            byte notUnderflow = 1;
            if (xReg > yReg)
                notUnderflow = 0;

            cpu.registers[cpu.registers.Length - 1] = notUnderflow;
            cpu.registers[x] = (byte)(yReg - xReg);
        }

        void _0x8XYE()
        {
            var x = (opcode & 0x0F00) >> 8;
            cpu.registers[cpu.registers.Length - 1] = (byte)((cpu.registers[x] & 0b1000_0000) >> 7);
            cpu.registers[x] <<= 1;
        }
        void _0x9XY0()
        {
            if (cpu.registers[(opcode & 0x0F00) >> 8] != cpu.registers[(opcode & 0x00F0) >> 4])
                cpu.pointer += 2;
        }

        void _0xANNN() => cpu.indexRegister = (ushort)(opcode & 0x0FFF);

        void _0xBNNN()
        {
            cpu.pointer = (ushort)(cpu.registers[0] + (opcode & 0x0FFF));
            cpu.pointer -= 2;
        }
        void _0xCXNN() => cpu.registers[(opcode & 0x0F00) >> 8] = (byte)(cpu.random.Next(byte.MaxValue) & (opcode & 0x00FF));
     

        void _0xDXYN()
        {
            ushort x = cpu.registers[(opcode & 0x0F00) >> 8];
            ushort y = cpu.registers[(opcode & 0x00F0) >> 4];
            ushort height = (ushort)(opcode & 0x000F);
            ushort pixelRow;

            byte flagValue = 0;
            for (int yline = 0; yline < height; yline++) {
                pixelRow = cpu.memory[cpu.indexRegister + yline];
                for (int xline = 0; xline < 8; xline++) {
                    if ((pixelRow & ((0b1000_0000) >> xline)) != 0) {
                        if (cpu.pixels[(x + xline) + ((y + yline) * 64) ] == 1) {
                            flagValue = 1;
                        }
                        cpu.pixels[x + xline + ((y + yline)* 64)] ^= 1;
                    }
                }
            }
            cpu.registers[0xF] = flagValue;
        }

        void _0xE()
        {
            var down = Input.IsKeyDown(cpu.registers[((opcode & 0x0F00) >> 8)]);
            if ((opcode & 0x00FF) == 0x00A1)
                down = !down;

            if (down)
                cpu.pointer += 2;
        }

        void _0xF()
        {
            switch ((opcode & 0x00FF))
            {
                case 0x0007:
                    _0xFX07();
                    break;

                case 0x000A:
                    _0xFX0A();
                    break;

                case 0x0015:
                    _0xFX15();
                    break;

                case 0x0018:
                    _0xFX18();
                    break;

                case 0x001E:
                    _0xFX1E();
                    break;

                case 0x0029:
                    _0xFX29();
                    break;

                case 0x0033:
                    _0xFX33();
                    break;

                case 0x0055:
                    _0xFX55();
                    break;

                case 0x0065:
                    _0xFX65();
                    break;
            }
        }

        void _0xFX07()
        {
            cpu.registers[(opcode & 0x0F00) >> 8] = cpu.delayTimer;
        }

        void _0xFX0A()
        {
            if (Input.InputsDown.Any())
                return;

            //keep same position in code
            cpu.pointer -= 2;

        }

        void _0xFX15() => cpu.delayTimer = cpu.registers[(opcode & 0x0F00) >> 8];

        void _0xFX18() => cpu.soundTimer = cpu.registers[(opcode & 0x0F00) >> 8];

        void _0xFX1E() => cpu.indexRegister += cpu.registers[(opcode & 0x0F00) >> 8];


        void _0xFX29()
        {
            cpu.indexRegister = (ushort)(((opcode & 0x0F00) >> 8) * 5);
        }

        void _0xFX33()
        {
            int num = cpu.registers[(opcode & 0x0F00) >> 8];
            cpu.memory[cpu.indexRegister + 2] = (byte)(num % 10);
            cpu.memory[cpu.indexRegister + 1] = (byte)((num % 100) / 10);
            cpu.memory[cpu.indexRegister] = (byte)((num % 1000) / 100);

        }

        void _0xFX55()
        {
            var ir = cpu.indexRegister;
            byte x = (byte)((opcode & 0x0F00) >> 8);
            for (byte i = 0; i <= x; i++)
                cpu.memory[ir + i] = cpu.registers[i];

            //Enligt turorial: On the original interpreter, when the operation is done, I = I + X + 1.
            //inte enligt wikipedia
            cpu.indexRegister += (byte)(x + 1);
        }

        void _0xFX65()
        {
            var ir = cpu.indexRegister;
            byte x = (byte)((opcode & 0x0F00) >> 8);
            for (byte i = 0; i <= x; i++)
                cpu.registers[i] = cpu.memory[ir + i];

            // Enligt turorial: On the original interpreter, when the operation is done, I = I + X + 1.
            //inte enligt wikipedia
            cpu.indexRegister += (byte)(x + 1);
        }
    }
}
