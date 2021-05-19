using System;

[assembly: FunctionName("abc")]

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //HelloWorldGenerated.HelloWorld.SayHello();

            Console.WriteLine(FunctionName.Endpoint.Name);
        }
    }
}
