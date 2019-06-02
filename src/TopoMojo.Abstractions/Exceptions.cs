// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

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