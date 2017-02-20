using System;

namespace TopoMojo
{
    public class ConsoleDiplayException : System.Exception
    {
        public ConsoleDiplayException() { }
        public ConsoleDiplayException( string message ) : base( message ) { }
        public ConsoleDiplayException( string message, System.Exception inner ) : base( message, inner ) { }
    }
}