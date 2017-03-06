
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Strategy
{
    private static Random rand = new Random();

    public static T RandomSelect<T>(List<T> sequence)
    {
        return sequence[rand.Next(sequence.Count)];
    }
}
