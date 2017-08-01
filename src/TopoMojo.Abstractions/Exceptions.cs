using System;

namespace TopoMojo
{
    public class ConsoleDisplayException : System.Exception
    {
        public ConsoleDisplayException() { }
        public ConsoleDisplayException( string message ) : base( message ) { }
        public ConsoleDisplayException( string message, System.Exception inner ) : base( message, inner ) { }
    }
}