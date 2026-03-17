using FluentBuilder;

namespace Demo
{
    public class OrderBuilder { } // top‑level type, should not conflict

    public partial class Container
    {
        [FluentBuilder]
        public class Order { }
    }
}
