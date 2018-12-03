// Program1.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#pragma warning( disable : 4996 ) 
typedef unsigned char byte;
typedef unsigned short word;
unsigned char RAM[4096] = { 0 };
short Accu = 0;
int pc = 0;






// splits a word into two bytes than writes to ram[adr] and ram[adr+1] in little endian
void Store(word value, word adr)
{
	RAM[adr + 1] = (value >> 8) & 0xff;
	RAM[adr] = value & 0xFF;
}
// returns the 16bit hex stored at adr and adr+1 as a big endian
word Read_at(word adr)
{
	word temp = (RAM[adr + 1] << 8) + (RAM[adr] & 0xff);
	return temp;
}
// reads input from user
word Read_in(word mode)
{
	word A;
	if (mode == 0) {
		scanf("%x", &A);
	}
	else if (mode == 1)
	{
		scanf("%d", &A);
	}
	else
	{
		scanf("%c", &A);
	}
	return A;
}

// prints value to screen. modes: 0 = hex, 1 = dec, 2 = ASCII
void Write(word value, word mode)
{
	if (mode == 0) {
		printf("%x", value);
	}
	else if (mode == 1)
	{
		printf("%d", value);
	}
	else
	{
		printf("%c", value);
	}
}

// function to fetch a instruction from Ram at pc and exceute it
int Step()
{
	word  value = Read_at(pc); // full instruction code
	byte opcode = (value >> 12) & 0xf; // opcode
	word operand = value & 0xFFF; // operand

	switch (opcode)
	{
	case 0x0: //hlt
		pc = pc + 2;
		return 0;


	case 0x1: //not
		Accu = ~Accu;
		break;

	case 0x2: //shift left shl
		Accu = Accu << operand;
		break;

	case 0x3: //shift right shr
		Accu = Accu >> operand;
		break;

	case 0x4: //inc
		Accu++;
		break;

	case 0x5: //dec
		Accu--;
		break;

	case 0x6: //jmp
		pc = operand;
		return 1;

	case 0x7: //jaz
		if (Accu == 0)
			pc = operand;
		break;

	case 0x8: //lda
		Accu = Read_at(operand);
		break;

	case 0x9: //sta
		Store(Accu, operand);
		break;

	case 0xA: //add
		Accu += Read_at(operand);
		break;

	case 0xB: //and
		Accu = Accu & Read_at(operand);
		break;

	case 0xC: //orr
		Accu = Accu | Read_at(operand);
		break;

	case 0xD: //xor
		Accu = Accu ^ Read_at(operand);
		break;

	case 0xE: //out
		Write(Accu, operand);
		break;

	case 0xF: //inp
		Accu = Read_in(operand);


	default:
		break;
	}
	pc = pc + 2;

}
// function to call Step() until hlt is fetched
void Run()
{
	int x = 1;
	while (x)
	{
		x = Step();
	}
}





// main program
int main()
{
	unsigned char program[] = {
		0x80,0x1c,
		0xc0,0x1e,
		0x90,0x06,
		0x00,0x00,
		0xb0,0x20,
		0x70,0x16,
		0xe0,0x02,
		0x80,0x1c,
		0x40,0x00,
		0x90,0x1c,
		0x60,0x00,
		0x00,0x00,
		0x42,0x41,
		0x00,0x43,
		0x00,0x18,
		0x80,0x00,
		0x00,0xff
	};
	int size = sizeof(program) / sizeof(program[1]);
	for (int i = 0; i < size; i += 2)
	{
		word temp = program[i + 1] + (program[i] << 8);
		Store(temp, i);
	}

	char command = '0';

	while (command != 'q')
	{

		int j = 0;
		int lb = 0x0;

		printf("?");
		scanf("%c", &command);
		switch (command)
		{
		case 'd'://dump: usage d start length
			int start, len;
			scanf("%d %d", &start, &len);
			printf("000   ");
			for (int i = 0; i < (start + len); i++)
			{
				if (j == 16) {
					lb += 16;
					printf("\n%x   [%x] ", lb, RAM[i]);
					j = 0;
				}
				else
					printf("[%x] ", RAM[i]);
				j++;
			}
			printf("\n");
			break;
		case 'a':	// shows the current value of pc and accumulator 
			printf("Accumulator: %d\n", Accu);
			printf("PC: %d\n", pc);
			break;
		case'q': //exits the program
			break;

		case 's':	// calls Step()
			Step();
			break;
		case 'r':	// calls Run()
			Run();
			break;
		case 'e':	//edits a value in ram. usage: e address value_to_input
			byte addr, val;
			scanf("%x %x", &addr, &val);

			if (val > 0xff || addr > 0xfff)
				printf("---invalid input---\n");
			else
			{
				RAM[addr] = val;
			}
			break;
		case 'h': // display a help string
			printf("\nCommands: Quit{ q },  Dump Ram{ d start length }, Print pc and accum{ a }\n Run{ r }, Step{ s }, edit ram{ e address value } \n");
			break;

		default:
			break;
		}
	}

	return 0;
}


