using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Whitespace
{
    public class Interpreter
    {
        Stream stream;
        StreamReader reader;
        ICommand proc;
        Dictionary<string, long> labels;
        Stack<long> callStack;
        List<long> breakPoints;
        string lastCommand;
        bool isStepByStep;

        public static void Run(Stream stream, ICommand proc)
        {
            Run(stream, proc, false);
        }

        public static void Run(Stream stream, ICommand proc, bool step)
        {
            var m = new Interpreter(stream, proc);
            m.Run(step);
        }

        void Run(bool defaultStep)
        {
            isStepByStep = defaultStep;
            try
            {
                reader = new StreamReader(stream);
                Console.WriteLine("=== Preprocess ===");
                // Label Dictionary
                while(MoveToNextToken())
                {
                    switch (GetIMP())
                    {
                        case IMP.StackManipulation:
                            StackManipulationPreProc();
                            break;
                        case IMP.Arithmetic:
                            ArithmeticPreProc();
                            break;
                        case IMP.HeapAccess:
                            HeapAccessPreProc();
                            break;
                        case IMP.FlowControl:
                            FlowControlPreProc();
                            break;
                        case IMP.IO:
                            IOPreProc();
                            break;
                    }
                }

                Console.WriteLine("=== Run Main ===");
                stream.Seek(0, SeekOrigin.Begin);
                // Main Process
                while(MoveToNextToken())
                {
                    if (breakPoints.Contains(stream.Position))
                        isStepByStep = true;

                    if (isStepByStep)
                    {
                        while(true)
                        {
                            Console.Write("{0} > ", stream.Position);
                            var line = Console.ReadLine();
                            if (DebugCommand(line))
                                break;
                        }
                    }

                    switch (GetIMP())
                    {
                        case IMP.StackManipulation:
                            StackManipulationProc();
                            break;
                        case IMP.Arithmetic:
                            ArithmeticProc();
                            break;
                        case IMP.HeapAccess:
                            HeapAccessProc();
                            break;
                        case IMP.FlowControl:
                            FlowControlProc();
                            break;
                        case IMP.IO:
                            IOProc();
                            break;
                    }
                }
            }
            catch(AbortException)
            {
                Console.WriteLine("Aborted");
            }
            finally
            {
                reader.Dispose();
                reader = null;
            }
        }

        private Interpreter(Stream stream, ICommand proc)
        {
            this.stream = stream;
            this.proc = proc;
            labels = new Dictionary<string, long>();
            callStack = new Stack<long>();
            breakPoints = new List<long>();
            lastCommand = string.Empty;
            isStepByStep = false;
        }

        bool DebugCommand(string command)
        {
            var sp = command.Split(new[] {' '}, 2);
            switch (sp[0])
            {
                case "cont":
                    isStepByStep = false;
                    lastCommand = sp[0];
                    return true;
                case "step":
                    lastCommand = sp[0];
                    return true;
                case "stack":
                    proc.ShowStack();
                    break;
                case "heap":
                    proc.ShowHeap();
                    break;
                case "call":
                    foreach (var item in callStack.Select((v, i) => new {Value = v, Index = i}))
                    {
                        Console.WriteLine("{0}: {1}", item.Index, item.Value);
                    }
                    break;
                case "label":
                    foreach (var item in labels)
                    {
                        Console.WriteLine("{0}: {1}", item.Key, item.Value);
                    }
                    break;
                case "break":
                    if (sp[1] != null && !string.IsNullOrWhiteSpace(sp[1]))
                    {
                        long pos;
                        if (!long.TryParse(sp[1], out pos))
                        {
                            Console.WriteLine("!!! Can't parse.");
                            return false;
                        }
                        if (pos < 0 && pos <= stream.Length)
                        {
                            Console.WriteLine("!!! Out of Range");
                            return false;
                        }
                        breakPoints.Add(pos);
                    }
                    break;
                case "breaks":
                    foreach(var item in breakPoints.Select((v, i) => new {Value = v, Index = i}))
                    {
                        Console.WriteLine("{0}: {1}", item.Index, item.Value);
                    }
                    break;
                case "":
                    return DebugCommand(lastCommand);
                default:
                    return false;
            }
            lastCommand = sp[0];
            return false;
        }

        bool MoveToNextToken()
        {
            int i;
            while((i = reader.Peek()) != -1)
            {
                var c = (char)i;
                if (c == ' ' || c == '\t' || c == '\n')
                    return true;
                reader.Read();
            }
            return false;
        }
        
        int GetNumber()
        {
            return Convert.ToInt32(GetLiteral(), 2);
        }
        
        string GetLabel()
        {
            return GetLiteral();
        }

        string GetLiteral()
        {
            var builder = new StringBuilder();
            while(MoveToNextToken())
            {
                var c = (char)reader.Read();
                switch(c)
                {
                    case ' ':
                        builder.Append('0');
                        break;
                    case '\t':
                        builder.Append('1');
                        break;
                    case '\n':
                        return builder.ToString();
                }
            }
            throw new FormatException("Unexpected EOF");
        }
        
        IMP GetIMP()
        {
            var c = (char)reader.Read();
            switch(c)
            {
                case ' ':
                    return IMP.StackManipulation;
                case '\t':
                    if (!MoveToNextToken())
                        throw new FormatException("Unexpected EOF");
                    var next = (char)reader.Read();
                    switch(next)
                    {
                        case ' ':
                            return IMP.Arithmetic;
                        case '\t':
                            return IMP.HeapAccess;
                        case '\n':
                            return IMP.IO;
                    }
                    throw new FormatException();
                case '\n':
                    return IMP.FlowControl;
            }
            throw new FormatException();
        }

        void StackManipulationProc()
        {
            var c = (char)reader.Read();
            char next;
            switch (c)
            {
                case ' ':
                    proc.Push(GetNumber());
                    break;
                case '\t':
                    if (!MoveToNextToken())
                        throw new FormatException("Unexpected EOF");
                    next = (char)reader.Read();
                    switch(next)
                    {
                        case ' ':
                            // Copy nth stack (param: number)
                            proc.Copy(GetNumber());
                            break;
                        case '\n':
                            proc.Slide(GetNumber());
                            break;
                    }
                    break;
                case '\n':
                    if (!MoveToNextToken())
                        throw new FormatException("Unexpected EOF");
                    next = (char)reader.Read();
                    switch(next)
                    {
                        case ' ':
                            proc.Duplicate();
                            break;
                        case '\t':
                            proc.Swap();
                            break;
                        case '\n':
                            proc.Discard();
                            break;
                    }
                    break;
            }
        }
        
        void ArithmeticProc()
        {
            var c = (char)reader.Read();
            if (!MoveToNextToken())
                throw new FormatException("Unexpected EOF");
            var next = (char)reader.Read();
            switch (c)
            {
                case ' ':
                    switch(next)
                    {
                        case ' ':
                            proc.Add();
                            break;
                        case '\t':
                            proc.Sub();
                            break;
                        case '\n':
                            proc.Mul();
                            break;
                    }
                    break;
                case '\t':
                    switch(next)
                    {
                        case ' ':
                            proc.Div();
                            break;
                        case '\t':
                            proc.Mod();
                            break;
                    }
                    break;
                case '\n':
                    throw new FormatException();
            }
        }
        
        void HeapAccessProc()
        {
            var c = (char)reader.Read();
            switch(c)
            {
                case ' ':
                    proc.Store();
                    break;
                case '\t':
                    proc.Retrieve();
                    break;
            }
        }
        
        void FlowControlProc()
        {
            var c = (char)reader.Read();
            if (!MoveToNextToken())
                throw new FormatException("Unexpected EOF");
            var next = (char)reader.Read();
            switch(c)
            {
                case ' ':
                    switch(next)
                    {
                        case ' ':
                            break;
                        case '\t':
                            Call(GetLabel());
                            break;
                        case '\n':
                            Jump(GetLabel());
                            break;
                    }
                    break;
                case '\t':
                    switch(next)
                    {
                        case ' ':
                            var zeroTest = proc.Test(x => x == 0);
                            if (zeroTest.HasValue && zeroTest.Value)
                                Jump(GetLabel());
                            break;
                        case '\t':
                            var negTest = proc.Test(x => x < 0);
                            if (negTest.HasValue && negTest.Value)
                                Jump(GetLabel());
                            break;
                        case '\n':
                            Return();
                            break;
                    }
                    break;
                case '\n':
                    if (next == '\n')
                        proc.End();
                    else
                        throw new FormatException();
                    break;
            }
        }
        
        void IOProc()
        {
            var c = (char)reader.Read();
            if (!MoveToNextToken())
                throw new FormatException("Unexpected EOF");
            var next = (char)reader.Read();
            switch(c)
            {
                case ' ':
                    switch(next)
                    {
                        case ' ':
                            proc.OutputCharacter();
                            break;
                        case '\t':
                            proc.OutputNumber();
                            break;
                        case '\n':
                            throw new FormatException();
                    }
                    break;
                case '\t':
                    switch(next)
                    {
                        case ' ':
                            proc.ReadCharacter();
                            break;
                        case '\t':
                            proc.ReadNumber();
                            break;
                        case '\n':
                            throw new FormatException();
                    }
                    break;
                case '\n':
                    throw new FormatException();
            }
        }
        
        void StackManipulationPreProc()
        {
            var c = (char)reader.Read();
            char next;
            switch (c)
            {
                case ' ':
                    // push number (param: number)
                    Console.WriteLine("(pre) Push Number");
                    GetNumber();
                    break;
                case '\t':
                    if (!MoveToNextToken())
                        throw new FormatException("Unexpected EOF");
                    next = (char)reader.Read();
                    switch(next)
                    {
                        case ' ':
                            // Copy nth stack (param: number)
                            Console.WriteLine("(pre) Copy nth stack");
                            GetNumber();
                            break;
                        case '\n':
                            // Slide n items (param: number)
                            Console.WriteLine("(pre) Slide items");
                            GetNumber();
                            break;
                    }
                    break;
                case '\n':
                    if (!MoveToNextToken())
                        throw new FormatException("Unexpected EOF");
                    next = (char)reader.Read();
                    switch(next)
                    {
                        case ' ':
                            Console.WriteLine("(pre) Duplicate");
                            break;
                        case '\t':
                            Console.WriteLine("(pre) Swap");
                            break;
                        case '\n':
                            Console.WriteLine("(pre) Discard");
                            break;
                    }
                    break;
            }
        }
        
        void ArithmeticPreProc()
        {
            var c = (char)reader.Read();
            if (!MoveToNextToken())
                throw new FormatException("Unexpected EOF");
            var next = (char)reader.Read();
            switch (c)
            {
                case ' ':
                    switch(next)
                    {
                        case ' ':
                            Console.WriteLine("(pre) Add");
                            break;
                        case '\t':
                            Console.WriteLine("(pre) Sub");
                            break;
                        case '\n':
                            Console.WriteLine("(pre) Mul");
                            break;
                    }
                    break;
                case '\t':
                    switch(next)
                    {
                        case ' ':
                            Console.WriteLine("(pre) Div");
                            break;
                        case '\t':
                            Console.WriteLine("(pre) Mod");
                            break;
                    }
                    break;
                case '\n':
                    throw new FormatException();
            }
        }
        
        void HeapAccessPreProc()
        {
            var c = (char)reader.Read();
            switch(c)
            {
                case ' ':
                    // Store
                    Console.WriteLine("(pre) Store Heap");
                    break;
                case '\t':
                    Console.WriteLine("(pre) Retrieve Heap");
                    break;
            }
        }
        
        void FlowControlPreProc()
        {
            var c = (char)reader.Read();
            if (!MoveToNextToken())
                throw new FormatException("Unexpected EOF");
            var next = (char)reader.Read();
            if (c == ' ' && next == ' ')
            {
                var label = GetLabel();
                if (labels.ContainsKey(label))
                    throw new FormatException("LABEL must be unique.");
                labels.Add(label, stream.Position);
            }
        }
        
        void IOPreProc()
        {
            var c = (char)reader.Read();
            if (!MoveToNextToken())
                throw new FormatException("Unexpected EOF");
            var next = (char)reader.Read();
            switch(c)
            {
                case ' ':
                    switch(next)
                    {
                        case ' ':
                            Console.WriteLine("(pre) Output char");
                            break;
                        case '\t':
                            Console.WriteLine("(pre) Output number");
                            break;
                        case '\n':
                            throw new FormatException();
                    }
                    break;
                case '\t':
                    switch(next)
                    {
                        case ' ':
                            // read character
                            Console.WriteLine("(pre) Read char");
                            break;
                        case '\t':
                            // read number
                            Console.WriteLine("(pre) Read number");
                            break;
                        case '\n':
                            throw new FormatException();
                    }
                    break;
                case '\n':
                    throw new FormatException();
            }
        }

        // Flow Control
        void Call(string label)
        {
            Console.WriteLine("CALL " + label);
            callStack.Push(stream.Position);
            Jump(label);
        }

        void Jump(string label)
        {
            Console.WriteLine("JUMP " + label);
            if (!labels.ContainsKey(label))
                throw new FormatException("LABEL not found");
            stream.Seek(labels[label], SeekOrigin.Begin);
        }

        void Return()
        {
            Console.WriteLine("RETURN");
            if (callStack.Count == 0)
                throw new InvalidOperationException("Empty call stack");
            stream.Seek(callStack.Pop(), SeekOrigin.Begin);
        }
    }
}

