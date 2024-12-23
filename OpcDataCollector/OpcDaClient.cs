using Opc.Da;
using OpcCom;
using System;
using System.Collections.Generic;

public class OpcDaClient
{
    private Opc.Da.Server server;
    private Subscription subscription;
    private List<Item> items = new List<Item>();

    public void Connect(string serverUrl)
    {
        server = new Opc.Da.Server(new Factory(), null)
        {
            Url = new Opc.URL(serverUrl)
        };
        server.Connect();
        Console.WriteLine("成功连接到 OPC DA Server！");
    }

    public void AddItems(List<string> tagNames)
    {
        if (server == null || !server.IsConnected)
            throw new Exception("OPC DA Server 未连接！");

        SubscriptionState state = new SubscriptionState
        {
            Name = "SubscriptionGroup",
            Active = true,
            UpdateRate = 1000
        };
        subscription = (Subscription)server.CreateSubscription(state);

        foreach (var tagName in tagNames)
        {
            items.Add(new Item { ItemName = tagName });
        }

        subscription.AddItems(items.ToArray());
        Console.WriteLine("成功添加数据项！");
    }

    public List<ItemValueResult> ReadItems()
    {
        return new List<ItemValueResult>(server.Read(subscription.Items));
    }
}
