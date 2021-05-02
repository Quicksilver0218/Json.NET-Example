using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class Program {
    public static void Main()  {

        // Creates a new TestType object by using a constructor from its subclass.
        TestType obj = new Sub();
        obj.SetValues();

        Console.WriteLine("Before serialization: ");
        obj.Print();
        /*  Output:

        This is a subclass.
        member1 = '11'
        member2 = 'hello'
        member3 = 'hello'
        member4 = '3.14159265'
        member5 = 'hello world!'
        member6 =
        '123: 123'
        '456: 456'
        member7 = '(1, 2)'
        member8 = '(234, 567)'
        */

        // We should use a custom serialization binder to serialize the object so that we can serialize the type of the object
        TypeBinder typeBinder = new TypeBinder();
        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects, SerializationBinder = typeBinder };

        // Opens file "data.json" and serializes the object to it.
        using (Stream stream = File.Open("data.json", FileMode.Create)) {
            string json = JsonConvert.SerializeObject(obj, settings);
            stream.Write(Encoding.ASCII.GetBytes(json), 0, json.Length);
        }

        // Empties obj.
        obj = null;

        // Opens file "data.json" and deserializes the object from it.
        using (StreamReader streamReader = File.OpenText("data.json")) {
            string json = streamReader.ReadToEnd();
            obj = JsonConvert.DeserializeObject<TestType>(json, settings);
        }

        Console.WriteLine();
        Console.WriteLine("After deserialization: ");
        obj.Print();
        /* Output:

        This is a subclass.
        member1 = '11'
        member2 = 'hello'
        member3 = ''
        member4 = '3.14159265'
        member5 = 'hello world!'
        member6 =
        '123: 123'
        '456: 456'
        member7 = '(1, 2)'
        member8 = '(234, 567)'
        */
    }
}

public class TypeBinder : ISerializationBinder
{
    public Type BindToType(string assemblyName, string typeName)
    {
        // In this example, we don't check the assembly name so we just get the type from the type name.
        // This is useful when you need to serialize and deserialize object between different assembly with the same class. e.g. between server and client
        return Type.GetType(typeName);
    }

    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        assemblyName = null;    // We don't use it. Just set it to null.
        typeName = serializedType.FullName; // Serialize the full name, including the package name, of the type.
    }
}

public class TestType  {
    [JsonProperty]
    private int member1;    // To serialize private member, [JsonProperty] attribute should be added.
    public string member2;
    [NonSerialized]
    public string member3;  // A field with the attribute [NonSerialized] will not be serialized.
    public double member4;
    public List<string> member5;    // Some popular C# data structures are serializable.
    public Dictionary<int, string> member6;
    public Vector2Int member7;  // An object with custom type.
    public (int i, string s) member8;   // Tuples are serializable.

    // Set sample values to the members
    public void SetValues() {
        member1 = 11;
        member2 = "hello";
        member3 = "hello";
        member4 = 3.14159265;
        member5 = new List<string> { "hello world!" };
        member6 = new Dictionary<int, string> {
            { 123, "123" },
            { 456, "456" }
        };
        member7 = new Vector2Int(1, 2);
        member8 = (234, "567");
    }

    public virtual void Print() {
        Console.WriteLine("member1 = '{0}'", member1);
        Console.WriteLine("member2 = '{0}'", member2);
        Console.WriteLine("member3 = '{0}'", member3);
        Console.WriteLine("member4 = '{0}'", member4);
        Console.WriteLine("member5 = '{0}'", member5[0]);
        Console.WriteLine("member6 = ");
        foreach (int key in member6.Keys)
            Console.WriteLine("'" + key + ": " + member6[key] + "'");
        Console.WriteLine("member7 = '{0}'", member7);
        Console.WriteLine("member8 = '{0}'", member8);
    }
}

public class Sub : TestType {
    public override void Print() {
        Console.WriteLine("This is a subclass.");
        base.Print();
    }
}

public class Vector2Int
{
    public readonly int x, y;

    public Vector2Int(int x, int y) {   // JsonConvert.DeserializeObject will call the constructor by matching the members' name to the arguments' name.
        this.x = x;
        this.y = y;
    }

    public override string ToString() {
        return "(" + x + ", " + y + ")";
    }
}