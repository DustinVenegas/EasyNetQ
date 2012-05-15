namespace EasyNetQ.Topology
{
    public class DirectExchange : Exchange
    {
        public DirectExchange(string name) : base(name)
        {
        }

        public override void Visit(ITopologyVisitor visitor)
        {
            visitor.CreateExchange(Name, ExchangeType.Direct);
            foreach (var binding in bindings)
            {
                binding.Visit(visitor);
            }
        }
    }
}