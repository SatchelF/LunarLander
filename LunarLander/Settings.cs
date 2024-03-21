using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract]
public class GameSettings
{
    [DataMember]
    public Dictionary<string, string> KeyBindings { get; set; }

    public GameSettings()
    {
        KeyBindings = new Dictionary<string, string>
        {
            {"Rotate Left", "Left"},
            {"Rotate Right", "Right"},
            {"Thrust", "Up"}
        };
    }
}
