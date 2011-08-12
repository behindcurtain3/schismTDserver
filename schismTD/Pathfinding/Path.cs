using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class Path : Stack<Cell>
    {
        public Path() { }
        public Path(Cell[] init) : base(init) { }
        public Path(Path p) :base(new Stack<Cell>(p)) { }
    }
}
