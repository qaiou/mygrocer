using System;
using MYGROCER.Models;
using System.Collections.Generic;

namespace MYGROCER.Patterns
{
    // 1. The Observer Interface
    public interface IOrderObserver
    {
        void Update(Order order);
    }

    // 2. Concrete Observer
    public class NotificationObserver : IOrderObserver
    {
        public void Update(Order order)
        {
            Console.WriteLine($"\n=========================================");
            Console.WriteLine($"[OBSERVER NOTIFICATION TRIGGERED]");
            Console.WriteLine($"Customer ID: {order.CustomerId}");
            Console.WriteLine($"Order #{order.OrderId} placed successfully!");
            Console.WriteLine($"Total Amount: RM {order.TotalAmount}");
            Console.WriteLine($"=========================================\n");
        }
    }

    // 3. The Subject Interface
    public interface IOrderSubject
    {
        void Attach(IOrderObserver observer);
        void Detach(IOrderObserver observer);
        void Notify(Order order);
    }
}