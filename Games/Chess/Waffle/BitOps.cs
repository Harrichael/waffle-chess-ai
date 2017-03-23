
using System;
using System.Collections.Generic;
using System.Linq;

public static class BitOps
{
    public static UInt64 MSB(UInt64 input)
    {
        if (input == 0) return 0;
    
        UInt64 msb = 1;
    
        if ((input >> 32) == 0) 
        {
            input = input << 32;
        } else {
            msb = msb << 32;
        }
        if ((input >> 48) == 0) 
        {
            input = input << 16;
        } else {
            msb = msb << 16;
        }
        if ((input >> 56) == 0) 
        {
            input = input << 8;
        } else {
            msb = msb << 8;
        }
        if ((input >> 60) == 0) 
        {
            input = input << 4;
        } else {
            msb = msb << 4;
        }
        if ((input >> 62) == 0) 
        {
            input = input << 2;
        } else {
            msb = msb << 2;
        }
        if ((input >> 63) == 0) 
        {
            input = input << 1;
        } else {
            msb = msb << 1;
        }
    
        return msb;
    } // End MSB

    public static UInt64 CountBits(UInt64 val)
    {
        UInt64 count = 0;
        while (val != 0)
        {
            count = count + 1;
            val = val & (val - 1);
        }
        return count;
    } // End CountBits

    public static byte bbIndex(UInt64 bb)
    {
        switch (bb)
        {
            case 1: return 1;
            case 2: return 2;
            case 4: return 3;
            case 8: return 4;
            case 16: return 5;
            case 32: return 6;
            case 64: return 7;
            case 128: return 8;
            case 256: return 9;
            case 512: return 10;
            case 1024: return 11;
            case 2048: return 12;
            case 4096: return 13;
            case 8192: return 14;
            case 16384: return 15;
            case 32768: return 16;
            case 65536: return 17;
            case 131072: return 18;
            case 262144: return 19;
            case 524288: return 20;
            case 1048576: return 21;
            case 2097152: return 22;
            case 4194304: return 23;
            case 8388608: return 24;
            case 16777216: return 25;
            case 33554432: return 26;
            case 67108864: return 27;
            case 134217728: return 28;
            case 268435456: return 29;
            case 536870912: return 30;
            case 1073741824: return 31;
            case 2147483648: return 32;
            case 4294967296: return 33;
            case 8589934592: return 34;
            case 17179869184: return 35;
            case 34359738368: return 36;
            case 68719476736: return 37;
            case 137438953472: return 38;
            case 274877906944: return 39;
            case 549755813888: return 40;
            case 1099511627776: return 41;
            case 2199023255552: return 42;
            case 4398046511104: return 43;
            case 8796093022208: return 44;
            case 17592186044416: return 45;
            case 35184372088832: return 46;
            case 70368744177664: return 47;
            case 140737488355328: return 48;
            case 281474976710656: return 49;
            case 562949953421312: return 50;
            case 1125899906842624: return 51;
            case 2251799813685248: return 52;
            case 4503599627370496: return 53;
            case 9007199254740992: return 54;
            case 18014398509481984: return 55;
            case 36028797018963968: return 56;
            case 72057594037927936: return 57;
            case 144115188075855872: return 58;
            case 288230376151711744: return 59;
            case 576460752303423488: return 60;
            case 1152921504606846976: return 61;
            case 2305843009213693952: return 62;
            case 4611686018427387904: return 63;
            case 9223372036854775808: return 64;
            default:
                Console.WriteLine("Incorrect bitboard, should have 1 set bit");
                throw new System.Exception();
        }
    } // End bbIndex

}
