﻿using Intex.Infastructure;
using System.Text.Json.Serialization;

namespace Intex.Models
{
    public class SessionCart : Cart
    {
        public static Cart GetCart(IServiceProvider services)
        {//object that provides custom support
            ISession session = services.GetRequiredService<IHttpContextAccessor>()
                .HttpContext?.Session;
            SessionCart cart = session?.GetJson<SessionCart>("Cart") ??
                new SessionCart();
            cart.Session = session;
            return cart;
        }
        [JsonIgnore]
        public ISession? Session { get; set; }
        public override void AddItem(Product prod,int quantity)
        {
            base.AddItem
                (prod, quantity);
            Session?.SetJson("Cart", this);
        }
        public override void RemoveLine(Product prod)
        {
            base.RemoveLine(prod);
            Session?.SetJson("Cart", this);

        }
        public override void Clear()
        {
            base.Clear();
            Session?.Remove("Cart");

        }
    }
}
