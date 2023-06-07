using Makaretu.Dns;

public interface IMdnsService {}

public class MdnsService :IMdnsService {
    public MdnsService(){

var mdns = new MulticastService();

            foreach (var a in MulticastService.GetIPAddresses())
            {
                Console.WriteLine($"IP address {a}");
            }

            mdns.QueryReceived += (s, e) =>
            {
                var names = e.Message.Questions
                    .Select(q => q.Name + " " + q.Type);
                Console.WriteLine($"got a query for {String.Join(", ", names)}");
            };
            mdns.AnswerReceived += (s, e) =>
            {
                var names = e.Message.Answers
                    .Select(q => q.Name + " " + q.Type)
                    .Distinct();
                Console.WriteLine($"got answer for {String.Join(", ", names)}");
            };
            mdns.NetworkInterfaceDiscovered += (s, e) =>
            {
                foreach (var nic in e.NetworkInterfaces)
                {
                    Console.WriteLine($"discovered NIC '{nic.Name}'");
                }
            };

            var sd = new ServiceDiscovery(mdns);
            sd.Advertise(new ServiceProfile("suite16", "_suite16._tcp", 5103));

            sd.Advertise(new ServiceProfile("ipfs1", "_ipfs-discovery._udp", 5010));
            sd.Advertise(new ServiceProfile("x1", "_xservice._tcp", 5011));
            sd.Advertise(new ServiceProfile("x2", "_xservice._tcp", 666));
            var z1 = new ServiceProfile("z1", "_zservice._udp", 5012);
            z1.AddProperty("foo", "bar");
            sd.Advertise(z1);

            mdns.Start();
    }
}