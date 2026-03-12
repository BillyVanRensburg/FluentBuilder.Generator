using FluentBuilder;

namespace Demo
{
    public partial class Container
    {
        // Example: This would cause FBBLD0006 error if uncommented
        // public class OrderBuilder { } // conflicting type

        [FluentBuilder]
        public class Order { }
    }
}
