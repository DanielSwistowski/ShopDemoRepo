using DataAccessLayer.Models;
using System.Linq;
using System.Web;
using System.Collections.Generic;

namespace Service_Layer.Services
{
    public interface ICartService
    {
        void AddToCart(CartItem cartItem);
        void UpdateCartProductCount(int productId, int productCount);
        void RemoveFromCart(int productId);
        void EmptyCart();
        ShoppingCart GetCart();
        int GetProductCount(int productId);
    }

    public class CartService : ICartService
    {
        private const string CartSessionKey = "CartId";
        public ShoppingCart Cart { get; set; }

        private readonly HttpContextBase context;
        public CartService(HttpContextBase context)
        {
            this.context = context;
            Cart = CreateCart();
        }

        private ShoppingCart CreateCart()
        {
            ShoppingCart cart = (ShoppingCart)context.Session[CartSessionKey];
            if (cart == null)
            {
                cart = new ShoppingCart();
                context.Session[CartSessionKey] = cart;
            }
            return cart;
        }

        public void AddToCart(CartItem cartItem)
        {
            if(Cart.CartItems != null)
            {
                CartItem item = Cart.CartItems.Where(p => p.ProductId == cartItem.ProductId).SingleOrDefault();

                if (item != null)
                {
                    item.ProductCount = item.ProductCount + cartItem.ProductCount;
                }
                else
                {
                    Cart.CartItems.Add(cartItem);
                }
            }
            else
            {
                List<CartItem> itemsList = new List<CartItem>();
                itemsList.Add(cartItem);
                Cart.CartItems = itemsList;
            }
            
        }

        public void RemoveFromCart(int productId)
        {
            CartItem item = Cart.CartItems.Where(p => p.ProductId == productId).SingleOrDefault();
            if (item != null)
            {
                Cart.CartItems.Remove(item);
            }
        }

        public void UpdateCartProductCount(int productId, int productCount)
        {
            CartItem item = Cart.CartItems.Where(p => p.ProductId == productId).SingleOrDefault();

            if (item != null)
            {
                if(productCount == 0)
                {
                    Cart.CartItems.Remove(item);
                }
                else
                {
                    item.ProductCount = productCount;
                }
            }
        }

        public void EmptyCart()
        {
            if (Cart != null)
                context.Session.Clear();
        }

        public ShoppingCart GetCart()
        {
            return Cart;
        }

        public int GetProductCount(int productId)
        {
            if (Cart.CartItems != null)
            {
                var product = Cart.CartItems.Find(p => p.ProductId == productId);
                if (product != null)
                    return product.ProductCount;
            }
            return 0;
        }
    }
}