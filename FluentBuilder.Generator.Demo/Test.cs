using FluentBuilder;

namespace MyApp
{
    [FluentBuilder] // ❌ FB001: Cannot generate fluent builder for abstract class 'MyAbstractClass'
    public class MyAbstractClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
