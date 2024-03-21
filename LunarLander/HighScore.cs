using System;
using System.Runtime.Serialization;

[DataContract(Name = "HighScore")]
public class HighScore
{
    [DataMember]
    public int FuelRemaining { get; set; }

    [DataMember]
    public DateTime Date { get; set; } 

    public HighScore(int fuelRemaining)
    {
        FuelRemaining = fuelRemaining;
        Date = DateTime.Now; // Set the current date and time
    }
}
