using System.Collections.Generic;
using MYGROCER.Models;

namespace MYGROCER.Patterns
{
    // 4. Concrete Subject
    public class OrderNotifier : IOrderSubject
    {
        // This list holds all the observers we want to notify
        private readonly List<IOrderObserver> _observers = new List<IOrderObserver>();

        public void Attach(IOrderObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IOrderObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(Order order)
        {
            // Loop through every observer and trigger their Update method
            foreach (var observer in _observers)
            {
                observer.Update(order);
            }
        }
    }
}