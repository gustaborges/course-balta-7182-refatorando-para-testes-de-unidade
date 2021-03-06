using System;
using System.Collections.Generic;
using Flunt.Validations;
using Store.Domain.Enums;

namespace Store.Domain.Entities
{
    public class Order : Entity
    {
        public Order(Customer customer, decimal deliveryFee, DiscountVoucher promotionalCode)
        {
            Customer = customer;
            Number = Guid.NewGuid().ToString().Substring(0, 8);
            Date = DateTime.Now;
            DeliveryFee = deliveryFee;
            PromotionalCode = promotionalCode;
            Status = EOrderStatus.WaitingPayment;
            Items = new List<OrderItem>();
        }
        public Customer Customer { get; }
        public decimal DeliveryFee { get; }
        public DiscountVoucher PromotionalCode { get; }
        public EOrderStatus Status { get; private set; }
        public DateTime Date {get; }
        public string Number { get; }
        public IList<OrderItem> Items { get; private set;}

        public void AddItem(Product product, int quantity)
        {
            var item = new OrderItem(product, quantity);
            
            if(item.IsValid)
                Items.Add(item);
        }

        public decimal Total()        
        {
            decimal total = 0;
            foreach(var item in Items)
            {
                total += item.Total();
            }

            total += DeliveryFee;
            total -= PromotionalCode != null ? PromotionalCode.Value() : 0;

            return total;
        }

        public void Pay(decimal amount)
        {
            if(amount == Total())
                Status = EOrderStatus.WaitingDelivery;
        }

        public void Cancel() 
        {
            Status = EOrderStatus.Canceled;
        }

        public void Validate()
        {
            AddNotifications(new Contract<Order>()
                .Requires()
                .IsNotNull(Customer, nameof(Customer), "Cliente inválido")
                .IsGreaterOrEqualsThan(DeliveryFee, 0, nameof(DeliveryFee), "Valor de frete inválido")
                .IsGreaterThan(Items.Count, 0, nameof(Items), "Pedido vazio")
            );

            foreach (var item in Items)
            {
                AddNotifications(item.Notifications);
            }
        }
    }
}