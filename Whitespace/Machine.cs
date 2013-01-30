using System;
using System.Collections.Generic;
using System.Linq;

namespace Whitespace
{
    public class Machine : ICommand
    {
        Stack<int> stack;
        Dictionary<int, int> heap;

        public Machine()
        {
            stack = new Stack<int>();
            heap = new Dictionary<int, int>();
        }

        // Stack Manipulation
        public void Push(int n)
        {
            stack.Push(n);
        }
        
        public void Duplicate()
        {
            stack.Push(stack.Peek());
        }
        
        public void Copy(int index)
        {
            stack.Push(stack.ElementAt(index));
        }
        
        public void Swap()
        {
            var first = stack.Pop();
            var second = stack.Pop();
            stack.Push(first);
            stack.Push(second);
        }
        
        public void Discard()
        {
            stack.Pop();
        }
        
        public void Slide(int n)
        {
            // ?
        }
        
        // Arithmetic
        public void Add()
        {
            stack.Push(stack.Pop() + stack.Pop());
        }
        
        public void Sub()
        {
            stack.Push(stack.Pop() - stack.Pop());
        }
        
        public void Mul()
        {
            stack.Push(stack.Pop() * stack.Pop());
        }
        
        public void Div()
        {
            stack.Push(stack.Pop() / stack.Pop());
        }
        
        public void Mod()
        {
            stack.Push(stack.Pop() % stack.Pop());
        }
        
        // Heap Access
        public void Store()
        {
            var value = stack.Pop();
            var address = stack.Pop();
            heap[address] = value;
        }
        
        public void Retrieve()
        {
            var address = stack.Pop();
            stack.Push(heap[address]);
        }
        
        // Flow Control
        public bool? Test(Func<int, bool> f)
        {
            return f(stack.Peek());
        }

        public void End()
        {
            throw new AbortException();
        }

        // IO
        public void OutputCharacter()
        {
            Console.Write((char)stack.Pop());
        }
        
        public void OutputNumber()
        {
            Console.Write(stack.Pop());
        }
        
        public void ReadCharacter()
        {
            heap[stack.Pop()] = (char)Console.Read();
        }
        
        public void ReadNumber()
        {
            heap[stack.Pop()] = Console.Read();
        }

        // Debug
        public void ShowStack()
        {
            foreach (var item in stack.Select((v, i) => new {Value = v, Index = i}))
            {
                Console.WriteLine("{0}: {1}", item.Index, item.Value);
            }
        }
        
        public void ShowHeap()
        {
            foreach (var item in heap)
            {
                Console.WriteLine("{0}: {1}", item.Key, item.Value);
            }
        }
    }
}

