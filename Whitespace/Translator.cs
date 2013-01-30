using System;

namespace Whitespace
{
    public class Translator : ICommand
    {
        // Stack Manipulation
        public void Push(int n)
        {
            Console.WriteLine("PUSH {0}", n);
        }

        public void Duplicate()
        {
            Console.WriteLine("DUP");
        }

        public void Copy(int index)
        {
            Console.WriteLine("COPY {0}th");
        }

        public void Swap()
        {
            Console.WriteLine("SWAP");
        }

        public void Discard()
        {
            Console.WriteLine("POP");
        }

        public void Slide(int n)
        {
            Console.WriteLine("SLIDE " + n);
        }
        
        // Arithmetic
        public void Add()
        {
            Console.WriteLine("ADD");
        }

        public void Sub()
        {
            Console.WriteLine("SUB");
        }

        public void Mul()
        {
            Console.WriteLine("MUL");
        }

        public void Div()
        {
            Console.WriteLine("DIV");
        }

        public void Mod()
        {
            Console.WriteLine("MOD");
        }
        
        // Heap Access
        public void Store()
        {
            Console.WriteLine("STORE");
        }

        public void Retrieve()
        {
            Console.WriteLine("LOAD");
        }
        
        // Flow Control
        public bool? Test(Func<int, bool> f)
        {
            Console.WriteLine("TEST");
            return null;
        }

        public void End()
        {
            Console.WriteLine("END");
        }

        // IO
        public void OutputCharacter()
        {
            Console.WriteLine("OUTCHR");
        }

        public void OutputNumber()
        {
            Console.WriteLine("OUTNUM");
        }

        public void ReadCharacter()
        {
            Console.WriteLine("READCHR");
        }

        public void ReadNumber()
        {
            Console.WriteLine("READNUM");
        }

        // Debug
        public void ShowStack()
        {
        }

        public void ShowHeap()
        {
        }
    }
}

